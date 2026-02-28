namespace CompressionForce.Web.Models
{
    public class AutoModeVM
    {
        // DB fields
        public string? ProductName { get; set; }
        public string? BatchNumber { get; set; }
        public int BatchQty { get; set; }

        // PLC/live fields (populated by SignalR or controller if available)
        public int GoodS1 { get; set; }
        public int RejectedS1 { get; set; }
        public int TotalS1 { get; set; }

        public int GoodS2 { get; set; }
        public int RejectedS2 { get; set; }
        public int TotalS2 { get; set; }

        public double AvgMain1 { get; set; }
        public double AvgPre1 { get; set; }
        public double AvgEject1 { get; set; }

        public double AvgMain2 { get; set; }
        public double AvgPre2 { get; set; }
        public double AvgEject2 { get; set; }

        public double DwellTime { get; set; }
        public int TotalQty { get; set; }
    }
}