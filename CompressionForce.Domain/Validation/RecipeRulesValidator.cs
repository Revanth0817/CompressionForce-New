using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Validation
{
    /// <summary>
    /// Code-based recipe validation (current implementation).
    /// Can later be replaced by config-driven validator.
    /// </summary>
    public class RecipeRulesValidator : IRecipeValidator
    {
        public ValidationResult Validate(Recipe recipe)
        {
            var result = new ValidationResult();

            // Example: MaxTurretRpm must be > 0
            var rpm = recipe.Parameters
                .FirstOrDefault(p => p.Name == "MaxTurretRpm");

            if (rpm?.Value is decimal rpmValue && rpmValue <= 0)
            {
                result.AddError("MaxTurretRpm must be greater than zero.");
            }

            // Example: Recipe must have at least one parameter
            if (!recipe.Parameters.Any())
            {
                result.AddError("Recipe must contain at least one parameter.");
            }

            return result;
        }
    }
}
