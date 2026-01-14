using CompressionForce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        // helpers for requirements (3,5,8,10,12)
        Task<bool> ExistsByCodeAsync(string recipeCode);
        Task<bool> ExistsByNameAsync(string recipeName);

    }
}
