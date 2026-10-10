using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class ServiciosController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Especialistas() => View();
        public IActionResult Sucursales() => View();
    }
}