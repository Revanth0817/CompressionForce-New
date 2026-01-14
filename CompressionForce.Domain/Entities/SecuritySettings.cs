namespace CompressionForce.Domain.Entities
{
    public class SecuritySettings
    {
        public int Id { get; set; }

        public int ApplicationTimeoutMinutes { get; set; }
        public int PasswordExpiryDays { get; set; }
        public int MaxWrongAttempts { get; set; }
    }
}
