using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Domain.Entities
{
    public class UserAccess
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to UserManagement
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UserManagement User { get; set; }

        // Admin / Operator / Viewer
        [Required]
        [MaxLength(50)]
        public string AccessLevel { get; set; }
    }
}
