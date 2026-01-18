using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Task<List<Recipe>> GetAllAsync() =>_ctx.Recipes.ToListAsync();

        public Task<Recipe> GetByCodeAsync(string recipeCode) => _ctx.Recipes.FirstAsync(r => r.RecipeCode == recipeCode);
    }
}

