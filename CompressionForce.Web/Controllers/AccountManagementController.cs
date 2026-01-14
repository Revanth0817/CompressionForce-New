using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Controllers
{
    public class AccountManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
