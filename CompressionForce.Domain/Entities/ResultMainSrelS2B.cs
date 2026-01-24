namespace CompressionForce.Domain.Entities;

public partial class ResultMainSrelS2B
{
    public int Id { get; set; }

    public DateTime? DateTime { get; set; }

    public string? BatchNumber { get; set; }

    public double? TabletQtyS2 { get; set; }

    public double? AverageS2 { get; set; }

    public double? SrelValueS2 { get; set; }

    public double? TurretSpeed { get; set; }

    public double? FeederSpeedS2 { get; set; }
}
