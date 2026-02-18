namespace CompressionForce.Domain.Validation
{
    /// <summary>
    /// Configurable validation rule for a single recipe parameter.
    /// </summary>
    public class ParameterRule
    {
        public string Name { get; set; } = "";          // e.g., "MaxTurretRpm", "ToolType"
        public string Type { get; set; } = "text";      // numeric | text | enum | multi-enum
        public bool Required { get; set; } = false;

        // Numeric ranges
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }

        // Text constraints
        public int? MaxLength { get; set; }
        public string? Regex { get; set; }

        // Enum/multi-enum lookup category name (matches LookupValues.Category)
        public string? LookupCategory { get; set; }

        public string? DefaultValue { get; set; }

        public string? ValidationMsg { get; set; }

        public string? PlaceHolder { get; set; }

    }
}


