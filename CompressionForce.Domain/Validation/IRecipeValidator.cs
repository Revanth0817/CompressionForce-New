using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Validation
{
    /// <summary>
    /// Abstraction for recipe validation.
    /// Allows swapping code-based, JSON-based, or DB-based validators.
    /// </summary>
    public interface IRecipeValidator
    {
        ValidationResult Validate(Recipe recipe);
    }
}
