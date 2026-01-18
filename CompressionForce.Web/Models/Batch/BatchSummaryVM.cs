namespace CompressionForce.Web.Models.Batch
{
    public class BatchSummaryVM
    {
        public string BatchCode { get; set; }
        public int BatchQty { get; set; }
        public int GoodQty { get; set; }
        public int RejectedQty { get; set; }
        public string BatchStatus { get; set; }
        public int TabletQty { get; set; }
    }
}
