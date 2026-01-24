using CompressionForce.Services.DTOs.Batch;
//using CompressionForce.Services.DTOs.Responses;

namespace CompressionForce.Services.Interfaces
{
    public interface IBatchQueryService
    {

        Task<IReadOnlyList<RecipeListItem>> GetRecipesfromRecipe();
        Task<IReadOnlyList<BatchListItem>> GetBatchesByRecipe(string recipeCode);
        Task<BatchDetailsDto> GetBatchDetails(string batchCode);
        Task<RecipeDetailsDto?> GetRecipeDetailsAsync(string recipeCode);
        Task<IReadOnlyList<BatchListItem>> GetActiveBatchesByRecipe(string recipeCode);

    }
}