namespace CompressionForce.Services.DTOs.Batch
{
    /// <summary>
    /// Aggregate DTO for the Batch page
    /// </summary>
    public class BatchDetailsDto
    {
        public string RecipeCode { get; set; }
        public string RecipeName { get; set; }

        public string BatchCode { get; set; }

        public int? BatchQty { get; set; }
        public int? GoodQty { get; set; }
        public int? RejectedQty { get; set; }

        public string BatchStatus { get; set; }
        public int? TabletQty { get; set; }

        /// <summary>
        /// JSON parameters from Recipes or CurrentBatches
        /// </summary>
        public string ParametersJson { get; set; }
    }
}
