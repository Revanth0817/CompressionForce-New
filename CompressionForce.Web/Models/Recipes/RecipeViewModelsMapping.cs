
using CompressionForce.Domain.Entities;
using CompressionForce.Services.DTOs;

namespace CompressionForce.Web.Models.Recipes
{
    public static class RecipeViewModelsMapping
    {
        public static RecipeDto ToDto(Recipe recipe)
        {
            return new RecipeDto
            {
                Code = recipe.Code,
                Name = recipe.Name,
                Parameters = recipe.Parameters?.ToList() ?? new List<RecipeParameter>()
            };
        }

        public static Recipe ToDomain(AddEditRecipeVm vm)
            => new Recipe(vm.Code, vm.Name, vm.Parameters);


        public static AddEditRecipeVm ToAddEditVm(Recipe recipe, List<string> toolTypes, List<string> treatments, List<string> awc_arTypes, List<string> forceFeederRatioS1Types, List<string> forceFeederRatioS2Types, List<string> recipeTypes)
        {
            return new AddEditRecipeVm
            {
                Code = recipe.Code,
                Name = recipe.Name,
                Parameters = recipe.Parameters?.ToList() ?? new List<RecipeParameter>(),
                ToolTypes = toolTypes,
                Treatments = treatments,
                AWC_ARTypes = awc_arTypes,
                ForceFeederRatioS1Types = forceFeederRatioS1Types,
                ForceFeederRatioS2Types = forceFeederRatioS1Types,
                RecipeTypes = recipeTypes
            };
        }

        public static DeleteRecipeVm ToDeleteVm(Recipe recipe)
        {
            return new DeleteRecipeVm
            {
                Code = recipe.Code,
                Name = recipe.Name,
                Parameters = recipe.Parameters?.ToList() ?? new List<RecipeParameter>()
            };
        }
    }
}
