using CompressionForce.Web.Models.Batch;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.ViewComponents
{

    public class RecipeParametersViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return View(null);

            var vm = RecipeParametersVM.FromJson(json);
            return View(vm);
        }
    }
}



