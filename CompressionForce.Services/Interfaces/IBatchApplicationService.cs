using CompressionForce.Services.DTOs.Batch;

namespace CompressionForce.Services.Interfaces
{
    public interface IBatchApplicationService
    {
        Task AddBatchAsync(AddBatchRequest request);
        Task EditBatchAsync(EditBatchRequest request);
        Task DeactivateBatchAsync(DeactivateBatchRequest request);
    }
}
