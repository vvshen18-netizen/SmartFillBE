using Microsoft.AspNetCore.Mvc;

namespace SmarfillAPI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
