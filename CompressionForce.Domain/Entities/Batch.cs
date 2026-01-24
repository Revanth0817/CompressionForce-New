namespace CompressionForce.Domain.Entities;

public class Batch
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public DateTime? DateTime { get; set; } = null;
    public string? UserName { get; set; } = null;

    public string? RecipeCode { get; set; } = null;
    public string? BatchCode { get; set; } = null;
    public int? BatchQty { get; set; } = null;
    public int? TabletQty { get; set; } = null;

    public string? BatchStatus { get; set; } = null;
    public int? GoodQty { get; set; } = 0;
    public int? RejectionQty { get; set; } = 0;



    //Extra properties
    public int? BatchSize { get; set; } = 0;
    public string? BatchNumber { get; set; } = null;
    public int? ProdusedQty { get; set; } = 0;
    public int? LeftQty { get; set; } = 0;
    public int? S2GoodQty { get; set; } = 0;
    public int? S2RejectionQty { get; set; } = 0;
    public string? BatchCondition { get; set; } = null;
}
