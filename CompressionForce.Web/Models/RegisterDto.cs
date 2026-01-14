namespace CompressionForce.Models
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }        // MUST NOT BE NULL
        public string Role { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
