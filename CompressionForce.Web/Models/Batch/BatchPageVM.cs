using System.Collections.Generic;

namespace CompressionForce.Web.Models.Batch
{
    public class BatchPageVM
    {
        public string SelectedRecipeCode { get; set; }
        public List<RecipeVM> Recipes { get; set; }
        public BatchSummaryVM Summary { get; set; }
        public RecipeParametersVM Parameters { get; set; }
    }
}