namespace CompressionForce.Domain.Entities;

public class BatchHistory
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public DateTime? DateTime { get; set; }
    public string? RecipeCode { get; set; }
    public string? BatchCode { get; set; }
    public string? UserName { get; set; }
    public long? BatchSize { get; set; }
    public string? EReventType { get; set; }
}
