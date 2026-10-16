using System;
using System.Globalization;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lavillemvc.Controllers
{
    // Controlador separado para no sobrescribir EspecialistaController.cs
    public class EspecialistaAgendaController : Controller
    {
        private readonly EspecialistaAgendaDAL _dal;
        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<EspecialistaAgendaController> _logger;

        public EspecialistaAgendaController(
            Conexion conexion,
            IWebHostEnvironment entorno,
            ILogger<EspecialistaAgendaController> logger)
        {
            _dal = new EspecialistaAgendaDAL(conexion);
            _entorno = entorno;
            _logger = logger;
        }

        private int? IdentificarEspecialista()
        {
            // Se espera que el login emita este claim con el ID real de especialista.
            string? claim = User.FindFirst("IdEspecialista")?.Value;
            if (int.TryParse(claim, out int id) && id > 0)
                return id;

            // Solo para desarrollo local sin sesión iniciada.
            if (_entorno.IsDevelopment() && User.Identity?.IsAuthenticated != true)
                return 1; // Valentina Rojas en los INSERT originales

            return null;
        }

        private IActionResult AccesoNoPermitido() =>
            User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        private bool PuedeAcceder() =>
            _entorno.IsDevelopment() || User.IsInRole("Especialista");

        [HttpGet]
        public async Task<IActionResult> MiAgenda(
            string periodo = "hoy",
            DateOnly? fecha = null,
            int? idCita = null)
        {
            if (!PuedeAcceder()) return AccesoNoPermitido();
            int? idEspecialista = IdentificarEspecialista();
            if (idEspecialista == null) return AccesoNoPermitido();

            string p = (periodo ?? "hoy").Trim().ToLowerInvariant();
            if (p != "hoy" && p != "semana" && p != "mes")
                return BadRequest("El período solicitado no es válido.");

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
            DateOnly seleccionada = fecha ?? hoy;
            if (seleccionada < hoy.AddYears(-2) || seleccionada > hoy.AddYears(2))
                return BadRequest("La fecha está fuera del rango permitido.");

            DateOnly inicio, fin;
            if (p == "semana")
            {
                int diasDesdeLunes = ((int)seleccionada.DayOfWeek + 6) % 7;
                inicio = seleccionada.AddDays(-diasDesdeLunes);
                fin = inicio.AddDays(6);
            }
            else if (p == "mes")
            {
                inicio = new DateOnly(seleccionada.Year, seleccionada.Month, 1);
                fin = inicio.AddMonths(1).AddDays(-1);
            }
            else
            {
                inicio = fin = seleccionada;
            }

            try
            {
                EspecialistaAgenda? modelo = await _dal.ObtenerAgendaAsync(
                    idEspecialista.Value,
                    seleccionada,
                    inicio,
                    fin,
                    hoy,
                    TimeOnly.FromDateTime(DateTime.Now));

                if (modelo == null) return NotFound("Especialista no registrado.");

                ViewBag.Periodo = p;
                ViewBag.FechaSeleccionada = seleccionada.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.InicioPeriodo = inicio;
                ViewBag.FinPeriodo = fin;
                ViewBag.FechaActual = hoy;
                ViewBag.EsDesarrollo = _entorno.IsDevelopment() && User.Identity?.IsAuthenticated != true;

                // Al pulsar 'Detalles', solo permite elegir una cita del especialista.
                if (idCita.HasValue)
                {
                    var elegida = modelo.Citas.Find(x => x.IdCita == idCita.Value);
                    if (elegida == null)
                        return NotFound("La cita no pertenece al período o especialista.");
                    modelo.ProximaCita = elegida;
                }

                ViewBag.NombreEspecialista = modelo.NombreEspecialista;
                ViewBag.RolEspecialista = modelo.RolEspecialista;

                return View("~/Views/Especialista/MiAgenda.cshtml", modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando agenda del especialista {Id}", idEspecialista);
                return StatusCode(500, "No se pudo cargar la agenda. Revisa los registros del servidor.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCita(
            int idCita, string periodo = "hoy", DateOnly? fecha = null)
        {
            if (!PuedeAcceder()) return AccesoNoPermitido();
            int? idEspecialista = IdentificarEspecialista();
            if (idEspecialista == null) return AccesoNoPermitido();
            if (idCita <= 0) return BadRequest("Identificador de cita inválido.");

            if (periodo != "hoy" && periodo != "semana" && periodo != "mes")
                periodo = "hoy";

            try
            {
                bool ok = await _dal.ConfirmarCitaAsync(
                    idEspecialista.Value,
                    idCita,
                    DateOnly.FromDateTime(DateTime.Now));

                if (ok)
                    TempData["AgendaExito"] = "La cita se confirmó correctamente.";
                else
                    TempData["AgendaError"] = "No se confirmó: la cita no está pendiente, ya pasó, pertenece a otro especialista o hay otra confirmada a la misma hora.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando cita {IdCita}", idCita);
                TempData["AgendaError"] = "No fue posible confirmar la cita. Inténtalo de nuevo.";
            }

            return RedirectToAction(nameof(MiAgenda), new { periodo, fecha });
        }
    }
}
