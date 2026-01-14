using CompressionForce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
