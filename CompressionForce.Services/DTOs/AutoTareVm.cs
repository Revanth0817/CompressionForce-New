namespace CompressionForce.Services.DTOs
{
    public class AutoTareVm
    {
        public string MotorStatus { get; set; } = "Off";
        public string MotorTrip { get; set; } = "Off";
        public int Revolutions { get; set; } = 10;

        public decimal S1Main { get; set; }
        public decimal S2Main { get; set; }
        public decimal S1Pre { get; set; }
        public decimal S2Pre { get; set; }
        public decimal S1Eject { get; set; }
        public decimal S2Eject { get; set; }
    }
}
