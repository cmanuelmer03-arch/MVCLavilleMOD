using System;
using System.Linq;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lavillemvc.Controllers
{
    public class GerentePromocionesController : Controller
    {
        private readonly GerentePromocionesDAL _dal;
        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<GerentePromocionesController> _logger;
        private const int SucursalDesarrollo = 2;

        public GerentePromocionesController(IConfiguration config,
            IWebHostEnvironment entorno,
            ILogger<GerentePromocionesController> logger)
        {
            _dal = new GerentePromocionesDAL(config);
            _entorno = entorno;
            _logger = logger;
        }

        // Para pruebas locales se usa la sucursal 2, igual que el Dashboard.
        // En producción EXIGE un gerente autenticado con claim IdSucursal verificado.
        private int? SucursalAutorizada()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Gerente"))
            {
                if (int.TryParse(User.FindFirst("IdSucursal")?.Value, out int id)
                    && id > 0) return id;
            }
            return _entorno.IsDevelopment() ? SucursalDesarrollo : null;
        }

        private bool PuedeCambiar() =>
            _entorno.IsDevelopment() ||
            (User.Identity?.IsAuthenticated == true &&
             User.IsInRole("Gerente") && SucursalAutorizada().HasValue);

        [HttpGet]
        public async Task<IActionResult> GestionPromociones()
        {
            int? sucursal = SucursalAutorizada();
            if (!sucursal.HasValue) return Forbid();

            try
            {
                var pagina = await _dal.ObtenerAsync(sucursal.Value);
                pagina.PuedeEditar = PuedeCambiar();
                return View("~/Views/Gerente/GestionPromociones.cshtml", pagina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando promociones del gerente");
                return View("~/Views/Gerente/GestionPromociones.cshtml",
                    new GerentePromocionesPagina
                    {
                        IdSucursal = sucursal.Value,
                        NombreSucursal = "L'Avielle",
                        PuedeEditar = false,
                        MensajeError = "No se pudieron cargar las promociones. Verifica la vista SQL y la tabla de control en Supabase."
                    });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(GerentePromocionFormulario datos)
        {
            int? sucursal = SucursalAutorizada();
            if (!sucursal.HasValue || !PuedeCambiar()) return Forbid();

            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(datos.Nombre) ||
                datos.FechaInicio is null || datos.FechaFin is null ||
                datos.FechaFin.Value.Date < datos.FechaInicio.Value.Date ||
                datos.IdPromocion < 0 ||
                !ImagenPermitida(datos.ImagenUrl))
            {
                TempData["ErrorPromociones"] =
                    "Verifica nombre, descuento, fechas y URL de la imagen. La fecha final no puede ser anterior al inicio.";
                return RedirectToAction(nameof(GestionPromociones));
            }
            try
            {
                await _dal.GuardarAsync(sucursal.Value, datos);
                TempData["ExitoPromociones"] = datos.IdPromocion is > 0
                    ? "Promoción actualizada correctamente."
                    : "Promoción creada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando promoción");
                TempData["ErrorPromociones"] = "No se pudo guardar. Revisa los registros del servidor.";
            }
            return RedirectToAction(nameof(GestionPromociones));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pausar(int idPromocion, bool pausada)
        {
            int? sucursal = SucursalAutorizada();
            if (!sucursal.HasValue || !PuedeCambiar()) return Forbid();
            if (idPromocion <= 0) return BadRequest();
            try
            {
                await _dal.PausarAsync(sucursal.Value, idPromocion, pausada);
                TempData["ExitoPromociones"] = pausada
                    ? "Promoción pausada sin modificar sus fechas."
                    : "Promoción reanudada.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pausar/reanudar promoción");
                TempData["ErrorPromociones"] = "No se pudo cambiar el estado de la promoción.";
            }
            return RedirectToAction(nameof(GestionPromociones));
        }

        // "Eliminar" del Figma se implementa como ARCHIVAR para no borrar datos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archivar(int idPromocion, bool archivada)
        {
            int? sucursal = SucursalAutorizada();
            if (!sucursal.HasValue || !PuedeCambiar()) return Forbid();
            if (idPromocion <= 0) return BadRequest();
            try
            {
                await _dal.ArchivarAsync(sucursal.Value, idPromocion, archivada);
                TempData["ExitoPromociones"] = archivada
                    ? "Promoción archivada. No se eliminó ningún dato."
                    : "Promoción recuperada del archivo.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al archivar/restaurar promoción");
                TempData["ErrorPromociones"] = "No se pudo archivar/restaurar la promoción.";
            }
            return RedirectToAction(nameof(GestionPromociones));
        }

        private static bool ImagenPermitida(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return true;
            if (valor.Length > 500) return false;
            // No admitir esquemas como javascript: ni rutas protocol-relative.
            if (valor.StartsWith("//", StringComparison.Ordinal)) return false;
            if (valor.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)) return true;
            return Uri.TryCreate(valor, UriKind.Absolute, out var url)
                && url.Scheme == Uri.UriSchemeHttps;
        }
    }
}
