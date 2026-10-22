using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompressionForce.Web.Authorization
{
    /// <summary>
    /// Requires the logged-in (session) user's group to hold at least one of the given privileges.
    /// Page navigations get redirected (login / access denied page); API calls get 401 / 403.
    /// An attribute on an action overrides one on its controller (the most specific one decides).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePrivilegeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string[] Privileges { get; }

        public RequirePrivilegeAttribute(params string[] privileges) => Privileges = privileges;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var metadata = context.ActionDescriptor.EndpointMetadata;

            // Actions can opt out (e.g. "my own privileges")
            if (metadata.OfType<SkipPrivilegeCheckAttribute>().Any())
                return;

            // controller-level and action-level attributes both run: only the most specific one decides
            if (!ReferenceEquals(metadata.OfType<RequirePrivilegeAttribute>().LastOrDefault(), this))
                return;

            var request = context.HttpContext.Request;
            var isPageNavigation = HttpMethods.IsGet(request.Method) &&
                                   request.Headers.Accept.ToString().Contains("text/html");

            var userName = context.HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                context.Result = isPageNavigation
                    ? new RedirectResult("/Account/Login")
                    : new UnauthorizedResult();
                return;
            }

            var service = context.HttpContext.RequestServices.GetRequiredService<PrivilegeService>();
            var allowed = await service.GetAllowedAsync(userName);

            if (Privileges.Any(allowed.Contains))
                return;

            context.Result = isPageNavigation
                ? new RedirectToActionResult("AccessDenied", "Home", null)
                : new ObjectResult("You do not have permission to perform this action.")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipPrivilegeCheckAttribute : Attribute { }
}
