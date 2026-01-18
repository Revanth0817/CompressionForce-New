using CompressionForce.Web.Models.Batch;
using CompressionForce.Services.DTOs.Batch;

namespace CompressionForce.Web.Mapping
{
    public static class BatchSummaryMapper
    {
        public static BatchSummaryVM ToVM(BatchSummaryDTO dto)
        {
            if (dto == null) return null;

            return new BatchSummaryVM
            {
                BatchCode = dto.BatchCode,
                BatchQty = dto.BatchQty,
                GoodQty = dto.GoodQty,
                RejectedQty = dto.RejectedQty,
                BatchStatus = dto.BatchStatus,
                TabletQty = dto.TabletQty
            };
        }
    }
}
