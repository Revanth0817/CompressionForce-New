using CompressionForce.Domain.Enums;
using CompressionForce.SignalRules.Enums;

namespace CompressionForce.Domain.Specification
{
    public static class SignalRules
    {
        public static int? ExpectedRegisterLength(SignalDataType dataType)
        {
            return dataType switch
            {
                SignalDataType.Bool => null,
                SignalDataType.Int16 => 1,
                SignalDataType.Int32 => 2,
                SignalDataType.Float => 2,
                SignalDataType.Double => 4,
                _ => null
            };
        }
    }
}
