
using CompressionForce.Data.Repositories;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Abstractions.UnitOfWork;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Exceptions;
using CompressionForce.Services.DTOs.Requests;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Mapping;
using System.Text.Json;
namespace CompressionForce.Services.Batches
{


    public class BatchApplicationService : IBatchApplicationService
    {
        private readonly IBatchRepository _batchRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly ICurrentBatchRepository _currentRepo;
        private readonly IUnitOfWork _uow;

        public BatchApplicationService(
            IBatchRepository batchRepo,
            IRecipeRepository recipeRepo,
            ICurrentBatchRepository currentRepo,
            IUnitOfWork uow)
        {
            _batchRepo = batchRepo;
            _recipeRepo = recipeRepo;
            _currentRepo = currentRepo;
            _uow = uow;
        }

        public async Task AddBatchAsync(AddBatchRequest request)
        {
            await _uow.BeginAsync();

            try
            {
                // 1️⃣ Validate recipe exists
                var recipe = await _recipeRepo.GetByCodeAsync(request.RecipeCode);
                if (recipe == null)
                    throw new DomainException("Recipe not found");

                // 2️⃣ GLOBAL batch uniqueness (recipe & status agnostic)
                if (await _batchRepo.ExistsAsync(request.BatchCode))
                    throw new DomainException(
                        $"Batch '{request.BatchCode}' already exists."
                    );

                // 3️⃣ Create batch
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

                // 4️⃣ Create snapshot (DO NOT delete later)
                var currentBatch = new CurrentBatch
                {
                    BatchNumber = request.BatchCode,
                    Parameters = JsonSerializer.Serialize(recipe.Parameters),
                    DateTime = DateTime.UtcNow
                };

                await _currentRepo.AddAsync(currentBatch);

                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task EditBatchAsync(EditBatchRequest request)
        {
            await _uow.BeginAsync();

            try
            {
                // 1️⃣ Load batch
                Console.WriteLine("--------------------Recipe Code to be  fetched: " + (request.BatchCode != null ? request.BatchCode : "null"));
                var batch = await _batchRepo.GetByBatchCodeAsync(request.BatchCode);

                if (batch == null)
                    throw new InvalidOperationException("Batch not found");

                // 2️⃣ Guard: prevent editing deactivated batch
                if (batch.BatchStatus == "Deactivated")
                    throw new InvalidOperationException("Cannot edit a deactivated batch");

                // 3️⃣ Update quantity
                batch.BatchQty = request.BatchQty;

                // 4️⃣ Persist
                await _batchRepo.UpdateAsync(batch);

                // 5️⃣ Commit
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
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
                    throw new InvalidOperationException("Batch not found");

                if (batch.BatchStatus == "Deactivated")
                    throw new InvalidOperationException("Batch already deactivated");

                // Deactivate batch
                batch.BatchStatus = "Deactivated";
                await _batchRepo.UpdateAsync(batch);

                // Commit
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

    }
}
