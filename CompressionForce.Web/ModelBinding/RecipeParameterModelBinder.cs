// File: Web/ModelBinding/RecipeParameterModelBinder.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CompressionForce.Domain.Entities;
using System.Globalization;

namespace CompressionForce.Web.ModelBinding
{

    public class RecipeParameterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            var vp = ctx.ValueProvider;
            var prefix = ctx.ModelName; // e.g., "Parameters[0]"

            // ✅ Guard: only bind if the prefix has any keys (Name/Type/Value)
            var hasAnyPrefixKey =
                vp.GetValue($"{prefix}.Name").Values.Count > 0 ||
                vp.GetValue($"{prefix}.Type").Values.Count > 0 ||
                vp.GetValue($"{prefix}.Value").Values.Count > 0;

            if (!hasAnyPrefixKey)
            {
                // 🔴 IMPORTANT: tell MVC this element is NOT bound
                ctx.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var name = vp.GetValue($"{prefix}.Name").FirstValue ?? string.Empty;
            var typeRaw = vp.GetValue($"{prefix}.Type").FirstValue ?? "text";
            var type = typeRaw.ToLowerInvariant();

            var param = new RecipeParameter { Name = name, Type = type };

            if (type == "multi-enum")
            {
                var values = vp.GetValue($"{prefix}.Value").Values;
                param.Value = values.ToArray();
            }
            else if (type == "numeric")
            {
                var raw = vp.GetValue($"{prefix}.Value").FirstValue;
                if (!string.IsNullOrWhiteSpace(raw) &&
                    decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    param.Value = d;
                }
                else
                {
                    // Field-level error so MVC can show a message
                    ctx.ModelState.AddModelError($"{prefix}.Value", "Numeric value required.");
                    param.Value = null;
                }
            }
            else
            {
                // enum/text
                param.Value = vp.GetValue($"{prefix}.Value").FirstValue ?? string.Empty;
            }

            ctx.Result = ModelBindingResult.Success(param);
            return Task.CompletedTask;
        }
    }

}
