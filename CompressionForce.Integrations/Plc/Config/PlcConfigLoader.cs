using CompressionForce.Domain.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc.Config
{
    public static class PlcConfigLoader
    {
        public static IReadOnlyList<PlcSignalDefinition> Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("PLC config file not found", path);

            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            );

            var signals = JsonSerializer.Deserialize<List<PlcSignalDefinition>>(json, options);

            if (signals == null || signals.Count == 0)
                throw new InvalidOperationException("PLC config file is empty or invalid");

            return signals;
        }
    }
}
