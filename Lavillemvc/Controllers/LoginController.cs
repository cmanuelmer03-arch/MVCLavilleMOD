using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Abre Views/Login/Index.cshtml
        }

        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            // Lógica temporal para probar la conexión de las vistas
            if (usuario == "admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (usuario == "recepcionista")
            {
                return RedirectToAction("PanelPrinciRecepcionista", "Gestion");
            }
            else if (usuario == "cliente")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas (Prueba con admin, recepcionista o cliente)";
            return View();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View(); // Abre Views/Login/Registro.cshtml
        }
    }
}