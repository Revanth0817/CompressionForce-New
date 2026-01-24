using CompressionForce.Web.Models.Batches;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{


    public class BatchSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BatchSummaryVM model)
        {
            return View("BatchSummary", model);
        }
    }
}
