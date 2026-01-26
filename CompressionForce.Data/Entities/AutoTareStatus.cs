using System.ComponentModel.DataAnnotations.Schema;

[Table("auto_tare_status", Schema = "public")]
public class AutoTareStatus
{
    public int Id { get; set; }

    public bool MotorStatus { get; set; }
    public bool MotorTrip { get; set; }
    public int Revolutions { get; set; }

    public decimal S1Main { get; set; }
    public decimal S2Main { get; set; }
    public decimal S1Pre { get; set; }
    public decimal S2Pre { get; set; }
    public decimal S1Eject { get; set; }
    public decimal S2Eject { get; set; }

    public DateTime last_updated { get; set; }
}
