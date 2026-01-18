using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.DTOs.Requests;
using CompressionForce.Services.Interfaces;

namespace CompressionForce.Services.Batch
{


    public class BatchApplicationService : IBatchApplicationService
    {
        private readonly IBatchRepository _batchRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly ICurrentBatchRepository _currentRepo;

        public BatchApplicationService(
            IBatchRepository batchRepo,
            IRecipeRepository recipeRepo,
            ICurrentBatchRepository currentRepo)
        {
            _batchRepo = batchRepo;
            _recipeRepo = recipeRepo;
            _currentRepo = currentRepo;
        }

        public async Task AddBatchAsync(AddBatchRequest request)
        {
            var recipe = await _recipeRepo.GetByCodeAsync(request.RecipeCode);

            var batch = new Batch
            {
                RecipeCode = request.RecipeCode,
                BatchCode = request.BatchCode,
                BatchQty = request.BatchQty,
                TabletQty = request.TabletQty,
                BatchStatus = "New - Active",
                DateTime = DateTime.UtcNow
            };

            await _batchRepo.AddAsync(batch);

            await _currentRepo.AddAsync(new CurrentBatch
            {
                BatchCode = request.BatchCode,
                ParametersJson = recipe.ParametersJson,
                DateTime = DateTime.UtcNow
            });
        }

        public async Task EditBatchAsync(EditBatchRequest request)
        {
            var batch = await _batchRepo.GetByBatchCodeAsync(request.BatchCode);
            batch.BatchQty = request.BatchQty;
            await _batchRepo.UpdateAsync(batch);
        }

        public async Task DeactivateBatchAsync(DeactivateBatchRequest request)
        {
            var batch = await _batchRepo.GetByBatchCodeAsync(request.BatchCode);
            batch.BatchStatus = "Deactivated";
            await _batchRepo.UpdateAsync(batch);
        }
    }
}

namespace CompressionForce.Services.Services
{
}
