using Microsoft.AspNetCore.Mvc;

namespace TuProyecto.Controllers
{
    public class ClienteController : Controller
    {
        // Muestra Views/Cliente/DashboardCliente.cshtml
        public IActionResult DashboardCliente()
        {
            return View();
        }

        // Muestra Views/Cliente/MisReservas.cshtml
        public IActionResult MisReservas()
        {
            return View();
        }

        // Muestra Views/Cliente/Pago.cshtml
        public IActionResult Pago()
        {
            return View();
        }

        // Muestra Views/Cliente/PerfilCliente.cshtml
        public IActionResult PerfilCliente()
        {
            return View();
        }

        // Muestra Views/Cliente/Promociones.cshtml
        public IActionResult Promociones()
        {
            return View();
        }

        // Muestra Views/Cliente/ReservarCita.cshtml
        // Recibe los parámetros opcionales que ya enviaste desde Servicios
        public IActionResult ReservarCita(string especialista, string servicio, string sucursal)
        {
            // Guardamos en ViewBag para mostrarlo en la vista si quieres
            ViewBag.Especialista = especialista;
            ViewBag.Servicio = servicio;
            ViewBag.Sucursal = sucursal;
            return View();
        }
    }
}