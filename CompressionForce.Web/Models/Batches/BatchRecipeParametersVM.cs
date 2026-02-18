namespace CompressionForce.Web.Models.Batches
{
    public class BatchRecipeParametersVM
    {
        //CommonProductParameter
        public string ToolType { get; set; }
        public string AwcArType { get; set; }
        public string RecipeType { get; set; }
        public string RecipeCode { get; set; }

        public string ProductName { get; set; }
        public string Shape { get; set; }
        public decimal Size { get; set; }

        public decimal TableThickness { get; set; }
        public decimal TableHardness { get; set; }
        public decimal TableWeight { get; set; }
        public int MaxTurretRpm { get; set; }

        //ProductParameter S1
        public decimal ForceFeederRatioS1 { get; set; }
        public decimal FillDepthS1 { get; set; }
        public decimal MainPenetrationPositionS1 { get; set; }
        public decimal MainThicknessPositionS1 { get; set; }
        public decimal PrePenetrationPositionS1 { get; set; }
        public decimal PreThicknessPositionS1 { get; set; }
        public int SampleIntervalS1 { get; set; }
        public int SampleRevolutionQtyS1 { get; set; }
        public decimal MaxMainCompressionForceS1 { get; set; }
        public decimal MaxPreCompressionForceS1 { get; set; }
        public decimal MaxEjectionCompressionForceS1 { get; set; }


        //ProductParameter S2
        public decimal ForceFeederRatioS2 { get; set; }
        public decimal FillDepthS2 { get; set; }
        public decimal MainPenetrationPositionS2 { get; set; }
        public decimal MainThicknessPositionS2 { get; set; }
        public decimal PrePenetrationPositionS2 { get; set; }
        public decimal PreThicknessPositionS2 { get; set; }
        public int SampleIntervalS2 { get; set; }
        public int SampleRevolutionQtyS2 { get; set; }
        public decimal MaxMainCompressionForceS2 { get; set; }
        public decimal MaxPreCompressionForceS2 { get; set; }
        public decimal MaxEjectionCompressionForceS2 { get; set; }


        //AwcArParameter S1
        public decimal RejectionLimitMaxS1 { get; set; }
        public decimal AwcLimitMaxS1 { get; set; }
        public decimal AwcSetPointS1 { get; set; }
        public decimal AwcLimitMinS1 { get; set; }
        public decimal RejectionLimitMinS1 { get; set; }


        //AwcArParameter S2
        public decimal RejectionLimitMaxS2 { get; set; }
        public decimal AwcLimitMaxS2 { get; set; }
        public decimal AwcSetPointS2 { get; set; }
        public decimal AwcLimitMinS2 { get; set; }
        public decimal RejectionLimitMinS2 { get; set; }
    }
}
