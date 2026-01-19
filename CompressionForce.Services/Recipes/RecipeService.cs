using CompressionForce.Data;
using CompressionForce.Data.Entities;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Exceptions;
using CompressionForce.Domain.Validation;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Mapping;
using CompressionForce.Services.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CompressionForce.Services.Batches;

namespace CompressionForce.Services.Recipes
{
    /// <summary>
    /// Implements all recipe-related business use cases.
    /// </summary>

    public class RecipeService : IRecipeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRecipeValidator _recipeValidator;
        private readonly LookupRecipeValidator _lookupValidator;
        private readonly ConfigRecipeValidator _configValidator;
        private readonly IBatchRepository _batchRepo;

        public RecipeService(
            ApplicationDbContext dbContext,
            IBatchRepository batchRepo,
            IRecipeValidator recipeValidator,
            LookupRecipeValidator lookupValidator,
            ConfigRecipeValidator configValidator)
        {
            _dbContext = dbContext;
            _batchRepo = batchRepo;
            _recipeValidator = recipeValidator;
            _lookupValidator = lookupValidator;
            _configValidator = configValidator;
        }

        public async Task<IReadOnlyList<string>> GetRecipeCodesAsync()
        {
            return await _dbContext.Recipes
                .OrderBy(r => r.RecipeCode)
                .Select(r => r.RecipeCode)
                .ToListAsync();
        }

        public async Task<Recipe?> GetByCodeAsync(string recipeCode)
        {
            var entity = await _dbContext.Recipes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecipeCode == recipeCode);

            return entity == null ? null : RecipeMapper.ToDomain(entity);
        }

        public async Task AddAsync(Recipe recipe, string user)
        {
            if (await _dbContext.Recipes.AnyAsync(r => r.RecipeCode == recipe.Code))
                throw new DomainException($"Recipe code '{recipe.Code}' already exists.");

            ValidateRecipe(recipe);

            var entity = RecipeMapper.ToEntity(recipe);
            _dbContext.Recipes.Add(entity);
            AddHistory(recipe.Code, "ADD", user, null, entity.Parameters);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Recipe recipe, string user)
        {
            var entity = await _dbContext.Recipes
                .FirstOrDefaultAsync(r => r.RecipeCode == recipe.Code);

            if (entity == null)
                throw new DomainException("Recipe does not exist.");

            ValidateRecipe(recipe);

            var oldParameters = entity.Parameters;

            entity.RecipeName = string.IsNullOrEmpty(recipe.Name) ? recipe.Code : recipe.Name;
            entity.Parameters = System.Text.Json.JsonSerializer.Serialize(recipe.Parameters);

            AddHistory(recipe.Code, "UPDATE", user, oldParameters, entity.Parameters);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(string recipeCode, string user)
        {
            // 🚨 Do NOT filter by status
            var batches = await _batchRepo.GetByRecipeAsync(recipeCode);

            if (batches.Any())
                throw new DomainException(
                    "Cannot delete recipe. Batches exist for this recipe."
                );

            var entity = await _dbContext.Recipes
                .FirstOrDefaultAsync(r => r.RecipeCode == recipeCode);

            if (entity == null)
                throw new DomainException("Recipe does not exist.");

            _dbContext.Recipes.Remove(entity);
            AddHistory(recipeCode, "DELETE", user, entity.Parameters, null);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string recipeCode)
            => await _dbContext.Recipes.AnyAsync(r => r.RecipeCode == recipeCode);

        public async Task<bool> ExistsByNameAsync(string recipeName)
            => await _dbContext.Recipes.AnyAsync(r => r.RecipeName == recipeName);

        private void ValidateRecipe(Recipe recipe)
        {
            var result = _recipeValidator.Validate(recipe); // code rules
            _configValidator.Validate(recipe, result);      // config rules
            _lookupValidator.Validate(recipe, result);      // enums membership

            if (!result.IsValid)
                throw new DomainException(string.Join(" | ", result.Errors));
        }

        private void AddHistory(
            string recipeCode,
            string action,
            string user,
            string? oldParams,
            string? newParams)
        {
            _dbContext.RecipeHistories.Add(new RecipeHistoryEntity
            {
                RecipeCode = recipeCode,
                Action = action,
                ChangedBy = user,
                ChangedAt = DateTime.UtcNow,
                OldParameters = oldParams,
                NewParameters = newParams
            });
        }
    }


}
