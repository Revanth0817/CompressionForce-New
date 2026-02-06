using System.Collections.Generic;
using CompressionForce.SignalRules.Enums;

namespace CompressionForce.SignalRules.Model
{
    public interface ISignalRuleContext
    {
        string SignalId { get; }
        bool IsPrimary { get; }

        // Address
        string AddressKey { get; }      // e.g. "InputRegister:0"
        int? AddressLength { get; }

        SignalDataType DataType { get; }

        IReadOnlyList<string> Aliases { get; }

        // roleName -> groups
        IReadOnlyDictionary<string, IReadOnlyList<string>> Roles { get; }
    }
}
