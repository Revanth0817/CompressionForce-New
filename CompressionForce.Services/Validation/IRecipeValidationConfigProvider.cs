using CompressionForce.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

