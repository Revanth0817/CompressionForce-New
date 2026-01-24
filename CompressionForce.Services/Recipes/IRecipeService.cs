using CompressionForce.Domain.Entities;

namespace CompressionForce.Services.Recipes
{
    /// <summary>
    /// Application-level use cases for recipe management.
    /// </summary>
    public interface IRecipeService
    {
        Task<IReadOnlyList<string>> GetRecipeCodesAsync();
        Task<Recipe?> GetByCodeAsync(string recipeCode);

        Task AddAsync(Recipe recipe, string user);
        Task UpdateAsync(Recipe recipe, string user);
        Task DeleteAsync(string recipeCode, string user);


        // helpers
        Task<bool> ExistsByCodeAsync(string recipeCode);
        Task<bool> ExistsByNameAsync(string recipeName);

    }
}
