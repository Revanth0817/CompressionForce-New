using CompressionForce.Web.Models.Batch;
using CompressionForce.Web.Models;
using System.Text.Json;

namespace CompressionForce.Web.Mapping
{
    public static class RecipeParametersMapper
    {
        public static RecipeParametersVM FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<RecipeParametersVM>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
