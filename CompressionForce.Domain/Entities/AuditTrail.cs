using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Domain.Entities
{
    [Table("AuditTrails")]
    public class AuditTrail
    {
        public int Id { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }

        [Column("Activity")]
        public string Activity { get; set; }

        [Column("EventDescription")]
        public string EventDescription { get; set; }

        [Column("DateTime")]
        public DateTime DateTime { get; set; }
    }
}
