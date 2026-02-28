namespace CompressionForce.Domain.PLC;

public enum PlcDataType
{
    Coil,
    DiscreteInput,
    InputRegister,
    HoldingRegister,
    RealInput          // ✅ NEW — 4-byte REAL from TwinCAT
}
