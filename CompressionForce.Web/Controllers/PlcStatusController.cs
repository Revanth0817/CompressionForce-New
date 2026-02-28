using CompressionForce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcStatusController : ControllerBase
    {
        private readonly IPlcStatusService _svc;

        public PlcStatusController(IPlcStatusService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var vm = _svc.GetPlcStatus();
            return Ok(vm);
        }
    }
}