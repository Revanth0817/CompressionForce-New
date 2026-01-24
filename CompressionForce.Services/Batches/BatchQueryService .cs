using CompressionForce.Domain.Abstractions;
using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Mappers;



namespace CompressionForce.Services.Batches
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

        /// <summary>
        /// Used to populate RecipeCode dropdown
        /// </summary>
        public async Task<IReadOnlyList<RecipeListItem>> GetRecipesfromRecipe()
        {
            var recipes = await _recipeRepo.GetAllAsync();
            if (recipes == null)
                return new List<RecipeListItem>();
            return recipes
                .Select(r => new RecipeListItem
                {
                    RecipeCode = r.Code,
                    RecipeName = r.Name
                })
                .ToList();
        }

        /// <summary>
        /// Used to populate BatchCode dropdown
        /// </summary>
        public async Task<IReadOnlyList<BatchListItem>> GetBatchesByRecipe(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode))
                return new List<BatchListItem>();

            var batches = await _batchRepo.GetByRecipeAsync(recipeCode);

            return batches
                .OrderBy(b => b.DateTime)
                .Select(b => new BatchListItem
                {
                    BatchCode = b.BatchCode,
                    BatchStatus = b.BatchStatus
                })
                .ToList();

        }

        /// <summary>
        /// Used when BatchCode dropdown changes
        /// </summary>
        public async Task<BatchDetailsDto> GetBatchDetails(string batchCode)
        {
            if (string.IsNullOrWhiteSpace(batchCode))
                return null;

            var batch = await _batchRepo.GetByBatchCodeAsync(batchCode);
            if (batch == null)
                return null;

            var currentBatch = await _currentRepo.GetByBatchNumberAsync(batchCode);

            if (currentBatch == null)
                return null;

            return new BatchDetailsDto
            {
                RecipeCode = batch.RecipeCode,
                BatchCode = batch.BatchCode,
                BatchQty = batch.BatchQty,
                GoodQty = batch.GoodQty,
                RejectedQty = batch.RejectionQty,
                BatchStatus = batch.BatchStatus,
                TabletQty = batch.TabletQty,

                ParametersJson = string.IsNullOrWhiteSpace(currentBatch?.Parameters)
                    ? null
                    : currentBatch.Parameters
            };
        }

        /// <summary>
        /// Used when no batches exist for the selected recipe
        /// </summary>
        public async Task<RecipeDetailsDto?> GetRecipeDetailsAsync(string recipeCode)
        {
            var recipe = await _recipeRepo.GetByCodeAsync(recipeCode);
            if (recipe == null) return null;

            return BatchMapper.ToDto(recipe);
        }

        public async Task<IReadOnlyList<BatchListItem>> GetActiveBatchesByRecipe(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode))
                return new List<BatchListItem>();

            var batches = await _batchRepo.GetByRecipeAsync(recipeCode);

            return batches
                .Where(b => b.BatchStatus != "Deactivated")
                .OrderBy(b => b.DateTime)
                .Select(b => new BatchListItem
                {
                    BatchCode = b.BatchCode,
                    BatchStatus = b.BatchStatus
                })
                .ToList();
        }

    }
}