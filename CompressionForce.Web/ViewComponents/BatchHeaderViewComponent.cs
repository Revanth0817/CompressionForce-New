using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{


    public class BatchHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
