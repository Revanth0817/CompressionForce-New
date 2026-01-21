using System.Text.Json;
using System.Text.Json.Serialization;
using CompressionForce.Domain.PLC;

public static class PlcConfigLoader
{
    public static PlcTagConfig Load(string filePath)
    {
        string json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<PlcTagConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            }
        )!;
    }
}
