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
        // HELPER: obtiene el IdSucursal del claim del usuario
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
        public async Task<IActionResult> GestionCitas(DateTime? semana)
        {
            var refFecha = semana ?? DateTime.Today;

            // Lunes de la semana
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

            ViewBag.Detalle = await _repo.ObtenerDetalleCitaAsync(
                                   citas.Count > 0 ? citas[0].IdCita : 0);

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

            return View();
        }

        public IActionResult Privacy() => View();
    }
}