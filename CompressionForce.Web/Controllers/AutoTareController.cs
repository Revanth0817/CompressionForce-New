using CompressionForce.Services.Interfaces;
using CompressionForce.Web.Mapping;
using Microsoft.AspNetCore.Mvc;


namespace CompressionForce.Web.Controllers
{


    public sealed class AutoTareController : Controller
    {
        private readonly IAutoTareService _service;

        public AutoTareController(IAutoTareService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult AutoTare()
        {
            var dto = _service.GetSnapshot();
            var vm = dto.ToVm();
            return View(vm);
        }
    }
}
