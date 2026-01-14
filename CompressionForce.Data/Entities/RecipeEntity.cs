using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Data.Entities
{
    /// <summary>
    /// EF Core entity for Recipes table.
    /// Parameters are stored as JSON (jsonb).
    /// </summary>
    [Table("Recipes")]
    public class RecipeEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecipeCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string RecipeName { get; set; }

        /// <summary>
        /// Dynamic recipe parameters stored as JSON.
        /// </summary>
        [Required]
        public string Parameters { get; set; }
    }
}
