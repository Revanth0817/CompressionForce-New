using CompressionForce.Data.Entities;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Services.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CompressionForce.Services.Mapping
{

    public static class BatchMapper
    {
        public static Batch ToDomain(BatchSummaryDTO entity)
        {
            return new Batch()
            {
                //IReadOnlyList<RecipeParameter> 
            };
                
        }
        public static CurrentBatch ToDomain(BatchDetails entity)
        {
            return new CurrentBatch()
            {

            };

        }
        public static String ToDomain(IReadOnlyList<RecipeParameter> parameters)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // pretty print
                PropertyNamingPolicy = null // keep Name, Type, Value as-is
            };
            return JsonSerializer.Serialize(parameters, options);
        }
        public static RecipeDetailsDto ToDto(Recipe recipe)
        {
            if (recipe == null)
                return null;

            // Serialize the dynamic parameters to JSON
            string? parametersJson = null;
            if (recipe.Parameters != null && recipe.Parameters.Count > 0)
            {
                parametersJson = JsonSerializer.Serialize(recipe.Parameters);
            }

            return new RecipeDetailsDto
            {
                RecipeCode = recipe.Code,
                RecipeName = recipe.Name,
                ParametersJson = parametersJson
            };
        }
        public static Batch ToDomain(AddBatchRequest request)
        {
            return new Batch
            {
                RecipeCode = request.RecipeCode,
                BatchCode = request.BatchCode,
                BatchQty = request.BatchQty,
                TabletQty = request.TabletQty,
                BatchStatus = "New-Active",
                DateTime = DateTime.UtcNow
            };
        }
    }
}
