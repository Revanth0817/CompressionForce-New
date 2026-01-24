namespace CompressionForce.Web.Models.Batches
{
    public class BatchPageVM
    {
        public string? SelectedRecipeCode { get; set; } = string.Empty;
        public List<RecipeVM>? Recipes { get; set; } = new List<RecipeVM>();
        public string? SelectedBatchCode { get; set; } = string.Empty;
        public List<BatchVM>? Batches { get; set; } = new List<BatchVM>();
        public BatchSummaryVM? Summary { get; set; } = new BatchSummaryVM();
        public BatchRecipeParametersVM? Parameters { get; set; } = new BatchRecipeParametersVM();

        public BatchPageVM(
            List<RecipeVM>? recipes = null,
            List<BatchVM>? batches = null,
            BatchSummaryVM? batchsummary = null,
            BatchRecipeParametersVM? reciparameters = null

            )
        {
            Recipes = recipes ?? new List<RecipeVM>();
            Batches = batches ?? new List<BatchVM>();
            Summary = batchsummary ?? new BatchSummaryVM();
            Parameters = reciparameters ?? new BatchRecipeParametersVM();
        }
    }
}