using System;
using System.Text.Json;

namespace CompressionForce.Domain.ValueObjects
{
    public sealed class PlcAddress
    {
        // Address kind (Numeric / Logical / Tag-based)
        public string Kind { get; init; } = default!;

        // Memory area (HoldingRegister, InputRegister, Coil, DiscreteInput, etc.)
        public string Area { get; init; } = default!;

        // Address value (numeric string or symbolic name)
        public string Value { get; init; } = default!;

        // Length in registers / bits (optional)
        public int Length { get; init; } = 1;

        // ------------------------------------------------------------
        // Static factory used by JSON loader
        // ------------------------------------------------------------
        public static PlcAddress FromJson(JsonElement element)
        {
            if (!element.TryGetProperty("kind", out var kindProp))
                throw new InvalidOperationException("Address.kind is required");

            if (!element.TryGetProperty("area", out var areaProp))
                throw new InvalidOperationException("Address.area is required");

            if (!element.TryGetProperty("value", out var valueProp))
                throw new InvalidOperationException("Address.value is required");

            return new PlcAddress
            {
                Kind = kindProp.GetString()!,
                Area = areaProp.GetString()!,
                Value = valueProp.GetString()!,
                Length = element.TryGetProperty("length", out var lengthProp)
                    ? lengthProp.GetInt32()
                    : 1
            };
        }
    }
}
