using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lavillemvc.Controllers
{
    // Controlador nuevo: no reemplaza el EspecialistaController ni el EspecialistaAgendaController.
    public class EspecialistaDisponibilidadController : Controller
    {
        private readonly EspecialistaDisponibilidadDAL _dal;
        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<EspecialistaDisponibilidadController> _logger;

        public EspecialistaDisponibilidadController(
            Conexion conexion,
            IWebHostEnvironment entorno,
            ILogger<EspecialistaDisponibilidadController> logger)
        {
            _dal = new EspecialistaDisponibilidadDAL(conexion);
            _entorno = entorno;
            _logger = logger;
        }

        private int? IdentificarEspecialista()
        {
            // El login debe generar este claim desde la identidad verificada.
            string? claim = User.FindFirst("IdEspecialista")?.Value;
            if (int.TryParse(claim, out int id) && id > 0)
                return id;

            // Prueba LOCAL solamente, compatible con Mi Agenda del Especialista.
            if (_entorno.IsDevelopment() && User.Identity?.IsAuthenticated != true)
                return 1;

            return null;
        }

        private bool PuedeAcceder() =>
            _entorno.IsDevelopment() || User.IsInRole("Especialista");

        private IActionResult AccesoNoPermitido() =>
            User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        // Usar un reloj alineado con el huso de las vistas SQL existentes.
        private static DateTime AhoraLocal()
        {
            var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Caracas");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
        }

        [HttpGet]
        public async Task<IActionResult> Disponibilidad()
        {
            if (!PuedeAcceder()) return AccesoNoPermitido();
            var id = IdentificarEspecialista();
            if (id == null) return AccesoNoPermitido();
            try
            {
                var modelo = await _dal.ObtenerAsync(id.Value,
                    DateOnly.FromDateTime(AhoraLocal()));
                if (modelo == null) return NotFound("El especialista no existe.");

                ViewBag.NombreEspecialista = modelo.NombreEspecialista;
                ViewBag.RolEspecialista = modelo.RolEspecialista;
                ViewBag.ActiveMenuEspecialista = "Disponibilidad";
                ViewBag.ModoPrueba = _entorno.IsDevelopment() && User.Identity?.IsAuthenticated != true;
                return View("~/Views/Especialista/Disponibilidad.cshtml", modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando disponibilidad de especialista {Id}", id);
                return StatusCode(500,
                    "No se pudo consultar disponibilidad. Comprueba que existen las tablas horario_semanal_especialista y ausencia_especialista con sus columnas esperadas.");
            }
        }

        public class FormularioHorarios
        {
            public List<EspecialistaDisponibilidadDAL.HorarioEditado> Horarios { get; set; } = new();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarHorario(FormularioHorarios datos)
        {
            if (!PuedeAcceder()) return AccesoNoPermitido();
            var id = IdentificarEspecialista();
            if (id == null) return AccesoNoPermitido();
            try
            {
                var ahora = AhoraLocal();
                await _dal.GuardarHorarioAsync(
                    id.Value, datos.Horarios,
                    DateOnly.FromDateTime(ahora), TimeOnly.FromDateTime(ahora));
                TempData["DisponibilidadExito"] = "Horario semanal guardado correctamente.";
            }
            catch (ArgumentException ex)
            {
                TempData["DisponibilidadError"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["DisponibilidadError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando horario de especialista {Id}", id);
                TempData["DisponibilidadError"] = "No se pudieron guardar los horarios. Revisa el registro del servidor.";
            }
            return RedirectToAction(nameof(Disponibilidad));
        }

        public class FormularioAusencia
        {
            public string Titulo { get; set; } = "";
            public string Tipo { get; set; } = "";
            public string? Descripcion { get; set; }
            public DateOnly FechaInicio { get; set; }
            public DateOnly FechaFin { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAusencia(FormularioAusencia datos)
        {
            if (!PuedeAcceder()) return AccesoNoPermitido();
            var id = IdentificarEspecialista();
            if (id == null) return AccesoNoPermitido();
            try
            {
                await _dal.AgregarAusenciaAsync(id.Value,
                    new EspecialistaDisponibilidadDAL.NuevaAusencia
                    {
                        Titulo = datos.Titulo ?? "",
                        Tipo = datos.Tipo ?? "",
                        Descripcion = datos.Descripcion,
                        Inicio = datos.FechaInicio,
                        Fin = datos.FechaFin
                    },
                    DateOnly.FromDateTime(AhoraLocal()));
                TempData["DisponibilidadExito"] = "Ausencia registrada correctamente.";
            }
            catch (ArgumentException ex)
            {
                TempData["DisponibilidadError"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["DisponibilidadError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando ausencia del especialista {Id}", id);
                TempData["DisponibilidadError"] = "No se pudo registrar la ausencia. Revisa el servidor.";
            }
            return RedirectToAction(nameof(Disponibilidad));
        }
    }
}
