using CompressionForce.Web.Models.Batches;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{


    public class BatchHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BatchHeaderVM vm)
        {
            return View("BatchHeader", vm);
        }
    }
}
