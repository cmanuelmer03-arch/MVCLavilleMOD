
using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Lavillemvc.Models;

namespace Lavillemvc.Controllers
{
    public class GerenteDashboardController : Controller
    {
        private readonly GerenteDashboardDAL _dashboardDAL;
        private readonly ILogger<GerenteDashboardController> _logger;

        public GerenteDashboardController(
            IConfiguration configuration,
            ILogger<GerenteDashboardController> logger)
        {
            _dashboardDAL =
                new GerenteDashboardDAL(configuration);

            _logger = logger;
        }

        // =========================================
        // DASHBOARD DEL GERENTE
        // =========================================

        [HttpGet]
        public async Task<IActionResult> DashboardSucursal()
        {
            try
            {
                // SUCURSAL DE PRUEBA
                // 2 = L'Avielle Las Mercedes
                int idSucursal = 2;

                // CONSULTAR SUPABASE DESDE LA DAL
                GerenteDashboardSucursal? modelo =
                    await _dashboardDAL.ObtenerDashboardAsync(
                        idSucursal
                    );

                // COMPROBAR SI EXISTE LA SUCURSAL
                if (modelo == null)
                {
                    return NotFound(
                        "No se encontraron datos para esta sucursal."
                    );
                }

                // ABRIR LA VISTA EN SU UBICACIÓN REAL
                return View(
                    "~/Views/Gerente/Dashboard.cshtml",
                    modelo
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cargando Dashboard del Gerente"
                );

                return StatusCode(
                    500,
                    "No se pudo cargar el Dashboard del Gerente. " +
                    "Revisa los registros de Visual Studio."
                );
            }
        }
    }
}
