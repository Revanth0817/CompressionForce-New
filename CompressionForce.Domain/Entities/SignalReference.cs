using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Entities
{
    /// <summary>
    /// A unified reference to a PLC value.
    /// Can represent:
    /// - Primary signal
    /// - Alias
    /// - Role
    /// </summary>
    public sealed class SignalReference
    {
        /// <summary>
        /// The name used by caller (SignalId, Alias, or Role name)
        /// </summary>
        public string Name { get; init; } = default!;

        /// <summary>
        /// The canonical primary signal (polling owner)
        /// </summary>
        public PlcSignal Primary { get; init; } = default!;

        /// <summary>
        /// Optional role metadata (null for primary / alias)
        /// </summary>
        public PlcSignalRole? Role { get; init; }

        public bool IsRole => Role != null;
    }
}
