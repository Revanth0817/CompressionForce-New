using CompressionForce.Web.Models.Batch;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{


    public class BatchSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BatchSummaryVM model)
        {
            return View(model);
        }
    }
}
