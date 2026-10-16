using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    [Authorize(Roles = "Recepcionista,Administrador,Gerente")]
    public class GestionController : Controller
    {
        private readonly GestionRepositorio _repo;

        public GestionController(GestionRepositorio repo)
        {
            _repo = repo;
        }

        // ==========================================================
        // HELPER
        // ==========================================================
        private int? IdSucursalActual()
        {
            var claim = User.FindFirst("IdSucursal")?.Value;
            return int.TryParse(claim, out var id) ? id : (int?)null;
        }

        // ==========================================================
        // PANEL PRINCIPAL
        // ==========================================================
        public async Task<IActionResult> PanelPrinciRecepcionista()
        {
            var suc = IdSucursalActual();

            ViewBag.Kpis = await _repo.ObtenerKpisPanelAsync(suc);
            ViewBag.Citas = await _repo.ObtenerCitasDelDiaAsync(suc);
            ViewBag.Reservas = await _repo.ObtenerReservasPendientesAsync(suc);
            ViewBag.Registros = await _repo.ObtenerUltimosClientesAsync();
            ViewBag.Atencion = await _repo.ObtenerAtencionAsync(suc);

            return View();
        }

        // ==========================================================
        // GESTIÓN DE CITAS
        // ==========================================================
        public async Task<IActionResult> GestionCitas(DateTime? semana, int? id)
        {
            var refFecha = semana ?? DateTime.Today;

            int diff = ((int)refFecha.DayOfWeek + 6) % 7;
            var lunes = refFecha.AddDays(-diff).Date;
            var sabado = lunes.AddDays(5);

            var citas = await _repo.ObtenerCitasSemanaAsync(
                            lunes, sabado, IdSucursalActual());

            ViewBag.Citas = citas;
            ViewBag.Lunes = lunes;
            ViewBag.Sabado = sabado;
            ViewBag.FechaRef = refFecha.Date;
            ViewBag.MesTexto = lunes.ToString("MMMM yyyy",
                                 new System.Globalization.CultureInfo("es-ES"));

            var idDetalle = id ?? (citas.Count > 0 ? citas[0].IdCita : 0);
            ViewBag.Detalle = await _repo.ObtenerDetalleCitaAsync(idDetalle);

            ViewBag.ClientesSelect = await _repo.ObtenerClientesSelectAsync();
            ViewBag.ServiciosSelect = await _repo.ObtenerServiciosSelectAsync();
            ViewBag.EspecialistasSelect = await _repo.ObtenerEspecialistasSelectAsync(IdSucursalActual());

            return View();
        }

        // ==========================================================
        // GESTIÓN DE CLIENTES
        // ==========================================================
        public async Task<IActionResult> GestionClientes(string? q, int page = 1)
        {
            const int tam = 6;
            var (items, total) = await _repo.ObtenerClientesAsync(
                q, (page - 1) * tam, tam);

            ViewBag.Clientes = items;
            ViewBag.Total = total;
            ViewBag.Buscar = q;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)tam);

            return View();
        }

        // ==========================================================
        // GESTIÓN DE PAGOS
        // ==========================================================
        public async Task<IActionResult> GestionPagos()
        {
            ViewBag.Kpis = await _repo.ObtenerKpisPagosAsync();
            ViewBag.Transacciones = await _repo.ObtenerTransaccionesAsync();
            ViewBag.Desglose = await _repo.ObtenerDesgloseMetodosAsync();
            ViewBag.ClientesSelect = await _repo.ObtenerClientesSelectAsync();

            return View();
        }

        // ==========================================================
        // ACCIÓN — Confirmar cita
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCita(int id, string? returnUrl)
        {
            var ok = await _repo.ConfirmarCitaAsync(id, IdSucursalActual());

            TempData["ToastOk"] = ok
                ? "Cita confirmada correctamente."
                : "No se pudo confirmar la cita.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(GestionCitas));
        }

        // ==========================================================
        // ACCIÓN — Cancelar cita
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCita(int id, string? returnUrl)
        {
            var ok = await _repo.CancelarCitaAsync(id, IdSucursalActual());

            TempData["ToastOk"] = ok
                ? "Cita cancelada."
                : "No se pudo cancelar la cita.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(GestionCitas));
        }

        // ==========================================================
        // ACCIÓN — Nuevo cliente
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoCliente(
            string nombre, string apellido,
            string? telefono, string? correo,
            string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido))
            {
                TempData["ToastOk"] = "Nombre y apellido son obligatorios.";
            }
            else
            {
                var ok = await _repo.InsertarClienteAsync(
                    nombre, apellido, telefono, correo, IdSucursalActual());

                TempData["ToastOk"] = ok
                    ? $"Cliente {nombre} {apellido} registrado correctamente."
                    : "No se pudo registrar el cliente.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(GestionClientes));
        }

        // ==========================================================
        // ACCIÓN — Nueva cita
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaCita(
            int idCliente, int idServicio, int? idEmpleado,
            DateTime fecha, string hora, string? returnUrl)
        {
            if (idCliente <= 0 || idServicio <= 0 || string.IsNullOrWhiteSpace(hora))
            {
                TempData["ToastOk"] = "Completa todos los campos obligatorios.";
            }
            else if (!TimeSpan.TryParse(hora, out var horaParsed))
            {
                TempData["ToastOk"] = "Hora inválida.";
            }
            else
            {
                var ok = await _repo.InsertarCitaAsync(
                    idCliente, idServicio, idEmpleado,
                    fecha, horaParsed, IdSucursalActual());

                TempData["ToastOk"] = ok
                    ? "Cita creada correctamente."
                    : "No se pudo crear la cita.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(GestionCitas));
        }

        // ==========================================================
        // ACCIÓN — Nuevo pago
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoPago(
            int idCliente, decimal monto,
            string metodo, string estado, string? returnUrl)
        {
            if (idCliente <= 0 || monto <= 0 ||
                string.IsNullOrWhiteSpace(metodo))
            {
                TempData["ToastOk"] = "Completa todos los campos obligatorios.";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(estado)) estado = "Pagado";

                var ok = await _repo.InsertarPagoAsync(
                    idCliente, monto, metodo, estado);

                TempData["ToastOk"] = ok
                    ? "Pago registrado correctamente."
                    : "No se pudo registrar el pago.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(GestionPagos));
        }

        public IActionResult Privacy() => View();
    }
}