namespace CompressionForce.Services.DTOs
{
    public sealed class AutoTareDto
    {
        public bool MotorStatus { get; init; }
        public bool MotorTrip { get; init; }
        public int Revolutions { get; init; }

        public decimal S1Main { get; init; }
        public decimal S2Main { get; init; }
        public decimal S1Pre { get; init; }
        public decimal S2Pre { get; init; }
        public decimal S1Eject { get; init; }
        public decimal S2Eject { get; init; }
    }
}
