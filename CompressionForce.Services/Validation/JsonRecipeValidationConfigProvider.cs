using CompressionForce.Domain.Validation;
using Microsoft.Extensions.Options;


namespace CompressionForce.Services.Validation
{
    /// <summary>
    /// Loads RecipeValidationConfig from app configuration (bound to options).
    /// </summary>
    public class JsonRecipeValidationConfigProvider : IRecipeValidationConfigProvider
    {
        private readonly IOptionsMonitor<RecipeValidationConfig> _options;

        public JsonRecipeValidationConfigProvider(IOptionsMonitor<RecipeValidationConfig> options)
        {
            _options = options;
        }

        public RecipeValidationConfig Get() => _options.CurrentValue;

        public ParameterRule? GetRuleFor(string parameterName)
            => _options.CurrentValue.Parameters
                .FirstOrDefault(p => p.Name.Equals(parameterName, System.StringComparison.OrdinalIgnoreCase));
    }
}

