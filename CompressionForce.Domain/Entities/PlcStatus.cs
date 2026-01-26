using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompressionForce.Data.Entities
{
    [Table("plc_status", Schema = "public")]
    public class PlcStatus
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        [Column("plc_heartbeat")]
        public DateTime PlcHeartbeat { get; set; }

        [Column("plc_ip")]
        public string PlcIp { get; set; }

        [Column("is_local_db_connected")]
        public bool IsLocalDbConnected { get; set; }

        [Column("is_plc_connected")]
        public bool IsPlcConnected { get; set; }
    }
}
