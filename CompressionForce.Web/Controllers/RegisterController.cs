using Microsoft.AspNetCore.Mvc;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Models;
using BCrypt.Net;

namespace CompressionForce.Controllers
{
    [ApiController]
    [Route("Register")]
    [CompressionForce.Web.Authorization.RequirePrivilege("AccessManagement")]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Register/Create
        [HttpPost("Create")]
        public IActionResult Create([FromBody] RegisterDto model)
        {
            if (model == null)
                return BadRequest("Request body is empty");

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(model.UserName) ||
                string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Username and password are required");

            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var email = model.Email.Trim().ToLower();
            var username = model.UserName.Trim();

            if (_context.UserManagements.Any(u => u.ERemail == email))
                return BadRequest("Email already exists");

            if (_context.UserManagements.Any(u => u.ERname == username))
                return BadRequest("Username already exists");

            var user = new UserManagement
            {
                ERname = username,
                ERemail = email,
                ERpassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ERlevel = "User", // role is assigned by an administrator
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserManagements.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                userName = user.ERname,
                email = user.ERemail,
                role = user.ERlevel,
                createdDate = user.CreatedDate,
                isActive = true
            });
        }
    }
}
