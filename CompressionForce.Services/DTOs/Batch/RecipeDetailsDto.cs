using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Batch
{
    public class RecipeDetailsDto
    {
        public string RecipeCode { get; set; } = default!;
        public string RecipeName { get; set; } = default!;

        /// <summary>
        /// Raw JSON stored in DB
        /// </summary>
        public string? ParametersJson { get; set; }
    }
}
