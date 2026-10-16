using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Lavillemvc.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace Lavillemvc.DAL
{
    public class GerentePromocionesDAL
    {
        private readonly Conexion _conexion;

        public GerentePromocionesDAL(IConfiguration configuration)
        {
            _conexion = new Conexion(configuration);
        }

        public async Task<GerentePromocionesPagina> ObtenerAsync(int idSucursal)
        {
            var pagina = new GerentePromocionesPagina { IdSucursal = idSucursal };
            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();

            // La consulta usa la vista SQL ya creada por el usuario.
            // La tabla auxiliar SOLO guarda pausa, archivo e imagen; no altera las promociones.
            const string sql = @"
                SELECT v.*,
                       COALESCE(c.pausada, FALSE) AS auxiliar_pausada,
                       COALESCE(c.archivada, FALSE) AS auxiliar_archivada,
                       c.imagen_url AS auxiliar_imagen_url
                FROM public.vw_gerente_gestion_promociones v
                LEFT JOIN public.gerente_promocion_control c
                    ON c.id_promocion = v.""IdPromocion""
                WHERE v.""IdSucursal"" = @sucursal
                ORDER BY v.""FechaInicio"" DESC NULLS LAST, v.""IdPromocion"" DESC";

            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("sucursal", idSucursal);
            await using var r = await cmd.ExecuteReaderAsync();

            string? Cadena(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));
            }
            int Entero(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i));
            }
            int? EnteroN(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? null : Convert.ToInt32(r.GetValue(i));
            }
            decimal? DecimalN(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? null : Convert.ToDecimal(r.GetValue(i));
            }
            DateTime? FechaN(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? null : r.GetDateTime(i);
            }
            bool? BoolN(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? null : r.GetBoolean(i);
            }
            DateOnly FechaSolo(string c)
            {
                int i = r.GetOrdinal(c);
                return r.IsDBNull(i) ? default : r.GetFieldValue<DateOnly>(i);
            }

            while (await r.ReadAsync())
            {
                var datos = new GerenteGestionPromociones
                {
                    IdPromocion = Entero("IdPromocion"),
                    IdSucursal = Entero("IdSucursal"),
                    NombreSucursal = Cadena("NombreSucursal") ?? "",
                    SucursalAbierta = BoolN("SucursalAbierta"),
                    Nombre = Cadena("Nombre") ?? "",
                    Descripcion = Cadena("Descripcion"),
                    Descuento = DecimalN("Descuento"),
                    FechaInicio = FechaN("FechaInicio"),
                    FechaFin = FechaN("FechaFin"),
                    EstadoPromocion = Cadena("EstadoPromocion") ?? "Sin definir",
                    FechaReferencia = FechaSolo("FechaReferencia"),
                    MesReferencia = FechaSolo("MesReferencia"),
                    ImagenUrl = Cadena("ImagenUrl"),
                    ServiciosRelacionadosTexto = Cadena("ServiciosRelacionadosTexto") ?? "",
                    ServiciosRelacionadosJson = Cadena("ServiciosRelacionadosJson") ?? "[]",
                    TotalServiciosRelacionados = Entero("TotalServiciosRelacionados"),
                    TotalUsos = EnteroN("TotalUsos"),
                    IngresosGenerados = DecimalN("IngresosGenerados"),
                    TotalPromociones = Entero("TotalPromociones"),
                    PromocionesActivas = Entero("PromocionesActivas"),
                    PromocionesProgramadas = Entero("PromocionesProgramadas"),
                    PromocionesFinalizadas = Entero("PromocionesFinalizadas"),
                    ClientesBeneficiadosMes = EnteroN("ClientesBeneficiadosMes"),
                    IngresosPromocionesMes = DecimalN("IngresosPromocionesMes")
                };

                try
                {
                    datos.ServiciosRelacionados =
                        JsonSerializer.Deserialize<List<GerenteGestionPromociones.ServicioPromocion>>(
                            datos.ServiciosRelacionadosJson) ?? new();
                }
                catch (JsonException)
                {
                    datos.ServiciosRelacionados = new();
                }

                pagina.NombreSucursal = datos.NombreSucursal;
                pagina.Promociones.Add(new PromocionGerenteFila
                {
                    Datos = datos,
                    Pausada = BoolN("auxiliar_pausada") ?? false,
                    Archivada = BoolN("auxiliar_archivada") ?? false,
                    ImagenPersonalizada = Cadena("auxiliar_imagen_url")
                });
            }

            // Una sucursal sin promociones no tendrá filas en la vista.
            // Consultamos su nombre por separado para mantener la cabecera correcta.
            if (string.IsNullOrWhiteSpace(pagina.NombreSucursal))
            {
                await r.DisposeAsync();
                await using var nombreCmd = new NpgsqlCommand(
                    "SELECT nombre_sucursal FROM public.sucursal WHERE id_sucursal = @sucursal",
                    conexion);
                nombreCmd.Parameters.AddWithValue("sucursal", idSucursal);
                pagina.NombreSucursal = Convert.ToString(await nombreCmd.ExecuteScalarAsync())
                    ?? "Sucursal no encontrada";
            }
            return pagina;
        }

        public async Task GuardarAsync(int idSucursal, GerentePromocionFormulario datos)
        {
            await using var con = _conexion.GetConnection();
            await con.OpenAsync();
            await using var transaccion = await con.BeginTransactionAsync();
            int id;
            if (datos.IdPromocion is > 0)
            {
                id = datos.IdPromocion.Value;
                const string actualizar = @"
                    UPDATE public.promocion
                    SET nombre = @nombre, descripcion = @descripcion,
                        descuento = @descuento, fecha_inicio = @inicio, fecha_fin = @fin
                    WHERE id_promocion = @id AND id_sucursal = @sucursal";
                await using var cmd = new NpgsqlCommand(actualizar, con, transaccion);
                ParametrosPromocion(cmd, datos, idSucursal);
                cmd.Parameters.AddWithValue("id", id);
                if (await cmd.ExecuteNonQueryAsync() != 1)
                    throw new InvalidOperationException("La promoción no pertenece a esta sucursal o no existe.");
            }
            else
            {
                // Id manual bajo bloqueo, conforme a otros controladores del proyecto.
                // Si el esquema usa secuencia identity, sincronizamos la secuencia después.
                await using (var bloqueo = new NpgsqlCommand(
                    "SELECT pg_advisory_xact_lock(42174, 3)", con, transaccion))
                    await bloqueo.ExecuteNonQueryAsync();

                const string insertar = @"
                    INSERT INTO public.promocion
                        (id_promocion, id_sucursal, nombre, descripcion,
                         descuento, fecha_inicio, fecha_fin)
                    SELECT COALESCE(MAX(id_promocion),0) + 1,
                           @sucursal, @nombre, @descripcion,
                           @descuento, @inicio, @fin
                    FROM public.promocion
                    RETURNING id_promocion";
                await using var cmd = new NpgsqlCommand(insertar, con, transaccion);
                ParametrosPromocion(cmd, datos, idSucursal);
                id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // El esquema compartido utiliza GENERATED BY DEFAULT AS IDENTITY.
                // Evita que un INSERT futuro con ID automático reutilice IDs explícitos.
                const string sincronizar = @"
                    SELECT setval(pg_get_serial_sequence('public.promocion','id_promocion'),
                                  (SELECT COALESCE(MAX(id_promocion),1) FROM public.promocion), true)";
                await using var secuencia = new NpgsqlCommand(sincronizar, con, transaccion);
                await secuencia.ExecuteNonQueryAsync();
            }

            const string imagen = @"
                INSERT INTO public.gerente_promocion_control (id_promocion, imagen_url)
                VALUES (@id, @imagen)
                ON CONFLICT (id_promocion)
                DO UPDATE SET imagen_url = EXCLUDED.imagen_url,
                              actualizado_en = CURRENT_TIMESTAMP";
            await using (var cmd = new NpgsqlCommand(imagen, con, transaccion))
            {
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.Add("imagen", NpgsqlDbType.Varchar).Value =
                    (object?)datos.ImagenUrl ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }
            await transaccion.CommitAsync();
        }

        public Task PausarAsync(int idSucursal, int idPromocion, bool pausada) =>
            CambiarEstadoAsync(idSucursal, idPromocion, "pausada", pausada);

        public Task ArchivarAsync(int idSucursal, int idPromocion, bool archivada) =>
            CambiarEstadoAsync(idSucursal, idPromocion, "archivada", archivada);

        private async Task CambiarEstadoAsync(int idSucursal, int idPromocion,
            string campo, bool valor)
        {
            if (campo != "pausada" && campo != "archivada")
                throw new ArgumentException("Campo inválido.");

            await using var con = _conexion.GetConnection();
            await con.OpenAsync();
            // El identificador de columna procede EXCLUSIVAMENTE de las dos constantes permitidas.
            string sql = $@"
                INSERT INTO public.gerente_promocion_control (id_promocion, {campo})
                SELECT p.id_promocion, @valor
                FROM public.promocion p
                WHERE p.id_promocion = @id AND p.id_sucursal = @sucursal
                ON CONFLICT (id_promocion)
                DO UPDATE SET {campo} = EXCLUDED.{campo},
                              actualizado_en = CURRENT_TIMESTAMP";
            await using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("valor", valor);
            cmd.Parameters.AddWithValue("id", idPromocion);
            cmd.Parameters.AddWithValue("sucursal", idSucursal);
            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException("La promoción no existe o pertenece a otra sucursal.");
        }

        private static void ParametrosPromocion(NpgsqlCommand cmd,
            GerentePromocionFormulario datos, int sucursal)
        {
            cmd.Parameters.AddWithValue("sucursal", sucursal);
            cmd.Parameters.AddWithValue("nombre", datos.Nombre.Trim());
            cmd.Parameters.Add("descripcion", NpgsqlDbType.Varchar).Value =
                (object?)datos.Descripcion?.Trim() ?? DBNull.Value;
            cmd.Parameters.Add("descuento", NpgsqlDbType.Numeric).Value =
                (object?)datos.Descuento ?? DBNull.Value;
            cmd.Parameters.Add("inicio", NpgsqlDbType.Timestamp).Value =
                DateTime.SpecifyKind(datos.FechaInicio!.Value.Date, DateTimeKind.Unspecified);
            cmd.Parameters.Add("fin", NpgsqlDbType.Timestamp).Value =
                DateTime.SpecifyKind(datos.FechaFin!.Value.Date, DateTimeKind.Unspecified);
        }
    }
}
