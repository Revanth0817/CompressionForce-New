using CompressionForce.Data.Mappers;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Data.Repositories
{ 
    public class CurrentBatchRepository : ICurrentBatchRepository
    {
        private readonly ApplicationDbContext _ctx;

        public CurrentBatchRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<CurrentBatch?> GetByBatchCodeAsync(string batchCode)
        {
            var entity = await _ctx.CurrentBatches
                .FirstOrDefaultAsync(c => c.BatchNumber == batchCode);

            return entity == null ? null : BatchMapper.ToDomain(entity);
        }

        public async Task<CurrentBatch?> GetByBatchNumberAsync(string batchNumber)
        {
            var entity = await _ctx.CurrentBatches
                .FirstOrDefaultAsync(x => x.BatchNumber == batchNumber);

            return entity == null ? null : BatchMapper.ToDomain(entity);
        }

        public async Task AddAsync(CurrentBatch batch)
        {
            _ctx.CurrentBatches.Add(BatchMapper.ToEntity(batch));
            await _ctx.SaveChangesAsync();
        }
        public async Task DeleteAsync(CurrentBatch batch)
        {
            var entity = BatchMapper.ToEntity(batch);
            _ctx.CurrentBatches.Remove(entity);
            await _ctx.SaveChangesAsync();
        }
        public async Task UpdateAsync(CurrentBatch batch)
        {
            var entity = await _ctx.CurrentBatches
                .FirstOrDefaultAsync(cb => cb.BatchNumber == batch.BatchNumber);

            if (entity == null)
                throw new InvalidOperationException("CurrentBatch not found");

            BatchMapper.ToEntity(batch, entity);

            await _ctx.SaveChangesAsync();
        }
        public async Task DeleteByBatchCodeAsync(string batchCode)
        {
            var entity = await _ctx.CurrentBatches
                .FirstOrDefaultAsync(cb => cb.BatchNumber == batchCode);

            if (entity != null)
            {
                _ctx.CurrentBatches.Remove(entity);
                await _ctx.SaveChangesAsync();
            }
        }

    }
}
