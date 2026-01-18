using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Data.Repositories
{


    public class BatchRepository : IBatchRepository
    {
        private readonly ApplicationDbContext _ctx;

        public BatchRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<List<Batch>> GetByRecipeAsync(string recipeCode) =>
            _ctx.Batches.Where(b => b.RecipeCode == recipeCode).ToListAsync();

        public Task<Batch> GetByBatchCodeAsync(string batchCode) =>
            _ctx.Batches.FirstAsync(b => b.BatchCode == batchCode);

        public async Task AddAsync(Batch batch)
        {
            _ctx.Batches.Add(batch);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Batch batch)
        {
            _ctx.Batches.Update(batch);
            await _ctx.SaveChangesAsync();
        }
    }
}
