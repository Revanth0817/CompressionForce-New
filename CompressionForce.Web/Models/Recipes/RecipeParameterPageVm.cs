using CompressionForce.Services.DTOs;
using System.Collections.Generic;

namespace CompressionForce.Web.Models.Recipes
{
    public class RecipeParameterPageVm
    {
        public List<string> RecipeCodes { get; set; } = new();
        public string? SelectedCode { get; set; }
        public RecipeDto? Recipe { get; set; }

        // Lookups
        public List<string> ToolTypes { get; set; } = new();
        public List<string> Treatments { get; set; } = new(); // multi-enum demo
        public List<string> AWC_ARTypes { get; set; } = new(); // multi-enum demo
        public List<string> ForceFeederRatioS1Types { get; set; } = new(); // multi-enum demo
        public List<string> ForceFeederRatioS2Types { get; set; } = new(); // multi-enum demo


        public bool HasRecipes => RecipeCodes.Count > 0;
    }
}
