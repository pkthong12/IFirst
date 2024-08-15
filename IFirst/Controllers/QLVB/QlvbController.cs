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
        }
        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            var x  = await _qlvbRepository.Sync();
            return Ok();
        }
    }
}
