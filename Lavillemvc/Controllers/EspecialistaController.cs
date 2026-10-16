using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class EspecialistaController : Controller
    {
        // Views/Especialista/MiAgenda.cshtml
        public IActionResult MiAgenda()
        {
            return RedirectToAction("MiAgenda", "EspecialistaAgenda");
        }

        // Views/Especialista/Disponibilidad.cshtml
        public IActionResult Disponibilidad()
        {
            return View();
        }

        // Views/Especialista/PerfilEspecialista.cshtml
        public IActionResult PerfilEspecialista()
        {
            return View();
        }
    }
}