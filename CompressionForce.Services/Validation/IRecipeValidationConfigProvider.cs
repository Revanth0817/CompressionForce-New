using CompressionForce.Domain.Validation;


namespace CompressionForce.Services.Validation
{
    /// <summary>
    /// Abstraction for obtaining recipe validation configuration (JSON now, DB later).
    /// </summary>
    public interface IRecipeValidationConfigProvider
    {
        RecipeValidationConfig Get();
        ParameterRule? GetRuleFor(string parameterName);
    }
}

