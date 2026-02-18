namespace CompressionForce.Services.DTOs.Batch
{
    public class EditBatchRequest
    {
        public string BatchCode { get; set; } = default!;
        public int BatchQty { get; set; }
    }
}


