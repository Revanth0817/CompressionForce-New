using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Abstractions.UnitOfWork;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Services.Exceptions;
using CompressionForce.Services.Interfaces;
using System.Text.Json;
namespace CompressionForce.Services.Batches
{


    public class BatchApplicationService : IBatchApplicationService
    {
        private readonly IBatchRepository _batchRepo;
        private readonly IBatchHistoryRepository _batcHistoryhRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly ICurrentBatchRepository _currentRepo;
        private readonly IUnitOfWork _uow;

        public BatchApplicationService(
            IBatchRepository batchRepo,
            IRecipeRepository recipeRepo,
            ICurrentBatchRepository currentRepo,
            IBatchHistoryRepository batchHistoryRepository,
            IUnitOfWork uow)
        {
            _batchRepo = batchRepo;
            _recipeRepo = recipeRepo;
            _currentRepo = currentRepo;
            _batcHistoryhRepo = batchHistoryRepository;
            _uow = uow;
        }

        public async Task AddBatchAsync(AddBatchRequest request)
        {
            await _uow.BeginAsync();

            try
            {
                // Validate recipe exists
                var recipe = await _recipeRepo.GetByCodeAsync(request.RecipeCode);
                if (recipe == null)
                    throw new ServiceException("Recipe not found");

                // GLOBAL batch uniqueness (recipe & status agnostic)
                if (await _batchRepo.ExistsAsync(request.BatchCode))
                    throw new ServiceException(
                        $"Batch '{request.BatchCode}' already exists."
                    );

                // Create batch
                var batch = new Batch
                {
                    RecipeCode = request.RecipeCode,
                    BatchCode = request.BatchCode,
                    BatchQty = request.BatchQty,
                    TabletQty = request.TabletQty,
                    BatchStatus = "New - Active",
                    UserName = "System",
                    DateTime = DateTime.UtcNow
                };

                await _batchRepo.AddAsync(batch);

                // Create snapshot (DO NOT delete later)
                var currentBatch = new CurrentBatch
                {
                    BatchNumber = request.BatchCode,
                    Parameters = JsonSerializer.Serialize(recipe.Parameters),
                    DateTime = DateTime.UtcNow
                };

                await _currentRepo.AddAsync(currentBatch);

                await _batcHistoryhRepo.EntryAsync(batch, "ADD");

                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw new ServiceException("Adding Batch not successful");
            }
        }

        public async Task EditBatchAsync(EditBatchRequest request)
        {
            await _uow.BeginAsync();

            try
            {
                // Load batch
                var batch = await _batchRepo.GetByBatchCodeAsync(request.BatchCode);

                if (batch == null)
                    throw new ServiceException("Batch not found");

                // Guard: prevent editing deactivated batch
                if (batch.BatchStatus == "Deactivated")
                    throw new ServiceException("Cannot edit a deactivated batch");

                // Update quantity
                batch.BatchQty = request.BatchQty;

                // Persist
                await _batchRepo.UpdateAsync(batch);

                await _batcHistoryhRepo.EntryAsync(batch, "EDIT");

                // Commit
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw new ServiceException("Editing Batch not successful");
            }
        }

        public async Task DeactivateBatchAsync(DeactivateBatchRequest request)
        {
            await _uow.BeginAsync();

            try
            {
                // Load batch
                var batch = await _batchRepo.GetByBatchCodeAsync(request.BatchCode);

                if (batch == null)
                    throw new ServiceException("Batch not found");

                if (batch.BatchStatus == "Deactivated")
                    throw new ServiceException("Batch already deactivated");

                // Deactivate batch
                batch.BatchStatus = "Deactivated";
                await _batchRepo.UpdateAsync(batch);

                await _batcHistoryhRepo.EntryAsync(batch, "DEACTIVATE");

                // Commit
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw new ServiceException("Deactivating Batch not successful");
            }
        }

    }
}
