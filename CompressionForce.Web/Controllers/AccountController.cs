using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

using CompressionForce.Data;
using CompressionForce.Models;
using CompressionForce.Web.Models;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.Audit;

namespace CompressionForce.WebControllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogger _auditLogger;

        // ===================== CONSTRUCTOR =====================
        public AccountController(
            ApplicationDbContext context,
            AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        /* ===================== LOGIN ===================== */

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var input = model.Email.Trim();

            var user = await _context.UserManagements
                .FirstOrDefaultAsync(u =>
                    u.ERemail == input.ToLower() ||
                    u.ERname == input
                );

            if (user == null || string.IsNullOrWhiteSpace(user.ERpassword))
            {
                ModelState.AddModelError("", "Invalid username/email or password");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is deactivated. Contact administrator.");
                return View(model);
            }

            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                var remaining = user.LockedUntil.Value - DateTime.UtcNow;

                ModelState.AddModelError("",
                    $"Account locked. Try again after {remaining.Minutes} min {remaining.Seconds} sec.");

                return View(model);
            }

            bool passwordValid;
            try
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.ERpassword);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid username/email or password");
                return View(model);
            }

            if (!passwordValid)
            {
                var settings = await _context.SecuritySettings.FirstOrDefaultAsync();
                user.FailedLoginAttempts++;

                if (settings != null &&
                    user.FailedLoginAttempts >= settings.MaxWrongAttempts)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }

                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Invalid username/email or password");
                return View(model);
            }

            // 🔁 PASSWORD EXPIRED
            if (user.ExpiryDate.HasValue && user.ExpiryDate < DateTime.UtcNow)
            {
                HttpContext.Session.SetString("UserName", user.ERname);
                return RedirectToAction("ChangePassword");
            }

            // ===================== SUCCESS LOGIN =====================
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", user.ERname);
            HttpContext.Session.SetString("UserLevel", user.ERlevel ?? "User");
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("O"));

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Login",
                $"User '{user.ERname}' logged into the system"
            );

            return RedirectToAction("Welcome", "Home");
        }

        /* ===================== REGISTER ===================== */

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLower();
            var username = model.UserName.Trim();

            if (await _context.UserManagements.AnyAsync(u => u.ERemail == email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            if (await _context.UserManagements.AnyAsync(u => u.ERname == username))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                return View(model);
            }

            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();

            var user = new UserManagement
            {
                ERname = username,
                ERemail = email,
                ERpassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ERlevel = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(settings?.PasswordExpiryDays ?? 90),
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            _context.UserManagements.Add(user);
            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "User Management",
                $"New user '{username}' registered"
            );

            TempData["SuccessMessage"] = "Signup successful. Please login.";
            return RedirectToAction("Login");
        }

        /* ===================== CHANGE PASSWORD ===================== */

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login");

            var user = await _context.UserManagements
                .FirstOrDefaultAsync(u => u.ERname == userName);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.ERpassword))
            {
                ModelState.AddModelError("OldPassword", "Old password is incorrect");
                return View(model);
            }

            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();

            user.ERpassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ExpiryDate = DateTime.UtcNow.AddDays(settings?.PasswordExpiryDays ?? 90);

            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Change Password",
                $"User '{userName}' changed password"
            );

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Welcome", "Home");
        }

        /* ===================== LOGOUT ===================== */

        public IActionResult Logout()
        {
            var userName = HttpContext.Session.GetString("UserName");

            if (!string.IsNullOrEmpty(userName))
            {
                // 🔥 AUDIT LOG
                _auditLogger.Log(
                    "Logout",
                    $"User '{userName}' logged out"
                );
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
