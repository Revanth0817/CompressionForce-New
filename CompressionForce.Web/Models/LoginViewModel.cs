using System.ComponentModel.DataAnnotations;

namespace CompressionForce.Web.Models
{
    public class LoginViewModel
    {
        // ✅ Can accept Username OR Email
        [Required(ErrorMessage = "Username or Email is required")]
        [Display(Name = "Username or Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Optional (UI only)
        public bool RememberMe { get; set; }
    }
}
