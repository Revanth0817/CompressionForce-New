namespace CompressionForce.Web.DTOs
{
    /// <summary>
    /// DTO for creating, editing, viewing recipes.
    /// </summary>
    public class RecipeDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<RecipeParameterDto> Parameters { get; set; } = new();
    }
}
