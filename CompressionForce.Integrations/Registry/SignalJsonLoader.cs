using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;
using CompressionForce.Domain.ValueObjects;

namespace CompressionForce.Integrations.Registry
{
    public sealed class SignalJsonLoader
    {
        public IReadOnlyDictionary<string, PlcSignal> Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var doc = JsonDocument.Parse(json);

            var signals = new Dictionary<string, PlcSignal>();

            foreach (var el in doc.RootElement.GetProperty("signals").EnumerateArray())
            {
                var signal = new PlcSignal
                {
                    SignalId = el.GetProperty("signalId").GetString(),
                    DataType = Enum.Parse<SignalDataType>(el.GetProperty("dataType").GetString()),
                    UpdateClass = Enum.Parse<UpdateClass>(el.GetProperty("updateClass").GetString()),
                    Address = new PlcAddress
                    {
                        Kind = Enum.Parse<AddressKind>(el.GetProperty("address").GetProperty("kind").GetString()),
                        Value = el.GetProperty("address").GetProperty("value").GetString(),
                        Length = el.GetProperty("address").TryGetProperty("length", out var l) ? l.GetInt32() : null
                    }
                };

                if (signals.ContainsKey(signal.SignalId))
                    throw new InvalidDataException($"Duplicate SignalId: {signal.SignalId}");

                signals.Add(signal.SignalId, signal);
            }

            return signals;
        }
    }
}
