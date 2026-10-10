using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult GestionServicios() => View();
        public IActionResult GestionSucursales() => View();
        public IActionResult GestionUsuarios() => View();
        public IActionResult ReportesGenerales() => View();
    }
}