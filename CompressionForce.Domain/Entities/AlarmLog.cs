namespace CompressionForce.Domain.Entities;

public class AlarmLog
{
    public int Id { get; set; }
    public int AlarmCode { get; set; }
    public DateTime AlarmCreatedTime { get; set; }
    public string AlarmDescription { get; set; }
    public string UserName { get; set; }
}
