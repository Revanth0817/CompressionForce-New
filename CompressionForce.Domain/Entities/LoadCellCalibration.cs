using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Domain.Entities
{
    [Table("LoadCellCalibrations")]
    public class LoadCellCalibration
    {
        [Key]
        public int Id { get; set; }

        // 🔹 Loadcell identifier
        [Required]
        [MaxLength(50)]
        public string LoadCellCode { get; set; } = string.Empty;

        // 🔹 Voltage range
        [Column(TypeName = "numeric(10,4)")]
        public decimal MinVolt { get; set; }

        [Column(TypeName = "numeric(10,4)")]
        public decimal MaxVolt { get; set; }

        // 🔹 Force range (kN)
        [Column(TypeName = "numeric(10,4)")]
        public decimal MinValue { get; set; }

        [Column(TypeName = "numeric(10,4)")]
        public decimal MaxValue { get; set; }

        // 🔹 Calibration parameters
        [Column(TypeName = "numeric(18,8)")]
        public decimal Factor { get; set; }

        [Column(TypeName = "numeric(18,8)")]
        public decimal Offset { get; set; }

        // 🔹 Audit
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
