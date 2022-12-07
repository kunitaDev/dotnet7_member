using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace dotnet7_member.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        [Authorize(Roles = "Member")]
        public IActionResult Member()
        {
            return View();
        }
    }
}
