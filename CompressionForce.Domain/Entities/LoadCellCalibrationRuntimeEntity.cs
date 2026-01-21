using System;
using System.ComponentModel.DataAnnotations;

namespace CompressionForce.Data.Entities
{
    public class LoadCellCalibrationRuntimeEntity
    {
        [Key]
        public string LoadCellName { get; set; }

        public decimal Factor { get; set; }
        public decimal Offset { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
