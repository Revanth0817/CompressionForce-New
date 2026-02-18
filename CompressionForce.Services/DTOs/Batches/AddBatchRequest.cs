namespace CompressionForce.Services.DTOs.Batch
{
    public class AddBatchRequest
    {
        public string RecipeCode { get; set; }
        public string BatchCode { get; set; }
        public int BatchQty { get; set; }
        public int TabletQty { get; set; }

    }
}
