using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Data.Entities
{
    /// <summary>
    /// Stores full JSON snapshots for recipe audit trail.
    /// </summary>
    [Table("RecipeHistories")]
    public class RecipeHistoryEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecipeCode { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } // ADD, UPDATE, DELETE

        [Required]
        [MaxLength(100)]
        public string ChangedBy { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; }

        public string OldParameters { get; set; }
        public string NewParameters { get; set; }
    }
}
