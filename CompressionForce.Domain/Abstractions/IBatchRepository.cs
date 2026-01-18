using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IBatchRepository
    {
        Task<List<Batch>> GetByRecipeAsync(string recipeCode);
        Task<Batch> GetByBatchCodeAsync(string batchCode);
        Task AddAsync(Batch batch);
        Task UpdateAsync(Batch batch);
    }
}
