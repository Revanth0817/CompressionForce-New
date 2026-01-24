namespace CompressionForce.Domain.Entities;

public partial class PrivilageHistory
{
    public int Id { get; set; }   // ✅ PRIMARY KEY

    public string? GroupLevel { get; set; }
    public bool? Recipe { get; set; }
    public bool? Batch { get; set; }
    public bool? Security { get; set; }
    public bool? Reports { get; set; }
    public bool? Alarms { get; set; }
    public bool? Backup { get; set; }
    public bool? Operation { get; set; }
    public bool? Calibration { get; set; }
    public bool? Diagnostics { get; set; }
    public bool? SignalMonitor { get; set; }
    public DateTime? DateTime { get; set; }
    public string? EventType { get; set; }
}
