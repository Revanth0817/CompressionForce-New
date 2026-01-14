using System;

namespace CompressionForce.Domain.Entities;

public partial class Batch
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public DateTime? DateTime { get; set; }
    public string? RecipeCode { get; set; }
    public string? BatchCode { get; set; }
    public string? BatchSize { get; set; }
    public string? BatchNumber { get; set; }
    public string? ProdusedQty { get; set; }
    public string? LeftQty { get; set; }
    public string? UserName { get; set; }
    public string? BatchStatus { get; set; }
    public string? GoodQty { get; set; }
    public string? RejectionQty { get; set; }
    public string? S2GoodQty { get; set; }
    public string? S2RejectionQty { get; set; }
    public string? TabletQty { get; set; }
    public string? BatchQty { get; set; }
    public string? BatchCondition { get; set; }
}
