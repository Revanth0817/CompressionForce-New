using System.ComponentModel.DataAnnotations;

namespace CompressionForce.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(30, ErrorMessage = "Username can be at most 30 characters")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Username can only use letters, numbers, dot, underscore and dash")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        // ✅ OPTIONAL (Admin can set later)
        public string? Role { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(CompressionForce.Web.Security.PasswordPolicy.Pattern,
            ErrorMessage = CompressionForce.Web.Security.PasswordPolicy.Message)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
