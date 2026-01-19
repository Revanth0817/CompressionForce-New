// RecipeParametersViewComponent
using CompressionForce.Web.Models.Batches;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{
    public class RecipeParametersViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BatchRecipeParametersVM vm)
        {
            return View("RecipeParameters", vm ?? new BatchRecipeParametersVM());
        }
    }
}




