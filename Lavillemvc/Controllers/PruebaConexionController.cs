
using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Npgsql;

namespace Lavillemvc.Controllers
{
    public class PruebaConexionController : Controller
    {
        private readonly Conexion _conexion;
        private readonly IWebHostEnvironment _entorno;

        public PruebaConexionController(
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

            await using var conn =
                _conexion.GetConnection();

            await conn.OpenAsync();

            string sql =
                "SELECT COUNT(*) FROM public.cliente";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            var resultado =
                await cmd.ExecuteScalarAsync();

            return Content(
                "Conexión exitosa a Supabase. " +
                "Clientes registrados: " + resultado
            );
        }
    }
}
