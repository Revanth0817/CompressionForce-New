using System;

namespace CompressionForce.Domain.Entities;

public partial class CurrentBatch
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public DateTime? DateTime { get; set; }
    public string? BatchNumber { get; set; }
    public string? TurretRpm { get; set; }
    public string? S1FeederRatio { get; set; }
    public string? S2FeederRatio { get; set; }
    public string? S1FillDepth { get; set; }
    public string? S1PreThickness { get; set; }
    public string? S1MainThickness { get; set; }
    public string? S1PrePenetration { get; set; }
    public string? S1MainPenetration { get; set; }
    public string? S2FillDepth { get; set; }
    public string? S2PreThickness { get; set; }
    public string? S2MainThickness { get; set; }
    public string? S2PrePenetration { get; set; }
    public string? S2MainPenetration { get; set; }
    public string? S1RejUpperLimit { get; set; }
    public string? S1AwcUpperLimit { get; set; }
    public string? S1AwcSetLimit { get; set; }
    public string? S1AwcLowerLimit { get; set; }
    public string? S1RejLowerLimit { get; set; }
    public string? S2RejUpperLimit { get; set; }
    public string? S2AwcUpperLimit { get; set; }
    public string? S2AwcSetLimit { get; set; }
    public string? S2AwcLowerLimit { get; set; }
    public string? S2RejLowerLimit { get; set; }
    public string? S1FillCam { get; set; }
    public string? S2FillCam { get; set; }
    public string? AwcUpper { get; set; }
    public string? AwcLower { get; set; }
    public string? RejUpper { get; set; }
    public string? RejLower { get; set; }
}
