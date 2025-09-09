using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
