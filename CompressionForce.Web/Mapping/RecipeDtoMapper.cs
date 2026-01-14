using CompressionForce.Domain.Entities;
using CompressionForce.Web.DTOs;

namespace CompressionForce.Web.Mapping
{
    /// <summary>
    /// Maps between Web DTOs and Domain models.
    /// </summary>
    public static class RecipeDtoMapper
    {
        public static Recipe ToDomain(RecipeDto dto)
        {
            return new Recipe(
                dto.Code,
                dto.Name,
                dto.Parameters.Select(p => new RecipeParameter
                {
                    Name = p.Name,
                    Type = p.Type,
                    Value = p.Value
                }).ToList()
            );
        }

        public static RecipeDto ToDto(Recipe recipe)
        {
            return new RecipeDto
            {
                Code = recipe.Code,
                Name = recipe.Name,
                Parameters = recipe.Parameters.Select(p => new RecipeParameterDto
                {
                    Name = p.Name,
                    Type = p.Type,
                    Value = p.Value
                }).ToList()
            };
        }
    }

}
