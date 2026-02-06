using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CompressionForce.SignalRules.Enums;
using CompressionForce.SignalRules.Model;

namespace CompressionForce.SignalGeneration.Validation
{
    internal sealed class JsonSignalRuleContext : ISignalRuleContext
    {
        private readonly JObject _signal;

        public JsonSignalRuleContext(JObject signal)
        {
            _signal = signal;
        }

        public string SignalId => _signal["signalId"]?.ToString();

        public bool IsPrimary =>
            _signal["isPrimary"]?.Value<bool>() ?? false;

        public string AddressKey =>
            _signal["address"] == null
                ? null
                : $"{_signal["address"]!["area"]}:{_signal["address"]!["value"]}";

        public int? AddressLength =>
            _signal["address"]?["length"]?.Value<int>();

        public SignalDataType DataType =>
            Enum.TryParse(
                _signal["dataType"]?.ToString(),
                out SignalDataType dt)
                ? dt
                : SignalDataType.Bool;

        public IReadOnlyList<string> Aliases =>
            _signal["aliases"] is JArray a
                ? a.Select(x => x.ToString()).ToList()
                : new List<string>();

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Roles =>
            _signal["roles"] is JObject roles
                ? roles.Properties().ToDictionary(
                    r => r.Name,
                    r => (IReadOnlyList<string>)(
                        r.Value["groups"] is JArray g
                            ? g.Select(x => x.ToString()).ToList()
                            : new List<string>()))
                : new Dictionary<string, IReadOnlyList<string>>();
    }
}
