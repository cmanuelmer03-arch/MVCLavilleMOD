using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lavillemvc.Models;
using Npgsql;

namespace Lavillemvc.DAL
{
    public class GestionRepositorio
    {
        private readonly Conexion _conexion;

        // Paleta fija para la dona de métodos de pago
        private static readonly string[] Paleta =
        {
            "#5a4035", // marrón (Efectivo)
            "#c8a96a", // dorado (Tarjeta Débito)
            "#a48b7d", // topo (Transferencia)
            "#e8c7c8", // rosa (Tarjeta Crédito)
            "#8fa88e", // verde seco (Pago Móvil)
            "#9b8aa8", // lila (Pagar en Sucursal)
            "#b8a888"  // arena (Tarjeta)
        };

        public GestionRepositorio(Conexion conexion)
        {
            _conexion = conexion;
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private async Task<NpgsqlConnection> AbrirAsync()
        {
            var conn = _conexion.GetConnection();
            await conn.OpenAsync();
            return conn;
        }

        private static string FormatoHora12(TimeOnly h)
        {
            var dt = DateTime.Today.Add(h.ToTimeSpan());
            return dt.ToString("h:mm tt");
        }

        private static string Iniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "?";
            var partes = nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1) return partes[0].Substring(0, 1).ToUpper();
            return (partes[0][0].ToString() + partes[^1][0].ToString()).ToUpper();
        }

        // ============================================================
        // PANEL — KPIs
        // ============================================================
        public async Task<KpiRecepcion> ObtenerKpisPanelAsync(int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM public.cita
                       WHERE fecha_cita = CURRENT_DATE
                         AND (@suc IS NULL OR id_sucursal = @suc)),
                    (SELECT COUNT(*) FROM public.cita
                       WHERE estado = 'Pendiente'
                         AND (@suc IS NULL OR id_sucursal = @suc)),
                    (SELECT COUNT(*) FROM public.cliente
                       WHERE fecha_registro::date >= DATE_TRUNC('month', CURRENT_DATE)),
                    (SELECT COALESCE(SUM(f.total),0)
                       FROM public.factura f
                       JOIN public.pago p ON p.id_pago = f.id_pago
                       LEFT JOIN public.cita c ON c.id_cita = p.id_cita
                       WHERE f.fecha = CURRENT_DATE
                         AND (@suc IS NULL OR c.id_sucursal = @suc));";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return new KpiRecepcion();

