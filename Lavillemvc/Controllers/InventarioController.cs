using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class InventarioController : Controller
    {
        public IActionResult GestionProductos() => View();
        public IActionResult PanelInventario() => View();
    }
}