using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string txtEmail, string txtPassword)
        {
            // Convertimos a minúsculas y evitamos null
            string correo = txtEmail?.ToLower() ?? "";

            // ===== LÓGICA TEMPORAL SIN BASE DE DATOS =====

            // ADMIN
            if (correo.Contains("admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            // RECEPCIONISTA
            else if (correo.Contains("recepcion"))
            {
                return RedirectToAction("PanelPrinciRecepcionista", "Gestion");
            }
            // ESPECIALISTA / ESTILISTA / COLORISTA
            else if (correo.Contains("especialista") || correo.Contains("estilista") || correo.Contains("colorista"))
            {
                return RedirectToAction("MiAgenda", "Especialista");
            }
            // CLIENTE (si el correo contiene "cliente" o es un @gmail normal)
            else if (correo.Contains("cliente") || correo.Contains("@gmail"))
            {
                return RedirectToAction("DashboardCliente", "Cliente");
            }
            //INVENTARIO 
            else if(correo.Contains("inventario"))
            {
                return RedirectToAction("PanlInventario", "Inventario")
            }

            ViewBag.Error = "Credenciales incorrectas. Prueba con admin@..., recepcion@..., especialista@... o cliente@gmail.com";
            return View();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }
    }
}