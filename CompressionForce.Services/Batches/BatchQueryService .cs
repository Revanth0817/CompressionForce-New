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
        public async Task<(string ProductName, string BatchNumber, int BatchQty)?>
    GetActiveBatchHeaderAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 GetActiveBatchHeaderAsync called");

                // ✅ Get all batches
                var allBatches = await _batchRepo.GetByRecipeAsync("");

                if (allBatches == null || !allBatches.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No batches found in database");
                    return null;
                }

                // ✅ Find most recent batch with "Active" OR "New-Active" status
                var activeBatch = allBatches
                    .Where(b => b.BatchStatus == "Active" || b.BatchStatus == "New-Active")
                    .OrderByDescending(b => b.DateTime)
                    .FirstOrDefault();

                if (activeBatch == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No Active or New-Active batches found");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Found Batch: {activeBatch.BatchCode}, Status: {activeBatch.BatchStatus}, DateTime: {activeBatch.DateTime}");

                // ✅ Get recipe
                var recipe = await _recipeRepo.GetByCodeAsync(activeBatch.RecipeCode);

                if (recipe == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Recipe not found: {activeBatch.RecipeCode}");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Recipe found: {recipe.Name}");
                System.Diagnostics.Debug.WriteLine($"✅ Returning: ProductName={recipe.Name}, BatchNumber={activeBatch.BatchCode}, BatchQty={activeBatch.BatchQty}");

                return (
                    recipe.Name,                // ProductName
                    activeBatch.BatchCode,      // BatchNumber
                    activeBatch.BatchQty ?? 0   // BatchQty
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in GetActiveBatchHeaderAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return null;
            }
        }

    }
}