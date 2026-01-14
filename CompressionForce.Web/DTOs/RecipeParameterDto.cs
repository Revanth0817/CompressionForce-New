namespace CompressionForce.Web.DTOs
{
    /// <summary>
    /// DTO for a single recipe parameter.
    /// </summary>
    public class RecipeParameterDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }
}
