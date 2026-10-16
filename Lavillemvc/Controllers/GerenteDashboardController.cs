
using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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


        [Authorize(Roles = "Gerente")]
        [HttpGet]
        public async Task<IActionResult> DashboardSucursal()
        {
            // Obtener la sucursal del gerente autenticado
            string? sucursalClaim = User.FindFirst("IdSucursal")?.Value;

            if (!int.TryParse(sucursalClaim, out int idSucursal)
                || idSucursal <= 0)
            {
                return Forbid();
            }

            try
            {
                GerenteDashboardSucursal? modelo =
                    await _dashboardDAL.ObtenerDashboardAsync(idSucursal);

                if (modelo == null)
                {
                    return NotFound(
                        "No se encontraron datos para esta sucursal."
                    );
                }

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
