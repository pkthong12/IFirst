using Aspose.Words;
using Aspose.Words.Pdf2Word.FixedFormats;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using First.DatabaseSpecific;
using First.EntityClasses;
using First.Linq;
using IFirst.Const;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO.Compression;

namespace IFirst.Controllers.QLVB
{
    public class QlvbRepository : IQlvbRepository
    {
        private readonly DataAccessAdapter _DataAccessAdapter;
        public QlvbRepository()
        {
            _DataAccessAdapter = new DataAccessAdapter(ConstDTO.CONNECTION);
        }
        public async Task<object> Sync()
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> Save(string location, DateTime dateModify)
        {
            var metaData = new LinqMetaData(_DataAccessAdapter);
            List<JsonDataSampleEntity> jsonData = metaData.JsonDataSample
                .Where(x => x.UpdatedDate >= dateModify).Take(10)
                .ToList();
            string outputPath = Path.Combine(Path.GetTempPath(), "Output.docx");

            // Tạo một tài liệu Word mới từ file mẫu
            byte[] result = [];
            if (jsonData.Any())
            {
                jsonData.ForEach(async x =>
                {
                    JObject jObject = JObject.Parse(x.JsonData);
                    var jArray = new JArray { jObject };
                    if (!string.IsNullOrEmpty(x.FilePath))
                    {
                        var absolutePath = Path.Combine(location, x.FilePath);
                        File.Copy(absolutePath, outputPath, true);
                        DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(jArray.ToString());
                        Aspose.Words.Document doc = new Aspose.Words.Document(absolutePath);
                        doc.MailMerge.Execute(dataTable);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            doc.Save(stream, SaveFormat.Docx);
                            byte[] fileContents = stream.ToArray();
                            using (MemoryStream stream1 = new MemoryStream(fileContents))
                            {
                                using (WordprocessingDocument doc1 = WordprocessingDocument.Open(stream1, true))
                                {
                                    foreach (var section in doc1.MainDocumentPart!.Document.Descendants<SectionProperties>())
                                    {
                                        section.RemoveAllChildren<HeaderReference>();
                                        section.RemoveAllChildren<FooterReference>();
                                    }
                                    MainDocumentPart mainPart = doc1.MainDocumentPart;
                                    int currentYear = DateTime.Now.Year;

                                    foreach (var textElement in mainPart.Document.Descendants<Text>())
                                    {
                                        if (textElement.Text.Contains("Evaluation Only. Created with Aspose.Words. Copyright 2003-" + currentYear + " Aspose Pty Ltd."))
                                        {
                                            textElement.Text = textElement.Text.Replace("Evaluation Only. Created with Aspose.Words. Copyright 2003-" + currentYear + " Aspose Pty Ltd.", string.Empty);
                                        }
                                        if (textElement.Text.Contains("This document was truncated here because it was created in the Evaluation Mode."))
                                        {
                                            textElement.Text = textElement.Text.Replace("This document was truncated here because it was created in the Evaluation Mode.", string.Empty);
                                        }
                                    }
                                }
                                // Convert the cleaned document to PDF
                                using (MemoryStream pdfStream = new MemoryStream())
                                {
                                    doc.Save(pdfStream, SaveFormat.Pdf);
                                    result = pdfStream.ToArray();
                                }
                            }
                        }
                        string fileName = x.FilePath + "-" + x.Id+".pdf";
                        File.WriteAllBytes(ConstDTO.STATIC_LOCATION_FILE_RESULT + fileName, result);

                        try
                        {
                            await _DataAccessAdapter.SaveEntityAsync(new QlvbdataEntity
                            {
                                JsonData = x.JsonData,
                                Name = fileName,
                                AbsolutePath = ConstDTO.STATIC_LOCATION_FILE_RESULT + fileName,
                                RefKey = x.Id,
                            });
                        }catch(Exception ex)
                        {
                            throw new Exception();
                        }
                    }
                });
                return result;
            }

            return [];
        }

    }
}
