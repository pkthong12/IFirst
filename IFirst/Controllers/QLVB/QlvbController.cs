using Microsoft.AspNetCore.Mvc;

namespace IFirst.Controllers.QLVB
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QlvbController : Controller
    {
        private readonly IQlvbRepository _qlvbRepository;
        private readonly IWebHostEnvironment _env;
        public QlvbController(
            IWebHostEnvironment env)
        {
            _env = env;
            _qlvbRepository = new QlvbRepository();
        }
        [HttpPost]
        public async Task<IActionResult> Save(DateTime dateModify)
        {
            string location = Path.Combine(_env.ContentRootPath,"Static","FileTemplate");
            var file = await _qlvbRepository.Save(location,dateModify);
            return File(file, "application/octet-stream", "2C_TCTW_98.doc");
        }
    }
}
