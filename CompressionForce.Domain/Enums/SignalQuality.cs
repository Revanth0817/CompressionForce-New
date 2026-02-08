namespace CompressionForce.Domain.Enums
{
    public enum SignalQuality
    {
        Unknown,   // Never received
        Good,      // Fresh & valid
        Stale,     // Too old
        Bad        // Read failed / invalid
    }
}
