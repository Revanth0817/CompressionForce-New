using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Data.Entities;

[Table("CurrentBatches")]
public partial class CurrentBatchEntity
{
    public int Id { get; set; }   // ✅ PRIMARY KEY
    public DateTime? DateTime { get; set; }
    public string? BatchNumber { get; set; }
    public string Parameters { get; set; }
}

