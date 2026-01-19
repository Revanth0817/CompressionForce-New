using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CompressionForce.Data.Mappers;
namespace CompressionForce.Data.Repositories
{


    public class BatchRepository : IBatchRepository
    {
        private readonly ApplicationDbContext _ctx;

        public BatchRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        /*public Task<List<Batch>> GetByRecipeAsync(string recipeCode) =>
            _ctx.Batches.Where(b => b.RecipeCode == recipeCode).ToListAsync();*/

        public async Task<List<Batch>> GetByRecipeAsync(string recipeCode)
        {
            var entities = await _ctx.Batches
                .Where(b => b.RecipeCode == recipeCode)
                .ToListAsync();

            return entities
                .Select(BatchMapper.ToDomain)
                .ToList();
        }


        /*public Task<Batch> GetByBatchCodeAsync(string batchCode) =>
             _ctx.Batches.FirstAsync(b => b.BatchCode == batchCode);*/

        public async Task<Batch?> GetByBatchCodeAsync(string batchCode)
        {
            var entity = await _ctx.Batches
                .FirstOrDefaultAsync(b => b.BatchCode == batchCode);

            return entity == null ? null : BatchMapper.ToDomain(entity);
        }

        public async Task AddAsync(Batch batch)
        {
            _ctx.Batches.Add(BatchMapper.ToEntity(batch));
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Batch batch)
        {
            var entity = await _ctx.Batches
                .FirstOrDefaultAsync(b => b.BatchCode == batch.BatchCode);

            if (entity == null)
                throw new InvalidOperationException("Batch not found");

            BatchMapper.ToEntity(batch, entity);

            await _ctx.SaveChangesAsync();
        }


        public async Task<bool> ExistsAsync(string batchCode)
        {
            return await _ctx.Batches
                .AnyAsync(b => b.BatchCode == batchCode);
        }

        public async Task<bool> ExistsNonDeactivatedBatchAsync(string recipeCode)
        {
                        return await _ctx.Batches
                .AnyAsync(b => b.RecipeCode == recipeCode && b.BatchStatus != "Deactivated");
        }

    }
}
