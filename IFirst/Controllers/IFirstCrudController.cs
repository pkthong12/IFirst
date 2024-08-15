using Microsoft.AspNetCore.Mvc;
using First.EntityClasses;
using First.DatabaseSpecific;
using First.HelperClasses;
using First.FactoryClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using SD.LLBLGen.Pro.QuerySpec;
using First.Linq;
using SD.LLBLGen.Pro.LinqSupportClasses;
using Dtos.DtoClasses;
using IFirst.DTO;
using System.Data;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Aspose.Cells;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Routing.Template;
using IFirst.Const;
namespace IFirst.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IFirstCrudController : ControllerBase
    {
        private readonly DataAccessAdapter _DataAccessAdapter;

        private readonly ILogger<IFirstCrudController> _logger;

        public IFirstCrudController(ILogger<IFirstCrudController> logger)
        {
            _logger = logger;
            _DataAccessAdapter = new DataAccessAdapter(ConstDTO.CONNECTION);
        }


        [HttpGet]
        public async Task<IActionResult> QueryList()
        {
            var meta = new LinqMetaData(_DataAccessAdapter);
            var funcList = await (from p in meta.Func
                                  join e in meta.FuncRight on p.Id equals e.FuncId into pet
                                  select new Func
                                  {
                                      Id = p.Id,
                                      Name = p.Name,
                                      Path = p.Name,
                                      Status = p.Status,
                                      DateCreate = p.DateCreate,
                                      DateModify = p.DateModify,
                                      UserCreate = p.UserCreate,
                                      UserModify = p.UserModify,
                                  }).ToListAsync();
            return Ok(new
            {
                Innerbody = funcList,
                Message = "QUERY_LIST_SUCCESS",
                StatusCode = 200,
            });

        }
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var meta = new LinqMetaData(_DataAccessAdapter);
            var query = await meta.Func.SingleOrDefaultAsync(p => p.Id == id);
            if (query == null)
            {
                return Ok(new
                {
                    Innerbody = query,
                    Message = "ENTITY_NOT_FOUND",
                    StatusCode = 404,
                });
            }
            var joined = new Func
            {
                Id = query.Id,
                Name = query.Name,
                Path = query.Name,
                Status = query.Status,
                DateCreate = query.DateCreate,
                DateModify = query.DateModify,
                UserCreate = query.UserCreate,
                UserModify = query.UserModify,
            };
            return Ok(new
            {
                Innerbody = joined,
                Message = "SUCCESS",
                StatusCode = 200,
            });

        }
        [HttpPost]
        public async Task<IActionResult> Create(Func model)
        {
            try
            {
                await _DataAccessAdapter.SaveEntityAsync(new FuncEntity
                {
                    Path = model.Path,
                    Name = model.Name,
                    Status = model.Status!.Value,
                    DateCreate = DateTime.UtcNow,
                    UserCreate = 0,
                    UserModify = "",
                    DateModify = DateTime.UtcNow,
                }
            );
                return Ok(new
                {
                    Message = "CREATED_SUCCESS",
                    StatusCode = 200,
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Message = ex.Message,
                    StatusCode = 500,
                });
            }

        }
        [HttpPost]
        public async Task<IActionResult> Update(Func model)
        {
            try
            {
                var meta = new LinqMetaData(_DataAccessAdapter);
                var query = await meta.Func.SingleOrDefaultAsync(p => p.Id == model.Id);
                if (query == null)
                {
                    return Ok(new
                    {
                        Innerbody = query,
                        Message = "ENTITY_NOT_FOUND",
                        StatusCode = 404,
                    });
                }
                RelationPredicateBucket filterBucket = new RelationPredicateBucket(FuncFields.Id == model.Id);
                var joined = new FuncEntity
                {
                    Id = model.Id!.Value,
                    Path = model.Path,
                    Name = model.Name,
                    Status = model.Status!.Value,
                    DateCreate = DateTime.UtcNow,
                    UserCreate = model.UserCreate!.Value,
                    UserModify = "admin",
                    DateModify = DateTime.UtcNow,
                };
                await _DataAccessAdapter.UpdateEntitiesDirectlyAsync(joined, filterBucket);
                return Ok(new
                {
                    Message = "UPDATED_SUCCESS",
                    StatusCode = 200,
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Message = ex.Message,
                    StatusCode = 500,
                });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteIds(List<int> ids)
        {
            try
            {
                var meta = new LinqMetaData(_DataAccessAdapter);
                var query = meta.Func.Where(p => ids.Contains(p.Id)).AsQueryable();
                foreach (var e in query)
                {
                    _DataAccessAdapter.DeleteEntity(e);
                }
                return Ok(new
                {
                    Message = "DELETED_SUCCESS",
                    StatusCode = 200,
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Message = ex.Message,
                    StatusCode = 500,
                });
            }

        }
        [HttpPost]
        public async Task<IActionResult> Import(ImportParam model)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var base64string = model.Base64String.Replace("data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,", "");
            byte[] fileAsBytes = Convert.FromBase64String(base64string);
            DataSet dataSet = new DataSet();

            using (MemoryStream stream = new MemoryStream(fileAsBytes))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSetConfig = new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true, // Dòng thứ 3 sẽ là header
                            FilterRow = (rowReader) => rowReader.Depth > 1 // Bỏ qua 2 dòng đầu tiên
                        }
                    };

                    dataSet = reader.AsDataSet(dataSetConfig);
                }
            }
            if (dataSet.Tables.Count == 0)
            {
                return Ok(new { Status = 205, Message = "DATA_CAN_NOT_EMPTY" });
            }
            if (dataSet.Tables.Count > 0)
            {
                if (dataSet.Tables[0].Columns.Contains("Error")) dataSet.Tables[0].Columns.Remove("Error");
                if (dataSet.Tables[0].Columns.Contains("Detail")) dataSet.Tables[0].Columns.Remove("Detail");
                dataSet.Tables[0].Columns.Add("Error");
                dataSet.Tables[0].Columns.Add("Detail");
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    return Ok(new { Status = 205, Message = "DATA_CAN_NOT_EMPTY" });
                }
                List<EmployeeDTO> dtos = new List<EmployeeDTO>();
                bool flag = false;
                int stt = 0;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    bool err = false;
                    stt++;
                    row["Stt"] = stt;
                    if (row["Name"] == null || String.IsNullOrEmpty(row["Name"].ToString().Trim()))
                    {
                        row["Error"] = "X";
                        row["Detail"] = string.Join(Environment.NewLine, ["Bắt buộc nhập tên!", row["Detail"]]);
                        err = true;
                    }
                    if (row["Age"] == null || !int.TryParse(row["Age"].ToString().Trim(), out int resultAge))
                    {
                        row["Error"] = "X";
                        row["Detail"] = string.Join(Environment.NewLine, ["Tuổi là bắt buộc và phải là số nguyên!", row["Detail"]]);
                        err = true;
                    }
                    if (row["Address"] != null &&  row["Address"].ToString().Trim().Length > 200)
                    {
                        row["Error"] = "X";
                        row["Detail"] = string.Join(Environment.NewLine, ["Địa chỉ quá 200 ký tự!", row["Detail"]]);
                        err = true;
                    }
                    if (row["BirthDay"] != null && !DateTime.TryParse(row["BirthDay"].ToString().Trim(), out DateTime resultBirth))
                    {
                        row["Error"] = "X";
                        row["Detail"] = string.Join(Environment.NewLine, ["Ngày sinh chưa đúng định dạng!", row["Detail"]]);

                        err = true;
                    }
                    if (row["Mail"] != null && dtos.SingleOrDefault(p => p.Mail.ToLower() == row["Mail"].ToString().ToLower().Trim()) != null)
                    {
                        row["Error"] = "X";
                        row["Detail"] = string.Join(Environment.NewLine, ["Trùng email!", row["Detail"]]);

                        err = true;
                    }

                    if (!err)
                    {
                        dtos.Add(new EmployeeDTO
                        {
                            Mail = row["Mail"].ToString().Trim(),
                            Address = row["Address"].ToString().Trim(),
                            Age = int.Parse(row["Age"].ToString().Trim()),
                            BirthDay =DateTime.Parse(row["BirthDay"].ToString().Trim()),
                            Name = row["Name"].ToString().Trim()
                        });
                    }
                    else flag = true;
                }
                if (!flag) return Ok(new { Status = 200, Message = "IMPORT_SUCESS", Data = dtos });
                else
                {
                    var dtData = dataSet.Tables[0];
                    dtData.TableName = "DATA";
                    Workbook workbook = new Workbook("..\\IFirst\\Static\\IMPORT_Error.xlsx");

                    WorkbookDesigner designer = new WorkbookDesigner();
                    designer.Workbook = workbook;

                    designer.SetDataSource(dtData);

                    designer.Process();
                    // Xuất Workbook ra bộ nhớ
                    using (MemoryStream stream = new MemoryStream())
                    {
                        workbook.Save(stream, SaveFormat.Xlsx);
                        byte[] fileContents = stream.ToArray();
                        return Ok(new
                        {
                            Status = 204,
                            Message ="IMPORT_FAIL",
                            Memory = File(fileContents, "application/octet-stream", "import_error.xlsx")
                    });

                    }
                }
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ExportTemplate()
        {
            Workbook workbook = new Workbook("..\\IFirst\\Static\\TEMPLATE_IMPORT.xlsx");

            WorkbookDesigner designer = new WorkbookDesigner();
            designer.Workbook = workbook;
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.Save(stream, SaveFormat.Xlsx);
                byte[] fileContents = stream.ToArray();
                return Ok(new
                {
                    Status = 200,
                    Memory = File(fileContents, "application/octet-stream", "Template_import.xlsx")
                });

            }
        }
    }
}
  