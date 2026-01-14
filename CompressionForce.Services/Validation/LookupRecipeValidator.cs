using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Validation;
using CompressionForce.Services.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Validation
{
    /// <summary>
    /// Validates recipe parameters against lookup values.
    /// </summary>
    public class LookupRecipeValidator
    {
        private readonly ILookupService _lookupService;

        public LookupRecipeValidator(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        public void Validate(Recipe recipe, ValidationResult result)
        {
            foreach (var parameter in recipe.Parameters)
            {
                if (parameter.Type == "enum")
                {
                    ValidateEnum(parameter, result);
                }
                else if (parameter.Type == "multi-enum")
                {
                    ValidateMultiEnum(parameter, result);
                }
            }
        }

        private void ValidateEnum(RecipeParameter parameter, ValidationResult result)
        {
            var allowed = _lookupService
                .GetCodesAsync(parameter.Name)
                .GetAwaiter()
                .GetResult();

            if (parameter.Value is string value &&
                !allowed.Contains(value))
            {
                result.AddError(
                    $"Invalid value '{value}' for parameter '{parameter.Name}'.");
            }
        }

        private void ValidateMultiEnum(RecipeParameter parameter, ValidationResult result)
        {
            var allowed = _lookupService
                .GetCodesAsync(parameter.Name)
                .GetAwaiter()
                .GetResult();

            if (parameter.Value is IEnumerable<string> values)
            {
                foreach (var value in values)
                {
                    if (!allowed.Contains(value))
                    {
                        result.AddError(
                            $"Invalid value '{value}' for parameter '{parameter.Name}'.");
                    }
                }
            }
        }
    }

}
