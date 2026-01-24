namespace CompressionForce.Domain.Abstractions
{
    /// <summary>
    /// Abstraction to access lookup values (ToolType, Treatment, etc).
    /// Implemented in Data/Services layer.
    /// </summary>
    public interface ILookupProvider
    {
        IReadOnlyList<string> GetValues(string category);
    }
}
