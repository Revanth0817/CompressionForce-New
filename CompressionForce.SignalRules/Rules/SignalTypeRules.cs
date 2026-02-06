using CompressionForce.SignalRules.Enums;

namespace CompressionForce.SignalRules.Rules
{
    public static class SignalTypeRules
    {
        public static int? ExpectedRegisterLength(SignalDataType type)
        {
            switch (type)
            {
                case SignalDataType.Bool:
                    return null;

                case SignalDataType.Int16:
                    return 1;

                case SignalDataType.Int32:
                case SignalDataType.Float:
                    return 2;

                case SignalDataType.Double:
                    return 4;

                default:
                    return null;
            }
        }
    }
}
