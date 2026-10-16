
using Lavillemvc.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lavillemvc.DAL
{
    public class GerenteDashboardDAL
    {
        private readonly Conexion _conexion;

        public GerenteDashboardDAL(
            IConfiguration configuration)
        {
            _conexion = new Conexion(configuration);
        }

        public async Task<GerenteDashboardSucursal?>
            ObtenerDashboardAsync(int idSucursal)
        {
            using var conexion = _conexion.GetConnection();

            await conexion.OpenAsync();

            const string sql = @"
                SELECT *
                FROM public.vw_gerente_dashboard_sucursal
                WHERE ""IdSucursal"" = @IdSucursal
                LIMIT 1";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "IdSucursal", idSucursal);

            using var reader =
                await comando.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            // Lectura segura de columnas
            string Texto(string columna)
            {
                int i = reader.GetOrdinal(columna);

                return reader.IsDBNull(i)
                    ? ""
                    : Convert.ToString(reader.GetValue(i)) ?? "";
            }

            int Numero(string columna)
            {
                int i = reader.GetOrdinal(columna);

                return reader.IsDBNull(i)
                    ? 0
                    : Convert.ToInt32(reader.GetValue(i));
            }

            decimal Dinero(string columna)
            {
                int i = reader.GetOrdinal(columna);

                return reader.IsDBNull(i)
                    ? 0
                    : Convert.ToDecimal(reader.GetValue(i));
            }

            bool? Booleano(string columna)
            {
                int i = reader.GetOrdinal(columna);

                return reader.IsDBNull(i)
                    ? null
                    : reader.GetBoolean(i);
            }

            // Llenar el modelo con datos reales
            var modelo = new GerenteDashboardSucursal
            {
                IdSucursal =
                    Numero("IdSucursal"),

                NombreSucursal =
                    Texto("NombreSucursal"),

                Direccion =
                    Texto("Direccion"),

                Abierto =
                    Booleano("Abierto"),

                ReservasHoy =
                    Numero("ReservasHoy"),

                CitasConfirmadas =
                    Numero("CitasConfirmadas"),

                CitasPendientes =
                    Numero("CitasPendientes"),

                IngresosCitasMes =
                    Dinero("IngresosCitasMes"),

                ClientesMes =
                    Numero("ClientesMes"),

                TotalEmpleados =
                    Numero("TotalEmpleados"),

                PromocionesActivas =
                    Numero("PromocionesActivas"),

                ServiciosDisponibles =
                    Numero("ServiciosDisponibles")
            };

            return modelo;
        }
    }
}
