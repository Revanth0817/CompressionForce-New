namespace CompressionForce.Domain.Entities
{
    public partial class UserManagement
    {
        public int Id { get; set; }

        public string ERname { get; set; }
        public string ERemail { get; set; }
        public string ERpassword { get; set; }
        public string ERlevel { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
