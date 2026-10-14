using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class ClienteController : Controller
    {
        public IActionResult DashboardCliente() => View();
        public IActionResult MisReservas() => View();
        public IActionResult Pago() => View();
        public IActionResult PerfilCliente() => View();
        public IActionResult Promociones() => View();

        public IActionResult ReservarCita(string especialista, string servicio, string sucursal)
        {
            ViewBag.Especialista = especialista;
            ViewBag.Servicio = servicio;
            ViewBag.Sucursal = sucursal;
            return View();
        }
    }
}