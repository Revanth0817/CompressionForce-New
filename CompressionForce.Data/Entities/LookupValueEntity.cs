using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Data.Entities
{
    /// <summary>
    /// Shared lookup table for UI dropdowns and validation.
    /// </summary>
    [Table("LookupValues")]
    public class LookupValueEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } // ToolType, Treatment, Shape, etc.

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // Value stored in recipe JSON

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; }

        public bool IsActive { get; set; }
    }

}
