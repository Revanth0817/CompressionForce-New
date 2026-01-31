using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CompressionForce.Domain.Plc;

namespace CompressionForce.Integrations.Plc.Config
{
    public static class PlcConfigLoader
    {
        public static IReadOnlyList<PlcSignalDefinition> Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<PlcSignalDefinition>>(json)!
                   ?? throw new InvalidOperationException("Invalid PLC config");
        }
    }
}
