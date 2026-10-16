
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Lavillemvc.Controllers
{
    public class AdminReportesController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AdminReportesController> _logger;

        public AdminReportesController(
            IConfiguration config,
            ILogger<AdminReportesController> logger)
        {
            _config = config;
            _logger = logger;
        }

        private NpgsqlConnection ConexionBD()
        {
            return new Conexion(_config).GetConnection();
        }

        private static void Fechas(
            NpgsqlCommand cmd,
            DateTime inicio,
            DateTime fin)
        {
            cmd.Parameters.Add(
                "inicio",
                NpgsqlDbType.Date).Value = inicio.Date;

            cmd.Parameters.Add(
                "fin",
                NpgsqlDbType.Date).Value = fin.Date;
        }

        private static string NormalizarEstado(string estado)
        {
            string valor = estado.Trim().ToLowerInvariant();

            return valor switch
            {
                "completada" or "completado"
                    or "finalizada" or "finalizado"
                    => "Completadas",

                "confirmada" or "confirmado"
                    => "Confirmadas",

                "cancelada" or "cancelado"
                    => "Canceladas",

                "pendiente" or "pendientes"
                    => "Pendientes",

                "no asistio" or "no asistió"
                    or "no-show" or "no show"
                    or "ausente"
                    => "No asistieron",

                _ => "Otros"
            };
        }

        private static async Task<(int Reservas, int Clientes)>
            ConsultarReservas(
                NpgsqlConnection conexion,
                DateTime inicio,
                DateTime fin)
        {
            const string sql = @"
                SELECT
                    COUNT(*)::INTEGER,
                    COUNT(DISTINCT ""IdCliente"")::INTEGER
                FROM public.vw_admin_reportes_citas
                WHERE ""FechaCita"" >= @inicio
                  AND ""FechaCita"" < @fin";

            using var cmd = new NpgsqlCommand(sql, conexion);

            Fechas(cmd, inicio, fin);

            using var r = await cmd.ExecuteReaderAsync();

            if (await r.ReadAsync())
            {
                return (
                    r.GetInt32(0),
                    r.GetInt32(1)
                );
            }

            return (0, 0);
        }

        private static async Task<(decimal Ingresos, int Pagos)>
            ConsultarIngresos(
                NpgsqlConnection conexion,
                DateTime inicio,
                DateTime fin)
        {
            const string sql = @"
                SELECT
                    COALESCE(SUM(""Importe""), 0),
                    COUNT(*)::INTEGER
                FROM public.vw_admin_reportes_pagos
                WHERE ""FechaPago"" >= @inicio
                  AND ""FechaPago"" < @fin
                  AND ""EsPagoConfirmado"" IS TRUE
                  AND ""Moneda"" = 'USD'
                  AND ""Importe"" > 0";

            using var cmd = new NpgsqlCommand(sql, conexion);

            Fechas(cmd, inicio, fin);

            using var r = await cmd.ExecuteReaderAsync();

            if (await r.ReadAsync())
            {
                return (
                    r.GetDecimal(0),
                    r.GetInt32(1)
                );
            }

            return (0, 0);
        }

        [HttpGet]
        public async Task<IActionResult> Datos(
            int anio = 0,
            int mes = 0)
        {
            try
            {
                DateTime hoy = DateTime.Today;

                if (anio == 0)
                    anio = hoy.Year;

                if (mes == 0)
                    mes = hoy.Month;

                if (anio < 2000 || anio > 2100 ||
                    mes < 1 || mes > 12)
                {
                    return BadRequest(new
                    {
                        mensaje = "El mes seleccionado no es válido."
                    });
                }

                DateTime inicio = new DateTime(anio, mes, 1);
                DateTime fin = inicio.AddMonths(1);
                DateTime anterior = inicio.AddMonths(-1);
                DateTime inicioGrafico = inicio.AddMonths(-5);

                var reporte = new AdminReportesGenerales
                {
                    Anio = anio,
                    Mes = mes
                };

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                // =====================================
                // 1. RESUMEN ACTUAL Y MES ANTERIOR
                // =====================================

                var reservasActuales = await ConsultarReservas(
                    conexion, inicio, fin);

                var reservasAnteriores = await ConsultarReservas(
                    conexion, anterior, inicio);

                var pagosActuales = await ConsultarIngresos(
                    conexion, inicio, fin);

                var pagosAnteriores = await ConsultarIngresos(
                    conexion, anterior, inicio);

                reporte.Reservas =
                    reservasActuales.Reservas;

                reporte.ClientesConReservas =
                    reservasActuales.Clientes;

                reporte.ReservasMesAnterior =
                    reservasAnteriores.Reservas;

                reporte.ClientesMesAnterior =
                    reservasAnteriores.Clientes;

                reporte.Ingresos =
                    pagosActuales.Ingresos;

                reporte.PagosConfirmados =
                    pagosActuales.Pagos;

                reporte.IngresosMesAnterior =
                    pagosAnteriores.Ingresos;

                // Sin capacidad/horarios reales por empleado
                // no se puede calcular ocupación fiable.
                reporte.TasaOcupacion = null;

                // =====================================
                // 2. INGRESOS DE LOS ÚLTIMOS 6 MESES
                // =====================================

                var cultura = CultureInfo.GetCultureInfo("es-SV");

                for (int i = 5; i >= 0; i--)
                {
                    DateTime fecha = inicio.AddMonths(-i);

                    reporte.IngresosMensuales.Add(
                        new AdminReporteMes
                        {
                            Anio = fecha.Year,
                            Mes = fecha.Month,
                            Etiqueta = cultura.TextInfo.ToTitleCase(
                                fecha.ToString("MMM", cultura)),
                            Ingresos = 0
                        });
                }

                const string sqlIngresos = @"
                    SELECT
                        EXTRACT(YEAR FROM ""FechaPago"")::INTEGER,
                        EXTRACT(MONTH FROM ""FechaPago"")::INTEGER,
                        COALESCE(SUM(""Importe""), 0)
                    FROM public.vw_admin_reportes_pagos
                    WHERE ""FechaPago"" >= @inicio
                      AND ""FechaPago"" < @fin
                      AND ""EsPagoConfirmado"" IS TRUE
                      AND ""Moneda"" = 'USD'
                      AND ""Importe"" > 0
                    GROUP BY
                        EXTRACT(YEAR FROM ""FechaPago""),
                        EXTRACT(MONTH FROM ""FechaPago"")
                    ORDER BY 1, 2";

                using (var cmd = new NpgsqlCommand(
                    sqlIngresos, conexion))
                {
                    Fechas(cmd, inicioGrafico, fin);

                    using var r = await cmd.ExecuteReaderAsync();

                    while (await r.ReadAsync())
                    {
                        int añoRegistro = r.GetInt32(0);
                        int mesRegistro = r.GetInt32(1);
                        decimal importe = r.GetDecimal(2);

                        var punto = reporte.IngresosMensuales
                            .FirstOrDefault(x =>
                                x.Anio == añoRegistro &&
                                x.Mes == mesRegistro);

                        if (punto != null)
                            punto.Ingresos = importe;
                    }
                }

                // =====================================
                // 3. SERVICIOS MÁS SOLICITADOS
                // =====================================

                const string sqlServicios = @"
                    SELECT
                        ""IdServicio"",
                        ""NombreServicio"",
                        COUNT(*)::INTEGER AS reservas
                    FROM public.vw_admin_reportes_servicios_reservados
                    WHERE ""FechaCita"" >= @inicio
                      AND ""FechaCita"" < @fin
                    GROUP BY
                        ""IdServicio"",
                        ""NombreServicio""
                    ORDER BY
                        reservas DESC,
                        ""NombreServicio""
                    LIMIT 6";

                using (var cmd = new NpgsqlCommand(
                    sqlServicios, conexion))
                {
                    Fechas(cmd, inicio, fin);

                    using var r = await cmd.ExecuteReaderAsync();

                    while (await r.ReadAsync())
                    {
                        reporte.ServiciosPopulares.Add(
                            new AdminReporteServicio
                            {
                                IdServicio = r.GetInt32(0),
                                Nombre = r.GetString(1),
                                Reservas = r.GetInt32(2)
                            });
                    }
                }

                // =====================================
                // 4. RENDIMIENTO DE SUCURSALES
                // =====================================

                const string sqlSucursales = @"
                    WITH reservas_actuales AS
                    (
                        SELECT
                            ""IdSucursal"",
                            COUNT(*)::INTEGER AS cantidad
                        FROM public.vw_admin_reportes_citas
                        WHERE ""FechaCita"" >= @inicio
                          AND ""FechaCita"" < @fin
                        GROUP BY ""IdSucursal""
                    ),
                    reservas_anteriores AS
                    (
                        SELECT
                            ""IdSucursal"",
                            COUNT(*)::INTEGER AS cantidad
                        FROM public.vw_admin_reportes_citas
                        WHERE ""FechaCita"" >= @anterior
                          AND ""FechaCita"" < @inicio
                        GROUP BY ""IdSucursal""
                    ),
                    pagos_actuales AS
                    (
                        SELECT
                            ""IdSucursal"",
                            COALESCE(SUM(""Importe""), 0)
                                AS ingresos
                        FROM public.vw_admin_reportes_pagos
                        WHERE ""FechaPago"" >= @inicio
                          AND ""FechaPago"" < @fin
                          AND ""EsPagoConfirmado"" IS TRUE
                          AND ""Moneda"" = 'USD'
                          AND ""Importe"" > 0
                        GROUP BY ""IdSucursal""
                    )
                    SELECT
                        s.id_sucursal,
                        s.nombre_sucursal,
                        COALESCE(ra.cantidad, 0),
                        COALESCE(rp.cantidad, 0),
                        COALESCE(pa.ingresos, 0),

                        COALESCE(
                            (
                                SELECT sr.""NombreServicio""
                                FROM public.vw_admin_reportes_servicios_reservados sr
                                WHERE sr.""IdSucursal"" = s.id_sucursal
                                  AND sr.""FechaCita"" >= @inicio
                                  AND sr.""FechaCita"" < @fin
                                GROUP BY
                                    sr.""IdServicio"",
                                    sr.""NombreServicio""
                                ORDER BY
                                    COUNT(*) DESC,
                                    sr.""NombreServicio""
                                LIMIT 1
                            ),
                            'Sin reservas'
                        )

                    FROM public.sucursal s

                    LEFT JOIN reservas_actuales ra
                        ON ra.""IdSucursal"" = s.id_sucursal

                    LEFT JOIN reservas_anteriores rp
                        ON rp.""IdSucursal"" = s.id_sucursal

                    LEFT JOIN pagos_actuales pa
                        ON pa.""IdSucursal"" = s.id_sucursal

                    ORDER BY s.id_sucursal";

                using (var cmd = new NpgsqlCommand(
                    sqlSucursales, conexion))
                {
                    Fechas(cmd, inicio, fin);

                    cmd.Parameters.Add(
                        "anterior",
                        NpgsqlDbType.Date).Value = anterior;

                    using var r = await cmd.ExecuteReaderAsync();

                    while (await r.ReadAsync())
                    {
                        reporte.Sucursales.Add(
                            new AdminReporteSucursal
                            {
                                IdSucursal = r.GetInt32(0),
                                Nombre = r.GetString(1),
                                Reservas = r.GetInt32(2),
                                ReservasMesAnterior = r.GetInt32(3),
                                Ingresos = r.GetDecimal(4),
                                ServicioTop = r.GetString(5),
                                Ocupacion = null,
                                Valoracion = null
                            });
                    }
                }

                // =====================================
                // 5. DISTRIBUCIÓN DE RESERVAS
                // =====================================

                const string sqlEstados = @"
                    SELECT
                        ""Estado"",
                        COUNT(*)::INTEGER
                    FROM public.vw_admin_reportes_citas
                    WHERE ""FechaCita"" >= @inicio
                      AND ""FechaCita"" < @fin
                    GROUP BY ""Estado""
                    ORDER BY ""Estado""";

                using (var cmd = new NpgsqlCommand(
                    sqlEstados, conexion))
                {
                    Fechas(cmd, inicio, fin);

                    using var r = await cmd.ExecuteReaderAsync();

                    while (await r.ReadAsync())
                    {
                        string estado = NormalizarEstado(
                            r.GetString(0));

                        int cantidad = r.GetInt32(1);

                        var existente = reporte.EstadosReservas
                            .FirstOrDefault(x =>
                                x.Estado == estado);

                        if (existente == null)
                        {
                            reporte.EstadosReservas.Add(
                                new AdminReporteEstado
                                {
                                    Estado = estado,
                                    Cantidad = cantidad
                                });
                        }
                        else
                        {
                            existente.Cantidad += cantidad;
                        }
                    }
                }

                // =====================================
                // 6. ACTIVIDAD POR FECHAS
                //
                // Se muestran citas programadas para
                // cada fecha y registros de clientes.
                // No se inventan auditorías o eventos.
                // =====================================

                const string sqlActividad = @"
                    WITH movimientos AS
                    (
                        SELECT
                            ""FechaCita"" AS dia,
                            1::INTEGER AS citas,
                            0::INTEGER AS registros
                        FROM public.vw_admin_reportes_citas
                        WHERE ""FechaCita"" >= @inicio
                          AND ""FechaCita"" < @fin

                        UNION ALL

                        SELECT
                            (
                                fecha_registro AT TIME ZONE
                                'America/El_Salvador'
                            )::DATE AS dia,
                            0::INTEGER AS citas,
                            1::INTEGER AS registros
                        FROM public.cliente
                        WHERE fecha_registro IS NOT NULL
                          AND (
                              fecha_registro AT TIME ZONE
                              'America/El_Salvador'
                          )::DATE >= @inicio
                          AND (
                              fecha_registro AT TIME ZONE
                              'America/El_Salvador'
                          )::DATE < @fin
                    )
                    SELECT
                        dia,
                        SUM(citas)::INTEGER,
                        SUM(registros)::INTEGER
                    FROM movimientos
                    GROUP BY dia
                    ORDER BY dia DESC
                    LIMIT 5";

                using (var cmd = new NpgsqlCommand(
                    sqlActividad, conexion))
                {
                    Fechas(cmd, inicio, fin);

                    using var r = await cmd.ExecuteReaderAsync();

                    while (await r.ReadAsync())
                    {
                        reporte.Actividad.Add(
                            new AdminReporteActividad
                            {
                                Fecha = r.GetDateTime(0),
                                CitasProgramadas = r.GetInt32(1),
                                ClientesRegistrados = r.GetInt32(2)
                            });
                    }
                }

                return Json(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cargando Reportes Generales");

                return StatusCode(500, new
                {
                    mensaje =
                        "No se pudieron consultar los reportes: "
                        + ex.Message
                });
            }
        }
    }
}
