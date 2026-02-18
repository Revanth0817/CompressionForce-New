
using CompressionForce.Domain.Entities;

namespace CompressionForce.Web.Models.Recipes
{
    public class AddEditRecipeVm
    {

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;


        public List<RecipeParameter> Parameters { get; set; } = new();

        public List<string> ToolTypes { get; set; } = new();
        public List<string> Treatments { get; set; } = new();
        public List<string> AWC_ARTypes { get; set; } = new();
        public List<string> ForceFeederRatioS1Types { get; set; } = new();
        public List<string> ForceFeederRatioS2Types { get; set; } = new();
        public List<string> RecipeTypes { get; set; } = new();

    }
}
