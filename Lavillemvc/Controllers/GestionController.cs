using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class GestionController : Controller
    {
        public IActionResult GestionCitas() => View();
        public IActionResult GestionClientes() => View();
        public IActionResult GestionPagos() => View();
        public IActionResult PanelPrinciRecepcionista() => View();
        public IActionResult Privacy() => View();
    }
}