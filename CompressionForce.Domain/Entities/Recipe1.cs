using System;

namespace CompressionForce.Domain.Entities;

public partial class Recipe1
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public DateTime? DateTime { get; set; }
    public string? RecipeCreatedBy { get; set; }
    public string? RecipeName { get; set; }
    public string? RecipeCode { get; set; }
    public string? ProductName { get; set; }
    public string? Shape { get; set; }
    public string? Size { get; set; }
    public string? ToolType { get; set; }

    public float? TabletThickness { get; set; }
    public float? TabletHardness { get; set; }
    public float? Weight { get; set; }
    public float? MaxTurretRpm { get; set; }

    public float? ForceFeederRatioS1 { get; set; }
    public float? FillDepthS1 { get; set; }
    public float? MainPenetrationPositionS1 { get; set; }
    public float? MainThicknessPositionS1 { get; set; }
    public float? PrePenetrationPositionS1 { get; set; }
    public float? PreThicknessPositionS1 { get; set; }
    public float? SampleIntervalS1 { get; set; }
    public float? SampleRevolutionQtyS1 { get; set; }
    public float? MaxMainCompForceS1 { get; set; }
    public float? MaxPreCompForceS1 { get; set; }
    public float? MaxEjectionForceS1 { get; set; }
    public float? RejectionForceLimitMaxS1 { get; set; }
    public float? AwcForceLimitMaxS1 { get; set; }
    public float? AwcForceSetPointS1 { get; set; }
    public float? AwcForceLimitMinS1 { get; set; }
    public float? RejectionForceLimitMinS1 { get; set; }

    public float? ForceFeederRatioS2 { get; set; }
    public float? FillDepthS2 { get; set; }
    public float? MainPenetrationPositionS2 { get; set; }
    public float? MainThicknessPositionS2 { get; set; }
    public float? PrePenetrationPositionS2 { get; set; }
    public float? PreThicknessPositionS2 { get; set; }
    public float? SampleIntervalS2 { get; set; }
    public float? SampleRevolutionQtyS2 { get; set; }
    public float? MaxMainCompForceS2 { get; set; }
    public float? MaxPreCompForceS2 { get; set; }
    public float? MaxEjectionForceS2 { get; set; }
    public float? RejectionForceLimitMaxS2 { get; set; }
    public float? AwcForceLimitMaxS2 { get; set; }
    public float? AwcForceSetPointS2 { get; set; }
    public float? AwcForceLimitMinS2 { get; set; }
    public float? RejectionForceLimitMinS2 { get; set; }

    public float? AwcUpper { get; set; }
    public float? AwcLower { get; set; }
    public float? RejUpper { get; set; }
    public float? RejLower { get; set; }
}
