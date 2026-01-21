using CompressionForce.Domain.Plc;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace CompressionForce.Services
{
    public class PlcMappingProvider
    {
        public PlcMapping Mapping { get; }

        public PlcMappingProvider(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "plc-mapping.json");

            if (!File.Exists(path))
                throw new FileNotFoundException("plc-mapping.json not found");

            Mapping = JsonSerializer.Deserialize<PlcMapping>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new Exception("PLC mapping load failed");
        }
    }
}
