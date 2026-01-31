namespace CompressionForce.Web.Models
{
    public sealed class AutoTareVm
    {
        public string MotorStatus { get; init; }
        public string MotorTrip { get; init; }
        public int Revolutions { get; init; }

        public decimal S1Main { get; init; }
        public decimal S2Main { get; init; }
        public decimal S1Pre { get; init; }
        public decimal S2Pre { get; init; }
        public decimal S1Eject { get; init; }
        public decimal S2Eject { get; init; }
    }
}
