namespace CompressionForce.Services.Lookups
{
    /// <summary>
    /// Application service for lookup values.
    /// </summary>
    public interface ILookupService
    {
        Task<IReadOnlyList<string>> GetCodesAsync(string category);
    }
}
