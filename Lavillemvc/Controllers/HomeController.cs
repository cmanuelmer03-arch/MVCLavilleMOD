using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}