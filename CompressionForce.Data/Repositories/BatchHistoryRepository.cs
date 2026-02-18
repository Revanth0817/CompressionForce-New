using CompressionForce.Data.Entities;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
namespace CompressionForce.Data.Repositories
{


    public class BatchHistoryRepository : IBatchHistoryRepository
    {
        private readonly ApplicationDbContext _ctx;

        public BatchHistoryRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task EntryAsync(Batch batch, string eventname)
        {
            _ctx.BatchHistories.Add(new BatchHistoryEntity()
            {
                DateTime = batch.DateTime,
                RecipeCode = batch.RecipeCode,
                BatchCode = batch.BatchCode,
                UserName = batch.UserName,
                BatchSize = batch.BatchQty,
                EReventType = eventname
            });
            await _ctx.SaveChangesAsync();
        }

    }
}
