using System.ComponentModel.DataAnnotations;

namespace CompressionForce.Domain.Entities
{
    public class UserGroup
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = "";
    }
}
