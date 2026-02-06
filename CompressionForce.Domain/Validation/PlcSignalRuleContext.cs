using System.Collections.Generic;
using System.Linq;
using CompressionForce.Domain.Entities;
using CompressionForce.SignalRules.Enums;
using CompressionForce.SignalRules.Model;

namespace CompressionForce.Domain.Validation
{
    internal sealed class PlcSignalRuleContext : ISignalRuleContext
    {
        private readonly PlcSignal _signal;

        public PlcSignalRuleContext(PlcSignal signal)
        {
            _signal = signal;
        }

        public string SignalId => _signal.SignalId;

        public bool IsPrimary => _signal.IsPrimary;

        public string AddressKey =>
            _signal.Address == null
                ? null
                : $"{_signal.Address.Area}:{_signal.Address.Value}";

        public int? AddressLength => _signal.Address?.Length;

        public SignalDataType DataType => _signal.DataType;

        public IReadOnlyList<string> Aliases =>
            _signal.Aliases?.ToList() ?? new List<string>();

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Roles =>
            _signal.Roles?.ToDictionary(
                r => r.Key,
                r => (IReadOnlyList<string>)r.Value.Groups.ToList()
            ) ?? new Dictionary<string, IReadOnlyList<string>>();
    }
}
