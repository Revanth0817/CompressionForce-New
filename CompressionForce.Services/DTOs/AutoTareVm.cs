namespace CompressionForce.Services.DTOs
{
    public class AutoTareVm
    {
        public string MotorStatus { get; set; } = "Off";
        public string MotorTrip { get; set; } = "Off";
        public int Revolutions { get; set; }

        /// <summary>false while the PLC has not written any reading yet</summary>
        public bool HasData { get; set; }

        /// <summary>When the PLC last wrote the reading (UTC)</summary>
        public DateTime? LastUpdatedUtc { get; set; }

        public decimal S1Main { get; set; }
        public decimal S2Main { get; set; }
        public decimal S1Pre { get; set; }
        public decimal S2Pre { get; set; }
        public decimal S1Eject { get; set; }
        public decimal S2Eject { get; set; }
    }
}
