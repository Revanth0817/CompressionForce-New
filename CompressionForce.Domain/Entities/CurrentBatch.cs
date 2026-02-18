namespace CompressionForce.Domain.Entities;

public partial class CurrentBatch
{
    public int Id { get; set; }   // ✅ PRIMARY KEY
    public DateTime? DateTime { get; set; }
    public string? BatchNumber { get; set; }
    public string Parameters { get; set; }


}

