using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Services.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Interfaces
{
    /*public interface IBatchQueryService
    {
        Task<BatchSummaryDTO> GetBatchSummaryAsync(
            string recipeCode,
            string batchCode);
    }*/

    public interface IBatchQueryService
    {
        //Task<BatchPageData> GetBatchPageDataAsync();
        Task<IReadOnlyList<RecipeListItem>> GetRecipesfromRecipe();
        Task<IReadOnlyList<BatchListItem>> GetBatchesByRecipe(string recipeCode);
        Task<BatchDetails> GetBatchDetails(string batchCode);
        Task<RecipeDetailsDto?> GetRecipeDetailsAsync(string recipeCode);
        Task<IReadOnlyList<BatchListItem>> GetActiveBatchesByRecipe(string recipeCode);

    }
}