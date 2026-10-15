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

        public async Task<IActionResult> DashboardCliente()
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
                SELECT *
                FROM public.vw_dashboard_cliente_completo
                WHERE ""IdCliente"" = @IdCliente
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@IdCliente", 1);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("No se encontraron datos para el cliente.");
            }

            var modelo = new DashboardClienteCompleto
            {
                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                Nombre = reader["Nombre"]?.ToString() ?? "",
                Apellido = reader["Apellido"]?.ToString() ?? "",
                NombreCompleto = reader["NombreCompleto"]?.ToString() ?? "",
                Correo = reader["Correo"]?.ToString(),
                Telefono = reader["Telefono"]?.ToString(),
                TipoCliente = reader["TipoCliente"]?.ToString(),

                PuntosFidelidad = reader["PuntosFidelidad"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["PuntosFidelidad"]),

                TotalPedidos = reader["TotalPedidos"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["TotalPedidos"]),

                Membresia = reader["Membresia"]?.ToString(),
                BeneficiosMembresia = reader["BeneficiosMembresia"]?.ToString(),

                InicioMembresia = reader["InicioMembresia"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["InicioMembresia"]),

                FinMembresia = reader["FinMembresia"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["FinMembresia"]),

                IdProximaCita = reader["IdProximaCita"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["IdProximaCita"]),

                ProximoServicio = reader["ProximoServicio"]?.ToString(),

                FechaProximaCita = reader["FechaProximaCita"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["FechaProximaCita"]),

                HoraProximaCita = reader["HoraProximaCita"] == DBNull.Value
                    ? null
                    : (TimeSpan?)reader["HoraProximaCita"],

                EstadoProximaCita = reader["EstadoProximaCita"]?.ToString(),
                EspecialistaProximaCita = reader["EspecialistaProximaCita"]?.ToString(),
                SucursalProximaCita = reader["SucursalProximaCita"]?.ToString(),

                CitasConfirmadas = reader["CitasConfirmadas"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["CitasConfirmadas"]),

                CitasPendientes = reader["CitasPendientes"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["CitasPendientes"]),

                CitasCanceladasMes = reader["CitasCanceladasMes"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["CitasCanceladasMes"]),

                HistorialJson = reader["HistorialJson"]?.ToString() ?? "[]",
                PromocionesJson = reader["PromocionesJson"]?.ToString() ?? "[]"
            };

            return View(modelo);
        }

        public IActionResult MisReservas()
        {
            return View();
        }

        public IActionResult Pago()
        {
            return View();
        }

        public IActionResult PerfilCliente()
        {
            return View();
        }

        public IActionResult Promociones()
        {
            return View();
        }

        public IActionResult ReservarCita(
            string especialista,
            string servicio,
            string sucursal)
        {
            ViewBag.Especialista = especialista;
            ViewBag.Servicio = servicio;
            ViewBag.Sucursal = sucursal;

            return View();
        }
    }
}