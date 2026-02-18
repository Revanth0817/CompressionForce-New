using CompressionForce.Data.Entities;
using CompressionForce.Domain.Entities;
using System.Text.Json;

namespace CompressionForce.Data.Mappers
{
    public static class RecipeMapper
    {
        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public static Recipe MapToDomain(RecipeEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var parameters = DeserializeParameters(entity.Parameters);

            return new Recipe(
                entity.RecipeCode,
                entity.RecipeName,
                parameters
            );
        }

        private static IReadOnlyList<RecipeParameter> DeserializeParameters(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<RecipeParameter>();

            try
            {
                return JsonSerializer.Deserialize<List<RecipeParameter>>(json, _jsonOptions)
                       ?? new List<RecipeParameter>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Invalid JSON stored in RecipeEntity.Parameters", ex);
            }
        }
    }
}
