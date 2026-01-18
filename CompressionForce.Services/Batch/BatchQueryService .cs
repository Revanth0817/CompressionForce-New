using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Services.DTOs.Responses;
using CompressionForce.Services.Interfaces;

namespace CompressionForce.Services.Batch
{


    public class BatchQueryService : IBatchQueryService
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly IBatchRepository _batchRepo;
        private readonly ICurrentBatchRepository _currentRepo;

        public BatchQueryService(
            IRecipeRepository recipeRepo,
            IBatchRepository batchRepo,
            ICurrentBatchRepository currentRepo)
        {
            _recipeRepo = recipeRepo;
            _batchRepo = batchRepo;
            _currentRepo = currentRepo;
        }

        public async Task<BatchPageData> GetBatchPageDataAsync(string recipeCode)
        {
            var recipes = await _recipeRepo.GetAllAsync();
            var batches = string.IsNullOrEmpty(recipeCode)
                ? new List<Domain.Entities.Batch>()
                : await _batchRepo.GetByRecipeAsync(recipeCode);

            var active = batches.FirstOrDefault();
            string parametersJson = null;

            if (active != null)
            {
                var current = await _currentRepo.GetByBatchCodeAsync(active.BatchCode);
                parametersJson = current?.ParametersJson
                    ?? (await _recipeRepo.GetByCodeAsync(recipeCode))?.ParametersJson;
            }

            return new BatchPageData
            {
                Recipes = recipes.Select(r => new RecipeListItem
                {
                    RecipeCode = r.RecipeCode,
                    RecipeName = r.RecipeName
                }).ToList(),

                ActiveBatch = active == null ? null : new BatchDetails
                {
                    RecipeCode = active.RecipeCode,
                    BatchCode = active.BatchCode,
                    BatchQty = active.BatchQty,
                    GoodQty = active.GoodQty,
                    RejectedQty = active.RejQty,
                    BatchStatus = active.BatchStatus,
                    TabletQty = active.TabletQty
                },

                ParametersJson = parametersJson
            };
        }
    }
}
