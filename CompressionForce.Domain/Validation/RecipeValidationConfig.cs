namespace CompressionForce.Domain.Validation
{
    /// <summary>
    /// Whole validation configuration payload.
    /// </summary>
    public class RecipeValidationConfig
    {
        public List<ParameterRule> Parameters { get; set; } = new();
        public Dictionary<string, bool> Rules { get; set; } = new(); // global flags, e.g., requireAtLeastOneParameter
    }
}

