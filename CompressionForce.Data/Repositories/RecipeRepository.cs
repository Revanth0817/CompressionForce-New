using CompressionForce.Data.Mappers;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Data.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ApplicationDbContext _ctx;

        public RecipeRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<List<Recipe>> GetAllAsync()
        {
            var entities = await _ctx.RecipesforBatches
                        .OrderBy(r => r.RecipeCode)
                        .ToListAsync();

            var recipes = entities
                .Select(e => RecipeMapper.MapToDomain(e))
                .ToList();

            return recipes;
        }

        public async Task<Recipe> GetByCodeAsync(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode))
                return null;
            var entity = await _ctx.Recipes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecipeCode == recipeCode);

            return RecipeMapper.MapToDomain(entity);
        }
    }
}

