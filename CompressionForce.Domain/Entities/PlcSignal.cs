using CompressionForce.Domain.Enums;
using CompressionForce.Domain.ValueObjects;

namespace CompressionForce.Domain.Entities
{
    public sealed class PlcSignal
    {
        public string SignalId { get; init; }
        public SignalDataType DataType { get; init; }
        public UpdateClass UpdateClass { get; init; }
        public PlcAddress Address { get; init; }
    }
}
