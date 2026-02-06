using System.Collections.Generic;

namespace CompressionForce.Domain.Entities
{
    public sealed class PlcSignalRole
    {
        public string RoleName { get; init; } = default!;

        public string? Description { get; init; }

        public List<string> Groups { get; init; } = new();
    }
}
