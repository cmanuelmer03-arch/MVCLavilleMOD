
using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Npgsql;

namespace Lavillemvc.Controllers
{
    public class PruebaDashboardController : Controller
    {
        private readonly Conexion _conexion;
        private readonly IWebHostEnvironment _entorno;

        public PruebaDashboardController(
            IConfiguration configuration,
            IWebHostEnvironment entorno)
        {
            _conexion = new Conexion(configuration);
            _entorno = entorno;
        }

        public async Task<IActionResult> Index()
        {
            if (!_entorno.IsDevelopment())
                return NotFound();

            await using var conn = _conexion.GetConnection();

            await conn.OpenAsync();

            string query = @"
                SELECT
                    ""NombreSucursal"",
                    ""TotalEmpleados"",
                    ""ServiciosDisponibles""
                FROM public.vw_gerente_dashboard_sucursal
                WHERE ""IdSucursal"" = @id";

            await using var cmd =
                new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("id", 2);

            await using var reader =
                await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                string sucursal = reader.GetString(0);
                int empleados = reader.GetInt32(1);
                int servicios = reader.GetInt32(2);

                return Content(
                    "CONEXION EXITOSA A SUPABASE\n" +
                    "Vista: vw_gerente_dashboard_sucursal\n" +
                    "Sucursal: " + sucursal + "\n" +
                    "Empleados: " + empleados + "\n" +
                    "Servicios disponibles: " + servicios
                );
            }

            return Content("No se encontraron datos.");
        }
    }
}



