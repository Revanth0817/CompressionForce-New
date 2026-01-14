using CompressionForce.Domain.Entities;

namespace CompressionForce.Web.Models.Recipes
{
    public class RecipeParameterViewModel
    {
        // For the dropdown list
        public List<string> AllRecipeCodes { get; set; } = new List<string>();

        // The full data for the recipe currently displayed
        public Recipe SelectedRecipe { get; set; }
    }
}
