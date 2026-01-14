
// File: Web/ModelBinding/RecipeParameterModelBinderProvider.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CompressionForce.Web.ModelBinding
{



    public class RecipeParameterModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(CompressionForce.Domain.Entities.RecipeParameter))
                return new RecipeParameterModelBinder();

            return null!;
        }
    }


}
