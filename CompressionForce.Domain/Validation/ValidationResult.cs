namespace CompressionForce.Domain.Validation
{
    /// <summary>
    /// Represents validation outcome without throwing immediately.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new();

        public bool IsValid => !_errors.Any();
        public IReadOnlyList<string> Errors => _errors;

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}
