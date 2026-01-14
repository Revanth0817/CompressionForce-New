
using CompressionForce.Domain.Entities;
using System.Collections.Generic;

namespace CompressionForce.Web.Models.Recipes
{
    public class DeleteRecipeVm
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<RecipeParameter> Parameters { get; set; } = new();
    }
}

