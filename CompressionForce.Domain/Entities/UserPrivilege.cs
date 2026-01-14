namespace CompressionForce.Domain.Entities
{
    public class UserPrivilege
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public UserManagement User { get; set; }

        public bool Recipe { get; set; }
        public bool Operation { get; set; }
        public bool Diagnostic { get; set; }
        public bool SignalMonitoring { get; set; }
        public bool Reports { get; set; }
        public bool Backup { get; set; }
        public bool Alarms { get; set; }
        public bool Batch { get; set; }
    }
}
