
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Data.Entities
{
    [Table("RecipeValidationRules")]
    public class RecipeValidationRuleEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";       // parameter name

        [Required, MaxLength(20)]
        public string Type { get; set; } = "text";   // numeric | text | enum | multi-enum

        public bool Required { get; set; } = false;

        public decimal? Min { get; set; }
        public decimal? Max { get; set; }

        public int? MaxLength { get; set; }
        public string? Regex { get; set; }

        [MaxLength(100)]
        public string? LookupCategory { get; set; }
    }
}
