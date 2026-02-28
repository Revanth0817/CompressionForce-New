using CompressionForce.Domain.PLC;

namespace CompressionForce.Domain.PLC
{
    public class PlcTag
    {
        public string Key { get; set; } = string.Empty;

        public string? LoadCellCode { get; set; }

        public PlcDataType Type { get; set; }

        // 🔵 Used by Modbus
        public int Address { get; set; }

        // 🟢 Used by ADS
        public string? Symbol { get; set; }

        public string Polling { get; set; } = "Medium";

        public bool Writable { get; set; }
    }
}