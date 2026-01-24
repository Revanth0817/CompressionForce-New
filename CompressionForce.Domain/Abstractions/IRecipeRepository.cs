using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{


    public interface IRecipeRepository
    {
        Task<List<Recipe>> GetAllAsync();
        Task<Recipe> GetByCodeAsync(string recipeCode);
    }
}
