using CompressionForce.Domain.Entities;

namespace CompressionForce.Services.DTOs
{
    public class RecipeDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<RecipeParameter> Parameters { get; set; }
    }
}
