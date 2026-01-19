using CompressionForce.Data.Entities;
using CompressionForce.Data.Mappers;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Data.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ApplicationDbContext _ctx;

        public RecipeRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }
        //public Task<List<Recipe>> GetAllAsync() => _ctx.RecipesforBatches.ToListAsync();
        public async Task<List<Recipe>> GetAllAsync()
        {
            Console.WriteLine("---------------------------Inside Recipe Respository 1---------------------------");
            var entities = await _ctx.RecipesforBatches.ToListAsync();
            Console.WriteLine("---------------------------Inside Recipe Respository 2---------------------------");
            var recipes = entities
                .Select(e => RecipeMapper.MapToDomain(e))
                .ToList();
            Console.WriteLine("---------------------------Inside Recipe Respository 3---------------------------");

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

