using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Services.DTOs.Batch;

namespace CompressionForce.Services.DTOs.Responses
{
    using System.Collections.Generic;

    public class BatchPageData
    {
        public string SelectedRecipeCode { get; set; }
        public string SelectedBatchCode { get; set; }

        public IReadOnlyList<RecipeListItem> Recipes { get; set; }
        public IReadOnlyList<BatchListItem> Batches { get; set; }

        public BatchDetails BatchDetails { get; set; }

        // JSON payload for all 5 parameter sections
        public string ParametersJson { get; set; }

        public BatchDetails ActiveBatch { get; set; }

    }
}