            return new KpiRecepcion
            {
                CitasHoy = r.GetInt32(0),
                Pendientes = r.GetInt32(1),
                ClientesNuevos = r.GetInt32(2),
                PagosDelDia = r.GetDecimal(3)
            };
        }

        // ============================================================
        // PANEL — Citas del día
        // ============================================================
        public async Task<List<CitaPanel>> ObtenerCitasDelDiaAsync(int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT c.id_cita,
                       c.hora_cita,
                       COALESCE(cl.nombre || ' ' || cl.apellido, c.nombre, 'Cliente'),
                       COALESCE(cl.tipo_cliente, 'Regular'),
                       COALESCE(s.nombre, '—'),
                       COALESCE(e.nombre || ' ' || e.apellido, 'Sin asignar'),
                       COALESCE(c.estado, 'Pendiente')
                FROM public.cita c
                LEFT JOIN public.cliente cl ON cl.id_cliente = c.id_cliente
                LEFT JOIN public.servicios s ON s.id_servicios = c.id_servicio
                LEFT JOIN public.empleado e ON e.id_empleado = c.id_empleado
                WHERE c.fecha_cita = CURRENT_DATE
                  AND (@suc IS NULL OR c.id_sucursal = @suc)
                ORDER BY c.hora_cita;";

            var lista = new List<CitaPanel>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var hora = r.GetFieldValue<TimeOnly>(1);
                var estado = r.GetString(6);

                lista.Add(new CitaPanel
                {
                    IdCita = r.GetInt32(0),
                    Hora = FormatoHora12(hora),
                    Cliente = r.GetString(2),
                    TipoCliente = r.GetString(3),
                    Servicio = r.GetString(4),
                    Especialista = r.GetString(5),
                    Estado = estado,
                    MostrarConfirmar = estado == "Pendiente"
                });
            }
            return lista;
        }

        // ============================================================
        // PANEL — Reservas por confirmar
        // ============================================================
        public async Task<List<ReservaPendiente>> ObtenerReservasPendientesAsync(int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT c.id_cita,
                       COALESCE(cl.nombre || ' ' || cl.apellido, c.nombre),
                       COALESCE(s.nombre, '—'),
                       c.fecha_cita,
                       c.hora_cita
                FROM public.cita c
                LEFT JOIN public.cliente cl ON cl.id_cliente = c.id_cliente
                LEFT JOIN public.servicios s ON s.id_servicios = c.id_servicio
                WHERE c.estado = 'Pendiente'
                  AND c.fecha_cita >= CURRENT_DATE
                  AND (@suc IS NULL OR c.id_sucursal = @suc)
                ORDER BY c.fecha_cita, c.hora_cita
                LIMIT 3;";

            var lista = new List<ReservaPendiente>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            await using var r = await cmd.ExecuteReaderAsync();
            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            while (await r.ReadAsync())
            {
                var fecha = r.GetDateTime(3);
                var hora = r.GetFieldValue<TimeOnly>(4);

                lista.Add(new ReservaPendiente
                {
                    IdCita = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Servicio = r.GetString(2),
                    Fecha = $"{dias[(int)fecha.DayOfWeek]} {fecha.Day} {meses[fecha.Month - 1]} — {FormatoHora12(hora)}"
                });
            }
            return lista;
        }

        // ============================================================
        // PANEL — Últimos registros
        // ============================================================
        public async Task<List<ClienteReciente>> ObtenerUltimosClientesAsync()
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT id_cliente,
                       nombre || ' ' || apellido,
                       COALESCE(fecha_registro, now())
                FROM public.cliente
                ORDER BY fecha_registro DESC NULLS LAST, id_cliente DESC
                LIMIT 5;";

            var lista = new List<ClienteReciente>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                var f = r.GetDateTime(2);
                lista.Add(new ClienteReciente
                {
                    IdCliente = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Tiempo = f.Date == DateTime.Today
                        ? "Hoy, " + f.ToString("h:mm tt")
                        : f.ToString("dd MMM yyyy")
                });
            }
            return lista;
        }

        // ============================================================
        // PANEL — Atención (usa indicadores_recepcion si existe)
        // ============================================================
        public async Task<AtencionResumen> ObtenerAtencionAsync(int? idSucursal)
        {
            await using var conn = await AbrirAsync();

            const string sqlInd = @"
                SELECT especialistas_activos,
                       especialistas_totales,
                       cabinas_ocupadas,
                       cabinas_totales,
                       espera_promedio_minutos
                FROM public.indicadores_recepcion
                WHERE id_sucursal = @suc
                LIMIT 1;";

            await using (var cmd = new NpgsqlCommand(sqlInd, conn))
            {
                cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);
                await using var r = await cmd.ExecuteReaderAsync();

                if (await r.ReadAsync())
                {
                    return new AtencionResumen
                    {
                        EspecialistasActivos = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                        EspecialistasTotal = r.IsDBNull(1) ? 0 : r.GetInt32(1),
                        CabinasOcupadas = r.IsDBNull(2) ? 0 : r.GetInt32(2),
                        CabinasTotal = r.IsDBNull(3) ? 0 : r.GetInt32(3),
                        EsperaPromedioMin = r.IsDBNull(4) ? 15 : r.GetInt32(4)
                    };
                }
            }

            const string sqlCalc = @"
                SELECT
                  (SELECT COUNT(*) FROM public.especialista e
                     JOIN public.empleado emp ON emp.id_empleado = e.id_empleado
                     WHERE e.disponibilidad = TRUE
                       AND (@suc IS NULL OR emp.id_sucursal = @suc)),
                  (SELECT COUNT(*) FROM public.especialista e
                     JOIN public.empleado emp ON emp.id_empleado = e.id_empleado
                     WHERE (@suc IS NULL OR emp.id_sucursal = @suc)),
                  (SELECT COUNT(*) FROM public.cita
                     WHERE fecha_cita = CURRENT_DATE
                       AND estado = 'Confirmada'
                       AND (@suc IS NULL OR id_sucursal = @suc)),
                  (SELECT COUNT(*) FROM public.cita
                     WHERE fecha_cita = CURRENT_DATE
                       AND (@suc IS NULL OR id_sucursal = @suc));";

            await using var cmd2 = new NpgsqlCommand(sqlCalc, conn);
            cmd2.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            await using var r2 = await cmd2.ExecuteReaderAsync();
            var at = new AtencionResumen();
            if (await r2.ReadAsync())
            {
                at.EspecialistasActivos = r2.GetInt32(0);
                at.EspecialistasTotal = r2.GetInt32(1);
                at.CabinasOcupadas = r2.GetInt32(2);
                at.CabinasTotal = r2.GetInt32(3);
                at.EsperaPromedioMin = 15;
            }
            return at;
        }

        // ============================================================
        // CALENDARIO — Citas de la semana
        // ============================================================
        public async Task<List<CitaCalendario>> ObtenerCitasSemanaAsync(
            DateTime inicio, DateTime fin, int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT c.id_cita, c.fecha_cita, c.hora_cita,
                       COALESCE(cl.nombre || ' ' || cl.apellido, c.nombre, '—'),
                       COALESCE(s.nombre, '—'),
                       COALESCE(e.nombre || ' ' || e.apellido, '—'),
                       COALESCE(c.estado, 'Pendiente')
                FROM public.cita c
                LEFT JOIN public.cliente cl ON cl.id_cliente = c.id_cliente
                LEFT JOIN public.servicios s ON s.id_servicios = c.id_servicio
                LEFT JOIN public.empleado e ON e.id_empleado = c.id_empleado
                WHERE c.fecha_cita BETWEEN @ini AND @fin
                  AND (@suc IS NULL OR c.id_sucursal = @suc)
                ORDER BY c.fecha_cita, c.hora_cita;";

            var lista = new List<CitaCalendario>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ini", inicio.Date);
            cmd.Parameters.AddWithValue("fin", fin.Date);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                lista.Add(new CitaCalendario
                {
                    IdCita = r.GetInt32(0),
                    Fecha = r.GetDateTime(1),
                    Hora = r.GetFieldValue<TimeOnly>(2),
                    Cliente = r.GetString(3),
                    Servicio = r.GetString(4),
                    Especialista = r.GetString(5),
                    Estado = r.GetString(6)
                });
            }
            return lista;
        }

        // ============================================================
        // CALENDARIO — Detalle de cita
        // ============================================================
        public async Task<CitaDetalle?> ObtenerDetalleCitaAsync(int idCita)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT c.id_cita,
                       COALESCE(cl.nombre || ' ' || cl.apellido, c.nombre, 'Cliente'),
                       COALESCE(m.nombre, ''),
                       COALESCE(s.nombre, '—'),
                       COALESCE(e.nombre || ' ' || e.apellido, 'Sin asignar'),
                       c.fecha_cita,
                       c.hora_cita,
                       COALESCE(c.estado, 'Pendiente')
                FROM public.cita c
                LEFT JOIN public.cliente cl ON cl.id_cliente = c.id_cliente
                LEFT JOIN public.servicios s ON s.id_servicios = c.id_servicio
                LEFT JOIN public.empleado e ON e.id_empleado = c.id_empleado
                LEFT JOIN public.identificador i ON i.id_cliente = cl.id_cliente
                                                AND i.estado = 'Activo'
                LEFT JOIN public.membresia m ON m.id_membresia = i.id_membresia
                WHERE c.id_cita = @id
                LIMIT 1;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", idCita);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            var fecha = r.GetDateTime(5);
            var hora = r.GetFieldValue<TimeOnly>(6);
            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            var cliente = r.GetString(1);
            return new CitaDetalle
            {
                IdCita = r.GetInt32(0),
                Cliente = cliente,
                Iniciales = Iniciales(cliente),
                Membresia = r.GetString(2),
                Servicio = r.GetString(3),
                Especialista = r.GetString(4),
                HorarioTexto = $"{dias[(int)fecha.DayOfWeek]}, {fecha.Day} {meses[fecha.Month - 1]} {fecha.Year} — {FormatoHora12(hora)}",
                Estado = r.GetString(7)
            };
        }

        // ============================================================
        // CLIENTES — Directorio
        // ============================================================
        public async Task<(List<ClienteDirectorio> Items, int Total)> ObtenerClientesAsync(
            string? buscar, int offset, int limite)
        {
            await using var conn = await AbrirAsync();

            string where = "";
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                where = @"WHERE (LOWER(cl.nombre || ' ' || cl.apellido) LIKE @q
                          OR LOWER(COALESCE(cl.correo,'')) LIKE @q
                          OR COALESCE(cl.telefono,'') LIKE @q)";
            }

            var sql = $@"
                SELECT cl.id_cliente,
                       cl.nombre || ' ' || cl.apellido,
                       COALESCE(cl.correo,''),
                       COALESCE(cl.telefono,''),
                       (SELECT MAX(fecha_cita) FROM public.cita
                          WHERE id_cliente = cl.id_cliente) AS ultima,
                       (SELECT COUNT(*) FROM public.cita
                          WHERE id_cliente = cl.id_cliente) AS total_citas,
                       COALESCE(m.nombre, '—') AS membresia
                FROM public.cliente cl
                LEFT JOIN public.identificador i ON i.id_cliente = cl.id_cliente
                                                AND i.estado = 'Activo'
                LEFT JOIN public.membresia m ON m.id_membresia = i.id_membresia
                {where}
                ORDER BY ultima DESC NULLS LAST, cl.id_cliente DESC
                OFFSET @off LIMIT @lim;";

            var lista = new List<ClienteDirectorio>();
            await using (var cmd = new NpgsqlCommand(sql, conn))
            {
                if (!string.IsNullOrWhiteSpace(buscar))
                    cmd.Parameters.AddWithValue("q", $"%{buscar.ToLowerInvariant()}%");
                cmd.Parameters.AddWithValue("off", offset);
                cmd.Parameters.AddWithValue("lim", limite);

                await using var r = await cmd.ExecuteReaderAsync();
                var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

                while (await r.ReadAsync())
                {
                    string ultima = "—";
                    if (!r.IsDBNull(4))
                    {
                        var f = r.GetDateTime(4);
                        ultima = $"{f.Day} {meses[f.Month - 1]} {f.Year}";
                    }

                    var membresia = r.GetString(6);
                    lista.Add(new ClienteDirectorio
                    {
                        IdCliente = r.GetInt32(0),
                        Nombre = r.GetString(1),
                        Email = r.GetString(2),
                        Telefono = r.GetString(3),
                        UltimaVisita = ultima,
                        Citas = r.GetInt32(5),
                        Membresia = membresia,
                        CssMembresia = membresia switch
                        {
                            var m when m.Contains("VIP") => "badge-platinum",
                            var m when m.Contains("Premium") => "badge-gold",
                            var m when m.Contains("Básico") => "badge-silver",
                            _ => "badge-none"
                        }
                    });
                }
            }

            var sqlTotal = $"SELECT COUNT(*) FROM public.cliente cl {where};";
            await using var cmdT = new NpgsqlCommand(sqlTotal, conn);
            if (!string.IsNullOrWhiteSpace(buscar))
                cmdT.Parameters.AddWithValue("q", $"%{buscar.ToLowerInvariant()}%");

            var total = Convert.ToInt32(await cmdT.ExecuteScalarAsync() ?? 0);
            return (lista, total);
        }

        // ============================================================
        // PAGOS — KPIs
        // ============================================================
        public async Task<KpiPagos> ObtenerKpisPagosAsync()
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT
                  (SELECT COALESCE(SUM(total),0) FROM public.factura
                     WHERE fecha = CURRENT_DATE),
                  (SELECT COALESCE(SUM(total),0) FROM public.factura
                     WHERE DATE_TRUNC('month', fecha) = DATE_TRUNC('month', CURRENT_DATE)),
                  (SELECT COUNT(*) FROM public.pago WHERE estado_pago = 'Pendiente'),
                  (SELECT COUNT(*) FROM public.factura WHERE fecha = CURRENT_DATE);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            var k = new KpiPagos();
            if (await r.ReadAsync())
            {
                k.IngresosDia = r.GetDecimal(0);
                k.IngresosMes = r.GetDecimal(1);
                k.Pendientes = r.GetInt32(2);
                k.TransaccionesHoy = r.GetInt32(3);
            }
            return k;
        }

        // ============================================================
        // PAGOS — Transacciones
        // ============================================================
        public async Task<List<PagoTransaccion>> ObtenerTransaccionesAsync(int limite = 20)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT f.id_factura,
                       f.fecha,
                       f.total,
                       COALESCE(p.metodo_pago, '—'),
                       COALESCE(p.estado_pago, 'Pendiente'),
                       COALESCE(e.nombre || ' ' || e.apellido, 'Equipo')
                FROM public.factura f
                JOIN public.pago p ON p.id_pago = f.id_pago
                LEFT JOIN public.cita c ON c.id_cita = p.id_cita
                LEFT JOIN public.empleado e ON e.id_empleado = c.id_empleado
                ORDER BY f.fecha DESC, f.hora DESC
                LIMIT @lim;";

            var lista = new List<PagoTransaccion>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("lim", limite);

            await using var r = await cmd.ExecuteReaderAsync();
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            while (await r.ReadAsync())
            {
                var fecha = r.GetDateTime(1);
                var estado = r.GetString(4);
                var total = r.GetDecimal(2);

                lista.Add(new PagoTransaccion
                {
                    IdFactura = r.GetInt32(0),
                    Factura = $"#F-{r.GetInt32(0):D4}",
                    Especialista = r.GetString(5),
                    Monto = $"Bs. {total:N0}",
                    Metodo = r.GetString(3),
                    Fecha = $"{fecha.Day} {meses[fecha.Month - 1]} {fecha.Year}",
                    Estado = estado,
                    BadgeClass = estado == "Pagado" ? "badge-pagado" : "badge-pendiente"
                });
            }
            return lista;
        }

        // ============================================================
        // PAGOS — Desglose por método (dona)
        // ============================================================
        public async Task<List<DesgloseMetodo>> ObtenerDesgloseMetodosAsync()
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT COALESCE(p.metodo_pago, 'Otro'),
                       SUM(f.total)
                FROM public.factura f
                JOIN public.pago p ON p.id_pago = f.id_pago
                WHERE DATE_TRUNC('month', f.fecha) = DATE_TRUNC('month', CURRENT_DATE)
                GROUP BY p.metodo_pago
                ORDER BY SUM(f.total) DESC;";

            var lista = new List<DesgloseMetodo>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();

            var total = 0m;
            var temp = new List<(string, decimal)>();
            while (await r.ReadAsync())
            {
                var monto = r.GetDecimal(1);
                temp.Add((r.GetString(0), monto));
                total += monto;
            }

            for (int i = 0; i < temp.Count; i++)
            {
                lista.Add(new DesgloseMetodo
                {
                    Metodo = temp[i].Item1,
                    Monto = temp[i].Item2,
                    Porcentaje = total > 0 ? Math.Round(temp[i].Item2 * 100 / total, 0) : 0,
                    ColorHex = Paleta[i % Paleta.Length]
                });
            }
            return lista;
        }

        // ============================================================
        // DROPDOWNS — Clientes
        // ============================================================
        public async Task<List<(int Id, string Nombre)>> ObtenerClientesSelectAsync()
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT id_cliente, nombre || ' ' || apellido
                FROM public.cliente
                ORDER BY nombre, apellido
                LIMIT 500;";

            var lista = new List<(int, string)>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                lista.Add((r.GetInt32(0), r.GetString(1)));
            return lista;
        }

        // ============================================================
        // DROPDOWNS — Servicios
        // ============================================================
        public async Task<List<(int Id, string Nombre)>> ObtenerServiciosSelectAsync()
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT id_servicios, nombre
                FROM public.servicios
                WHERE disponible = TRUE
                ORDER BY nombre;";

            var lista = new List<(int, string)>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                lista.Add((r.GetInt32(0), r.GetString(1)));
            return lista;
        }

        // ============================================================
        // DROPDOWNS — Especialistas
        // ============================================================
        public async Task<List<(int Id, string Nombre)>> ObtenerEspecialistasSelectAsync(int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                SELECT e.id_empleado, e.nombre || ' ' || e.apellido
                FROM public.empleado e
                WHERE e.cargo = 'Especialista'
                  AND (@suc IS NULL OR e.id_sucursal = @suc)
                ORDER BY e.nombre;";

            var lista = new List<(int, string)>();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                lista.Add((r.GetInt32(0), r.GetString(1)));
            return lista;
        }

        // ============================================================
        // ACCIONES — Confirmar cita
        // ============================================================
        public async Task<bool> ConfirmarCitaAsync(int idCita, int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                UPDATE public.cita
                SET estado = 'Confirmada'
                WHERE id_cita = @id
                  AND (@suc IS NULL OR id_sucursal = @suc)
                  AND estado <> 'Cancelada';";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", idCita);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ============================================================
        // ACCIONES — Cancelar cita
        // ============================================================
        public async Task<bool> CancelarCitaAsync(int idCita, int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                UPDATE public.cita
                SET estado = 'Cancelada'
                WHERE id_cita = @id
                  AND (@suc IS NULL OR id_sucursal = @suc);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", idCita);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ============================================================
        // ACCIONES — Insertar cliente
        // ============================================================
        public async Task<bool> InsertarClienteAsync(
            string nombre, string apellido,
            string? telefono, string? correo,
            int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                INSERT INTO public.cliente
                    (nombre, apellido, telefono, correo,
                     fecha_registro, id_sucursal_registro)
                VALUES
                    (@nom, @ape, @tel, @cor, NOW(), @suc);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("nom", nombre.Trim());
            cmd.Parameters.AddWithValue("ape", apellido.Trim());
            cmd.Parameters.AddWithValue("tel", (object?)telefono?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cor", (object?)correo?.Trim().ToLowerInvariant() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ============================================================
        // ACCIONES — Insertar cita
        // ============================================================
        public async Task<bool> InsertarCitaAsync(
            int idCliente, int idServicio, int? idEmpleado,
            DateTime fecha, TimeSpan hora, int? idSucursal)
        {
            await using var conn = await AbrirAsync();
            const string sql = @"
                INSERT INTO public.cita
                    (id_cliente, id_servicio, id_empleado,
                     fecha_cita, hora_cita, estado, id_sucursal)
                VALUES
                    (@cli, @ser, @emp, @fec, @hor, 'Pendiente', @suc);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("cli", idCliente);
            cmd.Parameters.AddWithValue("ser", idServicio);
            cmd.Parameters.AddWithValue("emp", (object?)idEmpleado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("fec", fecha.Date);
            cmd.Parameters.AddWithValue("hor", hora);
            cmd.Parameters.AddWithValue("suc", (object?)idSucursal ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ============================================================
        // ACCIONES — Insertar pago + factura (transacción)
        // ============================================================
        public async Task<bool> InsertarPagoAsync(
            int idCliente, decimal monto,
            string metodo, string estado)
        {
            await using var conn = await AbrirAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                const string sqlPago = @"
                    INSERT INTO public.pago
                        (id_cliente, subtotal, total, monto,
                         moneda, metodo_pago, estado_pago, fecha_pago)
                    VALUES
                        (@cli, @sub, @tot, @mon,
                         'USD', @met, @est, NOW())
                    RETURNING id_pago;";

                await using var cmdPago = new NpgsqlCommand(sqlPago, conn, tx);
                cmdPago.Parameters.AddWithValue("cli", idCliente);
                cmdPago.Parameters.AddWithValue("sub", monto);
                cmdPago.Parameters.AddWithValue("tot", monto);
                cmdPago.Parameters.AddWithValue("mon", monto);
                cmdPago.Parameters.AddWithValue("met", metodo);
                cmdPago.Parameters.AddWithValue("est", estado);

                var idPago = (int)(await cmdPago.ExecuteScalarAsync() ?? 0);
                if (idPago <= 0) { await tx.RollbackAsync(); return false; }

                const string sqlFact = @"
                    INSERT INTO public.factura (id_pago, total)
                    VALUES (@pago, @tot);";

                await using var cmdFact = new NpgsqlCommand(sqlFact, conn, tx);
                cmdFact.Parameters.AddWithValue("pago", idPago);
                cmdFact.Parameters.AddWithValue("tot", monto);

                await cmdFact.ExecuteNonQueryAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }
    }
}