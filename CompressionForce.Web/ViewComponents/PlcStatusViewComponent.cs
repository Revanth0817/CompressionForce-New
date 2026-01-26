using Microsoft.AspNetCore.Mvc;
using CompressionForce.Services.Interfaces;


namespace CompressionForce.Web.ViewComponents
{
    public class PlcStatusViewComponent : ViewComponent
    {
        private readonly IPlcStatusService _plcStatusService;

        public PlcStatusViewComponent(IPlcStatusService plcStatusService)
        {
            _plcStatusService = plcStatusService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _plcStatusService.GetPlcStatus();
            return View(model);
        }
    }
}
