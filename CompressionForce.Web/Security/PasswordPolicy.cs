using System.Text.RegularExpressions;

namespace CompressionForce.Web.Security
{
    /// <summary>
    /// The one password rule of the application (sign-up, change password, admin-created and admin-reset passwords).
    /// </summary>
    public static class PasswordPolicy
    {
        public const int MinLength = 8;
        public const int MaxLength = 15;

        // 8-15 characters with a lower case letter, an upper case letter, a digit and a special character
        public const string Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";

        public const string Message =
            "Password must be 8-15 characters with uppercase, lowercase, number, and special character";

        private static readonly Regex Rule = new(Pattern, RegexOptions.Compiled);

        public static bool IsValid(string? password) => !string.IsNullOrEmpty(password) && Rule.IsMatch(password);
    }
}
