using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Domain.Entities
{
    public class ServoCalibration
    {
        public int Id { get; set; }

        [Required]
        public string ServoCode { get; set; }

        public string ServoName { get; set; }

        public decimal JogSpeed { get; set; }

        public decimal TorqueLimit { get; set; }

        public decimal SetValue { get; set; }   // ✅ NEW

        public decimal SetPosition { get; set; }

        public decimal SetSpeed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
