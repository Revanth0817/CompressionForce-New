using CompressionForce.Services.DTOs.Responses;
using CompressionForce.Web.Models.Batch;

namespace CompressionForce.Web.Mapping
{
    public static class BatchPageMapper
    {
        public static BatchPageVM ToVM(BatchPageData data)
        {
            return new BatchPageVM
            {
                Recipes = data.Recipes.Select(r => new RecipeVM
                {
                    RecipeCode = r.RecipeCode,
                    RecipeName = r.RecipeName
                }).ToList(),

                Summary = data.ActiveBatch == null ? null : new BatchSummaryVM
                {
                    BatchCode = data.ActiveBatch.BatchCode,
                    BatchQty = data.ActiveBatch.BatchQty,
                    Status = data.ActiveBatch.BatchStatus
                },

                Parameters = RecipeParametersVM.FromJson(data.ParametersJson)
            };
        }
    }
}


