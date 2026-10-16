using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lavillemvc.Controllers
{
    public class GerenteEmpleadosController : Controller
    {
        private readonly GerenteEmpleadosDAL _dal;
        private readonly ILogger<GerenteEmpleadosController> _logger;
        private readonly IWebHostEnvironment _entorno;

        // SOLO PARA PRUEBAS. Sustituir por sucursal del gerente autenticado.
        private const int SucursalDePrueba = 2;

        public GerenteEmpleadosController(
            IConfiguration configuration,
            ILogger<GerenteEmpleadosController> logger,
            IWebHostEnvironment entorno)
        {
            _dal = new GerenteEmpleadosDAL(configuration);
            _logger = logger;
            _entorno = entorno;
        }

        [HttpGet]
        public async Task<IActionResult> GestionEmpleados(string? buscar, string? categoria)
        {
            try
            {
                var empleados = await _dal.ObtenerPorSucursalAsync(SucursalDePrueba);
                ViewBag.NombreSucursal = empleados.FirstOrDefault()?.NombreSucursal
                    ?? "Sucursal Las Mercedes (prueba)";

                ViewBag.TotalEmpleadosSucursal = empleados.Count;
                ViewBag.TiempoCompletoSucursal = empleados.Count(e => e.HorasSemanales >= 40);
                ViewBag.MedioTiempoSucursal = empleados.Count(e =>
                    e.HorasSemanales.HasValue && e.HorasSemanales > 0 && e.HorasSemanales < 40);
                ViewBag.SinHorario = empleados.Count(e => !e.DiasConfigurados.HasValue);
                ViewBag.Buscar = buscar ?? "";
                ViewBag.Categoria = categoria ?? "Todos";
                ViewBag.Especialidades = await _dal.ObtenerEspecialidadesAsync();
                ViewBag.EdicionHabilitada = _entorno.IsDevelopment();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    empleados = empleados.Where(e =>
                        e.NombreCompleto.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                        (e.Cargo ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                        e.Especialidad.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todos")
                {
                    empleados = empleados.Where(e =>
                        string.Equals(e.CategoriaFiltro, categoria,
                            StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return View("~/Views/Gerente/GestionEmpleados.cshtml", empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando Gestión de Empleados");
                ViewBag.Error = "No fue posible cargar los especialistas. Revisa la vista SQL y los registros del servidor.";
                ViewBag.Buscar = buscar ?? "";
                ViewBag.Categoria = categoria ?? "Todos";
                ViewBag.NombreSucursal = "";
                ViewBag.Especialidades = new List<GerenteEmpleadosDAL.EspecialidadOpcion>();
                ViewBag.EdicionHabilitada = false;
                return View("~/Views/Gerente/GestionEmpleados.cshtml", new List<GerenteGestionEmpleados>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(GerenteEmpleadosDAL.CrearEntrada datos)
        {
            // Sin autenticación real no se habilitan cambios en producción.
            if (!_entorno.IsDevelopment()) return Forbid();

            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Apellido) ||
                string.IsNullOrWhiteSpace(datos.Cargo) ||
                datos.IdEspecialidad <= 0 ||
                datos.AniosExperiencia < 0)
            {
                TempData["ErrorEmpleados"] = "Revisa el nombre, apellido, cargo, especialidad y experiencia.";
                return RedirectToAction(nameof(GestionEmpleados));
            }

            try
            {
                await _dal.CrearAsync(SucursalDePrueba, datos);
                TempData["ExitoEmpleados"] = "Especialista agregado a la sucursal.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando especialista");
                TempData["ErrorEmpleados"] = "No se pudo crear el especialista. Revisa los datos o consulta los registros.";
            }
            return RedirectToAction(nameof(GestionEmpleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(GerenteEmpleadosDAL.EditarEntrada datos)
        {
            if (!_entorno.IsDevelopment()) return Forbid();
            if (datos.IdEspecialista <= 0 || datos.IdEspecialidad <= 0 ||
                string.IsNullOrWhiteSpace(datos.Cargo) || datos.AniosExperiencia < 0)
            {
                TempData["ErrorEmpleados"] = "Los datos de edición son inválidos.";
                return RedirectToAction(nameof(GestionEmpleados));
            }

            try
            {
                await _dal.EditarAsync(SucursalDePrueba, datos);
                TempData["ExitoEmpleados"] = "Especialista actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando especialista");
                TempData["ErrorEmpleados"] = "No se pudo actualizar. Comprueba que pertenezca a esta sucursal.";
            }
            return RedirectToAction(nameof(GestionEmpleados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarHorario(GerenteEmpleadosDAL.HorarioEntrada datos)
        {
            if (!_entorno.IsDevelopment()) return Forbid();
            if (datos.IdEspecialista <= 0 || datos.DiaSemana is < 1 or > 7)
            {
                TempData["ErrorEmpleados"] = "El especialista o el día no es válido.";
                return RedirectToAction(nameof(GestionEmpleados));
            }
            if (datos.Habilitado)
            {
                if (datos.HoraInicio == null || datos.HoraFin == null ||
                    datos.HoraFin <= datos.HoraInicio)
                {
                    TempData["ErrorEmpleados"] = "La hora de salida debe ser posterior a la entrada.";
                    return RedirectToAction(nameof(GestionEmpleados));
                }
                if (datos.InicioAlmuerzo.HasValue != datos.FinAlmuerzo.HasValue ||
                    (datos.InicioAlmuerzo.HasValue &&
                     (datos.InicioAlmuerzo < datos.HoraInicio ||
                      datos.FinAlmuerzo > datos.HoraFin ||
                      datos.FinAlmuerzo <= datos.InicioAlmuerzo)))
                {
                    TempData["ErrorEmpleados"] = "El horario del almuerzo no es válido.";
                    return RedirectToAction(nameof(GestionEmpleados));
                }
            }
            try
            {
                await _dal.GuardarHorarioAsync(SucursalDePrueba, datos);
                TempData["ExitoEmpleados"] = "Horario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando horario");
                TempData["ErrorEmpleados"] = "No se pudo guardar el horario. Comprueba la tabla de horarios en Supabase.";
            }
            return RedirectToAction(nameof(GestionEmpleados));
        }
    }
}
