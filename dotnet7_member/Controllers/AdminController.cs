using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet7_member.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }
    }
}
