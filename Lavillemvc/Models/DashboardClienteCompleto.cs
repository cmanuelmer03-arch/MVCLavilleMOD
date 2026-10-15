
using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Npgsql;

namespace Lavillemvc.Controllers
{
    public class ClienteController : Controller
    {
        private readonly Conexion _conexion;

        public ClienteController(IConfiguration configuration)
        {
            _conexion = new Conexion(configuration);
        }

        public async Task<IActionResult> Dashboard()
        {
            // SOLO PARA PRUEBAS:
            // María Silva tiene IdCliente = 1.
            // Más adelante obtendremos el ID
            // del usuario que inició sesión.
            int idCliente = 1;

            await using var conn =
                _conexion.GetConnection();

            await conn.OpenAsync();

            string sql = @"
                SELECT *
                FROM public.vw_dashboard_cliente_completo
                WHERE ""IdCliente"" = @idCliente";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "idCliente", idCliente
            );

            await using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(
                    "No se encontró el cliente."
                );
            }

            string? Texto(string columna)
            {
                int i = reader.GetOrdinal(columna);
                return reader.IsDBNull(i)
                    ? null
                    : reader.GetString(i);
            }

            DateTime? Fecha(string columna)
            {
                int i = reader.GetOrdinal(columna);
                return reader.IsDBNull(i)
                    ? null
                    : reader.GetDateTime(i);
            }

            int? Entero(string columna)
            {
                int i = reader.GetOrdinal(columna);
                return reader.IsDBNull(i)
                    ? null
                    : reader.GetInt32(i);
            }

            TimeSpan? Hora(string columna)
            {
                int i = reader.GetOrdinal(columna);
                return reader.IsDBNull(i)
                    ? null
                    : reader.GetTimeSpan(i);
            }

            var modelo = new DashboardClienteCompleto
            {
                IdCliente = reader.GetInt32(
                    reader.GetOrdinal("IdCliente")),

                Nombre = Texto("Nombre") ?? "",
                Apellido = Texto("Apellido") ?? "",
                NombreCompleto =
                    Texto("NombreCompleto") ?? "",

                Correo = Texto("Correo"),
                Telefono = Texto("Telefono"),
                TipoCliente = Texto("TipoCliente"),

                PuntosFidelidad = reader.GetInt32(
                    reader.GetOrdinal("PuntosFidelidad")),

                TotalPedidos = reader.GetInt32(
                    reader.GetOrdinal("TotalPedidos")),

                Membresia = Texto("Membresia"),
                BeneficiosMembresia =
                    Texto("BeneficiosMembresia"),

                InicioMembresia =
                    Fecha("InicioMembresia"),
                FinMembresia =
                    Fecha("FinMembresia"),

                IdProximaCita =
                    Entero("IdProximaCita"),
                ProximoServicio =
                    Texto("ProximoServicio"),
                FechaProximaCita =
                    Fecha("FechaProximaCita"),
                HoraProximaCita =
                    Hora("HoraProximaCita"),
                EstadoProximaCita =
                    Texto("EstadoProximaCita"),
                EspecialistaProximaCita =
                    Texto("EspecialistaProximaCita"),
                SucursalProximaCita =
                    Texto("SucursalProximaCita"),

                CitasConfirmadas = reader.GetInt32(
                    reader.GetOrdinal("CitasConfirmadas")),

                CitasPendientes = reader.GetInt32(
                    reader.GetOrdinal("CitasPendientes")),

                CitasCanceladasMes = reader.GetInt32(
                    reader.GetOrdinal("CitasCanceladasMes")),

                HistorialJson =
                    Texto("HistorialJson") ?? "[]",

                PromocionesJson =
                    Texto("PromocionesJson") ?? "[]"
            };

            return View(modelo);
        }
    }
}
