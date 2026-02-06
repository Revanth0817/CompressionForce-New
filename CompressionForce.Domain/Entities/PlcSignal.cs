using CompressionForce.Domain.Enums;
using CompressionForce.Domain.ValueObjects;
using System.Collections.Generic;
using CompressionForce.SignalRules.Enums;

namespace CompressionForce.Domain.Entities
{
    public sealed class PlcSignal
    {
        public string SignalId { get; init; } = default!;

        public bool IsPrimary { get; init; }

        public string? Description { get; init; }

        public PlcAddress? Address { get; init; }

        public SignalDataType DataType { get; init; }


        public UpdateClass UpdateClass { get; init; }

        public List<string> Aliases { get; init; } = new();

        public List<string> Groups { get; init; } = new();

        // 🔹 NEW: Roles (logical views)
        public Dictionary<string, PlcSignalRole> Roles { get; init; }
            = new();
    }
}
