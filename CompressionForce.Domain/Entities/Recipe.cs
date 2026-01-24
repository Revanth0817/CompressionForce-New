namespace CompressionForce.Domain.Entities
{
    public class Recipe
    {
        public string Code { get; }
        public string Name { get; }
        public IReadOnlyList<RecipeParameter> Parameters { get; }

        public Recipe(
            string code,
            string name,
            IReadOnlyList<RecipeParameter> parameters)
        {
            Code = code;
            Name = name;
            Parameters = parameters ?? new List<RecipeParameter>();
        }

    }
}