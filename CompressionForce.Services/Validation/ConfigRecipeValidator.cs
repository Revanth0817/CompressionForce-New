using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CompressionForce.Services.Validation
{
    /// <summary>
    /// Validates a Recipe against configurable rules (from JSON/DB).
    /// Complements your RecipeRulesValidator and LookupRecipeValidator.
    /// </summary>
    public class ConfigRecipeValidator
    {
        private readonly IRecipeValidationConfigProvider _configProvider;

        public ConfigRecipeValidator(IRecipeValidationConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public void Validate(Recipe recipe, ValidationResult result)
        {
            var cfg = _configProvider.Get();

            if (cfg.Rules.TryGetValue("requireAtLeastOneParameter", out var require) && require)
            {
                if (recipe.Parameters == null || !recipe.Parameters.Any())
                    result.AddError("Recipe must contain at least one parameter.");
            }

            foreach (var param in recipe.Parameters)
            {
                var rule = _configProvider.GetRuleFor(param.Name);
                if (rule == null) continue;

                // Required
                if (rule.Required)
                {
                    if (param.Value == null ||
                        (param.Value is string s && string.IsNullOrWhiteSpace(s)) ||
                        (param.Value is IEnumerable<string> arr && !arr.Any()))
                    {
                        result.AddError($"Parameter '{param.Name}' is required.");
                        continue;
                    }
                }

                switch (rule.Type.ToLowerInvariant())
                {
                    case "numeric":
                        if (param.Value is decimal d)
                        {
                            if (rule.Min.HasValue && d < rule.Min.Value)
                                result.AddError($"'{param.Name}' must be ≥ {rule.Min.Value}.");
                            if (rule.Max.HasValue && d > rule.Max.Value)
                                result.AddError($"'{param.Name}' must be ≤ {rule.Max.Value}.");
                        }
                        else
                        {
                            result.AddError($"'{param.Name}' must be numeric.");
                        }
                        break;

                    case "text":
                        if (param.Value is string text)
                        {
                            if (rule.MaxLength.HasValue && text.Length > rule.MaxLength.Value)
                                result.AddError($"'{param.Name}' exceeds max length {rule.MaxLength.Value}.");
                            if (!string.IsNullOrEmpty(rule.Regex))
                            {
                                var re = new Regex(rule.Regex);
                                if (!re.IsMatch(text))
                                    result.AddError($"'{param.Name}' has invalid format.");
                            }
                        }
                        else
                        {
                            result.AddError($"'{param.Name}' must be text.");
                        }
                        break;

                    case "enum":
                        if (param.Value is not string)
                            result.AddError($"'{param.Name}' must be a single selection.");
                        break;

                    case "multi-enum":
                        if (param.Value is not IEnumerable<string>)
                            result.AddError($"'{param.Name}' must be a list of selections.");
                        break;
                }
            }
        }
    }
}


