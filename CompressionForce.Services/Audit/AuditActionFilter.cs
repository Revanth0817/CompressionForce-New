using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;   // ✅ REQUIRED

namespace CompressionForce.Services.Audit
{
    public class AuditActionFilter : IAsyncActionFilter   // ✅ NOW VALID
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AuditActionFilter(
            ApplicationDbContext context,
            IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Execute action first
            var resultContext = await next();

            var method = context.HttpContext.Request.Method;

            // Log only data-changing requests
            if (method != HttpMethods.Post &&
                method != HttpMethods.Put &&
                method != HttpMethods.Delete)
                return;

            var userName =
                _http.HttpContext?.Session.GetString("UserName")
                ?? "System";

            var controller =
                context.ActionDescriptor.RouteValues["controller"];

            var action =
                context.ActionDescriptor.RouteValues["action"];

            var audit = new AuditTrail
            {
                UserName = userName,
                Activity = controller,
                EventDescription = $"{action} action executed",
                DateTime = DateTime.UtcNow
            };

            _context.AuditTrails.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
