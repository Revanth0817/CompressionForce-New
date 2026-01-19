using System.Text.Json;
using CompressionForce.Web.Models.Batches;
namespace CompressionForce.Web.Mapping
{
    public static class RecipeParametersMapper
    {
        public static BatchRecipeParametersVM FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<BatchRecipeParametersVM>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
