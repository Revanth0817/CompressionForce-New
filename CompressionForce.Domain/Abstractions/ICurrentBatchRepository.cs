using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface ICurrentBatchRepository
    {
        Task<CurrentBatch?> GetByBatchNumberAsync(string batchNumber);
        Task<CurrentBatch?> GetByBatchCodeAsync(string batchCode);
        Task AddAsync(CurrentBatch batch);
        Task DeleteAsync(CurrentBatch batch);
        Task UpdateAsync(CurrentBatch batch);
        Task DeleteByBatchCodeAsync(string batchCode);
    }

}

