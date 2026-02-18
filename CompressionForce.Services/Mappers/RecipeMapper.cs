using CompressionForce.Data.Entities;
using CompressionForce.Domain.Entities;
using System.Text.Json;

namespace CompressionForce.Services.Mappers
{
    /// <summary>
    /// Maps between EF entities and domain models.
    /// </summary>
    public static class RecipeMapper
    {
        public static Recipe ToDomain(RecipeEntity entity)
        {
            var parameters = JsonSerializer
                .Deserialize<List<RecipeParameter>>(entity.Parameters)
                ?? new List<RecipeParameter>();

            return new Recipe(
                entity.RecipeCode,
                entity.RecipeName,
                parameters);
        }

        public static RecipeEntity ToEntity(Recipe recipe)
        {
            return new RecipeEntity
            {
                RecipeCode = recipe.Code,
                RecipeName = string.IsNullOrEmpty(recipe.Name) ? recipe.Code : recipe.Name, //if recipe.Name is either empty or null, set it to recipe.Code,
                Parameters = JsonSerializer.Serialize(recipe.Parameters)
            };
        }
    }

}
