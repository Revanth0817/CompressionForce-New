using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Batch
{
    /// <summary>
    /// Lightweight DTO for recipe dropdowns
    /// </summary>
    public class RecipeListItem
    {
        public string RecipeCode { get; set; }
        public string RecipeName { get; set; }
    }
}
