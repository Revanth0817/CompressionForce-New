using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IBatchRepository
    {
        Task<List<Batch>> GetByRecipeAsync(string recipeCode);
        Task<Batch> GetByBatchCodeAsync(string batchCode);
        Task AddAsync(Batch batch);
        Task UpdateAsync(Batch batch);
        Task<bool> ExistsAsync(string batchCode);
        Task<bool> ExistsNonDeactivatedBatchAsync(string recipeCode);
        Task<List<Batch>> GetAllAsync();
    }
}
