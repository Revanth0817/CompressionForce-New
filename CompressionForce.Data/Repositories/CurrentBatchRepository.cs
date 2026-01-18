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
    public class CurrentBatchRepository : ICurrentBatchRepository
    {
        private readonly ApplicationDbContext _ctx;

        public CurrentBatchRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<CurrentBatch> GetByBatchCodeAsync(string batchCode) =>
            _ctx.CurrentBatches.FirstOrDefaultAsync(c => c.BatchCode == batchCode);

        public async Task AddAsync(CurrentBatch batch)
        {
            _ctx.CurrentBatches.Add(batch);
            await _ctx.SaveChangesAsync();
        }
    }
}
