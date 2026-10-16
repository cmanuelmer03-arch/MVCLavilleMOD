using Microsoft.AspNetCore.Mvc;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Npgsql;
using System.Text.Json;

namespace Lavillemvc.Controllers
{
    public class ClienteController : Controller
    {
        private readonly Conexion _conexion;

        public ClienteController(IConfiguration configuration)
        {
            _conexion = new Conexion(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> PerfilCliente()
        {
            int idCliente = 1;

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT
            c.id_cliente,
            c.nombre,
            c.apellido,
            c.correo,
            c.telefono,
            c.fecha_nacimiento,
            c.direccion,
            c.foto_perfil_url,

            p.tipo_cabello,
            p.id_especialista_preferido,
            p.id_sucursal_favorita,
            p.alergias_condiciones,
            p.notificaciones_correo,
            p.recordatorios_sms,
            p.promociones_especiales,
            p.boletin_mensual,
            p.idioma

        FROM public.cliente c

        LEFT JOIN public.preferencias_cliente p
            ON p.id_cliente = c.id_cliente

        WHERE c.id_cliente = @idCliente
        LIMIT 1;
    ";

            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@idCliente",
                idCliente);

            await using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound();
            }

            PerfilCliente modelo = new PerfilCliente
            {
                IdCliente = Convert.ToInt32(
                    reader["id_cliente"]),

                Nombre = reader["nombre"]?.ToString() ?? "",

                Apellido = reader["apellido"]?.ToString() ?? "",

                Correo = reader["correo"]?.ToString(),

                Telefono = reader["telefono"]?.ToString(),

                FechaNacimiento =
    reader["fecha_nacimiento"] == DBNull.Value
        ? null
        : ((DateOnly)reader["fecha_nacimiento"])
            .ToDateTime(TimeOnly.MinValue),

                Direccion = reader["direccion"]?.ToString(),

                FotoPerfilUrl =
                    reader["foto_perfil_url"]?.ToString(),

                TipoCabello =
                    reader["tipo_cabello"]?.ToString(),

                IdEspecialistaPreferido =
                    reader["id_especialista_preferido"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["id_especialista_preferido"]),

                IdSucursalFavorita =
                    reader["id_sucursal_favorita"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["id_sucursal_favorita"]),

                AlergiasCondiciones =
                    reader["alergias_condiciones"]?.ToString(),

                NotificacionesCorreo =
                    reader["notificaciones_correo"] != DBNull.Value
                        && Convert.ToBoolean(
                            reader["notificaciones_correo"]),

                RecordatoriosSms =
                    reader["recordatorios_sms"] != DBNull.Value
                        && Convert.ToBoolean(
                            reader["recordatorios_sms"]),

                PromocionesEspeciales =
                    reader["promociones_especiales"] != DBNull.Value
                        && Convert.ToBoolean(
                            reader["promociones_especiales"]),

                BoletinMensual =
                    reader["boletin_mensual"] != DBNull.Value
                        && Convert.ToBoolean(
                            reader["boletin_mensual"]),

                Idioma =
                    reader["idioma"]?.ToString() ?? "es"
            };

            return View(modelo);
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
    : ((DateOnly)reader["FechaProximaCita"])
        .ToDateTime(TimeOnly.MinValue),

                HoraProximaCita = reader["HoraProximaCita"] == DBNull.Value
    ? null
    : ((TimeOnly)reader["HoraProximaCita"]).ToTimeSpan(),

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

        public async Task<IActionResult> MisReservas()
        {
            int idCliente = 1;

            List<ClienteProcesoPago> reservas =
                new List<ClienteProcesoPago>();

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT
            ""IdCita"",
            ""IdCliente"",
            ""EstadoReserva"",
            ""ServicioPrincipal"",
            ""NombreEspecialista"",
            ""NombreSucursal"",
            ""FechaCita"",
            ""HoraCita""
        FROM public.vw_proceso_pago_cita
        WHERE ""IdCliente"" = @IdCliente
        ORDER BY ""FechaCita"", ""HoraCita"";
    ";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@IdCliente",
                idCliente);

            await using var reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ClienteProcesoPago reserva =
                    new ClienteProcesoPago();

                reserva.IdCita =
                    Convert.ToInt32(reader["IdCita"]);

                reserva.IdCliente =
                    Convert.ToInt32(reader["IdCliente"]);

                reserva.EstadoReserva =
                    reader["EstadoReserva"]?.ToString();

                reserva.ServicioPrincipal =
                    reader["ServicioPrincipal"]?.ToString()
                    ?? "";

                reserva.NombreEspecialista =
                    reader["NombreEspecialista"]?.ToString();

                reserva.NombreSucursal =
                    reader["NombreSucursal"]?.ToString();

                reserva.FechaCita =
                    (DateOnly)reader["FechaCita"];

                reserva.HoraCita =
                    (TimeOnly)reader["HoraCita"];

                reservas.Add(reserva);
            }

            return View(reservas);
        }

        [HttpPost]
        public async Task<IActionResult> CancelarReserva(int idCita)
        {
            int idCliente = 1;

            if (idCita <= 0)
            {
                return BadRequest();
            }

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        UPDATE public.cita
        SET estado = 'Cancelada'
        WHERE id_cita = @IdCita
          AND id_cliente = @IdCliente
          AND LOWER(COALESCE(estado, '')) IN
              ('pendiente', 'confirmada');
    ";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@IdCita",
                idCita);

            cmd.Parameters.AddWithValue(
                "@IdCliente",
                idCliente);

            await cmd.ExecuteNonQueryAsync();

            return RedirectToAction(
                nameof(MisReservas));
        }

        [HttpGet]
        public async Task<IActionResult> Pago(int idCita, int? idPromocion)
        {
            if (idCita <= 0)
            {
                return BadRequest();
            }

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
    SELECT
        p.*,
        m.nombre AS ""NombreMembresiaBD"",
        COALESCE(
            substring(m.beneficios FROM '([0-9]+)%'),
            '0'
        )::numeric AS ""DescuentoMembresiaBD"",
        promo.nombre AS ""NombrePromocionBD"",
        promo.descuento AS ""DescuentoPromocionBD""
    FROM public.vw_proceso_pago_cita p
    LEFT JOIN public.identificador i
        ON i.id_cliente = p.""IdCliente""
        AND LOWER(i.estado) = 'activo'
        AND p.""FechaCita"" >= i.fecha_inicio::date
        AND p.""FechaCita"" <= i.fecha_fin::date
    LEFT JOIN public.membresia m
        ON m.id_membresia = i.id_membresia
        AND m.activo = true
    LEFT JOIN public.promocion promo
        ON promo.id_promocion = @idPromocion
        AND promo.id_sucursal = p.""IdSucursal""
    WHERE p.""IdCita"" = @idCita
    LIMIT 1;
";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@idCita",
                idCita);

            cmd.Parameters.AddWithValue(
    "@idPromocion",
    idPromocion.HasValue
        ? idPromocion.Value
        : (object)DBNull.Value);


            await using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound();
            }

            ClienteProcesoPago modelo =
                new ClienteProcesoPago();

            // =====================================
            // CITA
            // =====================================

            modelo.IdCita =
                Convert.ToInt32(reader["IdCita"]);

            modelo.IdCliente =
                Convert.ToInt32(reader["IdCliente"]);

            modelo.EstadoReserva =
                reader["EstadoReserva"]?.ToString();


            // =====================================
            // CLIENTE
            // =====================================

            modelo.NombreCliente =
                reader["NombreCliente"]?.ToString()
                ?? "";

            modelo.CorreoCliente =
                reader["CorreoCliente"]?.ToString();


            // =====================================
            // SERVICIO
            // =====================================

            modelo.IdServicio =
                Convert.ToInt32(reader["IdServicio"]);

            modelo.ServicioPrincipal =
                reader["ServicioPrincipal"]?.ToString()
                ?? "";

            modelo.ServiciosContratados =
                reader["ServiciosContratados"]?.ToString();

            modelo.PrecioBaseServicio =
                Convert.ToDecimal(
                    reader["PrecioBaseServicio"]);

            modelo.Subtotal =
                Convert.ToDecimal(
                    reader["SubtotalServicios"]);


            // =====================================
            // ESPECIALISTA
            // =====================================

            modelo.IdEspecialista =
                reader["IdEspecialista"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(
                        reader["IdEspecialista"]);

            modelo.NombreEspecialista =
                reader["NombreEspecialista"]?.ToString();


            // =====================================
            // SUCURSAL
            // =====================================

            modelo.IdSucursal =
                reader["IdSucursal"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(
                        reader["IdSucursal"]);

            modelo.NombreSucursal =
                reader["NombreSucursal"]?.ToString();

            modelo.DireccionSucursal =
                reader["DireccionSucursal"]?.ToString();


            // =====================================
            // FECHA Y HORA
            // =====================================

            if (reader["FechaCita"] != DBNull.Value)
            {
                modelo.FechaCita =
                    (DateOnly)reader["FechaCita"];
            }

            if (reader["HoraCita"] != DBNull.Value)
            {
                modelo.HoraCita =
                    (TimeOnly)reader["HoraCita"];
            }


            // =====================================
            // DURACIÓN
            // =====================================

            modelo.DuracionEstimada =
                reader["DuracionEstimada"]?.ToString();


            // =====================================
            // MEMBRESÍA
            // =====================================

            if (reader["NombreMembresiaBD"] != DBNull.Value)
            {
                modelo.NombreMembresia =
                    reader["NombreMembresiaBD"]?.ToString();
            }

            decimal descuentoPorcentaje = 0;

            if (reader["DescuentoMembresiaBD"] != DBNull.Value)
            {
                descuentoPorcentaje =
                    Convert.ToDecimal(
                        reader["DescuentoMembresiaBD"]);
            }

            if (descuentoPorcentaje > 0)
            {
                modelo.DescuentoMembresiaRegistrado =
                    modelo.Subtotal *
                    descuentoPorcentaje / 100m;
            }
            else
            {
                modelo.DescuentoMembresiaRegistrado = null;
            }
            // =====================================
            // PROMOCIÓN
            // =====================================

            decimal descuentoPromocion = 0;

            if (reader["DescuentoPromocionBD"] != DBNull.Value)
            {
                decimal porcentajePromocion =
                    Convert.ToDecimal(
                        reader["DescuentoPromocionBD"]);

                if (porcentajePromocion > 0)
                {
                    descuentoPromocion =
                        modelo.Subtotal *
                        porcentajePromocion / 100m;

                    modelo.DescuentoGeneralRegistrado =
                        descuentoPromocion;
                }
            }

            // =====================================
            // PUNTOS
            // =====================================

            modelo.PuntosActuales =
                reader["PuntosActuales"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(
                        reader["PuntosActuales"]);


            // =====================================
            // CALCULAR TOTAL CON MEMBRESÍA
            // =====================================

            decimal totalConMembresia =
    modelo.Subtotal
    - (modelo.DescuentoMembresiaRegistrado ?? 0)
    - (modelo.DescuentoGeneralRegistrado ?? 0);

            if (totalConMembresia < 0)
            {
                totalConMembresia = 0;
            }


            // =====================================
            // PUNTOS POR GANAR
            // =====================================

            modelo.PuntosPorGanar =
                (int)Math.Floor(
                    totalConMembresia / 10m);


            // =====================================
            // TOTAL INICIAL
            // =====================================

            modelo.TotalAPagar =
                totalConMembresia;


            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPerfil(PerfilCliente modelo)
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Actualizar datos personales
                string sqlCliente = @"
            UPDATE public.cliente
            SET
                nombre = @Nombre,
                apellido = @Apellido,
                correo = @Correo,
                telefono = @Telefono,
                fecha_nacimiento = @FechaNacimiento,
                direccion = @Direccion,
                foto_perfil_url = @FotoPerfilUrl
            WHERE id_cliente = @IdCliente;
        ";

                await using (var cmd = new NpgsqlCommand(sqlCliente, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", modelo.IdCliente);
                    cmd.Parameters.AddWithValue("@Nombre", modelo.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Apellido", modelo.Apellido ?? "");
                    cmd.Parameters.AddWithValue("@Correo",
                        (object?)modelo.Correo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono",
                        (object?)modelo.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaNacimiento",
                        (object?)modelo.FechaNacimiento ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion",
                        (object?)modelo.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FotoPerfilUrl",
                        (object?)modelo.FotoPerfilUrl ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Actualizar preferencias
                string sqlPreferencias = @"
            INSERT INTO public.preferencias_cliente
            (
                id_cliente,
                tipo_cabello,
                id_especialista_preferido,
                id_sucursal_favorita,
                alergias_condiciones,
                notificaciones_correo,
                recordatorios_sms,
                promociones_especiales,
                boletin_mensual,
                idioma
            )
            VALUES
            (
                @IdCliente,
                @TipoCabello,
                @IdEspecialistaPreferido,
                @IdSucursalFavorita,
                @AlergiasCondiciones,
                @NotificacionesCorreo,
                @RecordatoriosSms,
                @PromocionesEspeciales,
                @BoletinMensual,
                @Idioma
            )
            ON CONFLICT (id_cliente)
            DO UPDATE SET
                tipo_cabello = EXCLUDED.tipo_cabello,
                id_especialista_preferido = EXCLUDED.id_especialista_preferido,
                id_sucursal_favorita = EXCLUDED.id_sucursal_favorita,
                alergias_condiciones = EXCLUDED.alergias_condiciones,
                notificaciones_correo = EXCLUDED.notificaciones_correo,
                recordatorios_sms = EXCLUDED.recordatorios_sms,
                promociones_especiales = EXCLUDED.promociones_especiales,
                boletin_mensual = EXCLUDED.boletin_mensual,
                idioma = EXCLUDED.idioma;
        ";

                await using (var cmd = new NpgsqlCommand(
                    sqlPreferencias, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", modelo.IdCliente);

                    cmd.Parameters.AddWithValue("@TipoCabello",
                        (object?)modelo.TipoCabello ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@IdEspecialistaPreferido",
                        (object?)modelo.IdEspecialistaPreferido ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@IdSucursalFavorita",
                        (object?)modelo.IdSucursalFavorita ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@AlergiasCondiciones",
                        (object?)modelo.AlergiasCondiciones ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@NotificacionesCorreo",
                        modelo.NotificacionesCorreo);

                    cmd.Parameters.AddWithValue(
                        "@RecordatoriosSms",
                        modelo.RecordatoriosSms);

                    cmd.Parameters.AddWithValue(
                        "@PromocionesEspeciales",
                        modelo.PromocionesEspeciales);

                    cmd.Parameters.AddWithValue(
                        "@BoletinMensual",
                        modelo.BoletinMensual);

                    cmd.Parameters.AddWithValue(
                        "@Idioma",
                        modelo.Idioma ?? "es");

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return RedirectToAction(nameof(PerfilCliente));
            }
            catch
            {
                await transaction.RollbackAsync();

                return StatusCode(
                    500,
                    "Ocurrió un error al guardar los cambios."
                );
            }
        }

        public async Task<IActionResult> Promociones()
        {
            int idCliente = 1;

            await using var conn =
                _conexion.GetConnection();

            await conn.OpenAsync();


            string sql = @"
        SELECT
            ""IdCliente"",
            ""NombreCompleto"",
            ""TipoCliente"",
            ""PuntosFidelidad"",
            ""FechaReferencia"",
            ""IdMembresiaActual"",
            ""MembresiaActual"",
            ""BeneficiosMembresiaActual"",
            ""DuracionMembresiaActual"",
            ""FechaInicioMembresia"",
            ""FechaFinMembresia"",
            ""EstadoMembresia"",
            ""TotalPromocionesActivas"",
            ""TotalPlanes"",
            ""PromocionesActivasJson"",
            ""OfertasTemporadaJson"",
            ""PlanesMembresiaJson""
        FROM public.vw_promociones_membresias_cliente
        WHERE ""IdCliente"" = @idCliente;
    ";


            await using var cmd =
                new NpgsqlCommand(
                    sql,
                    conn);


            cmd.Parameters.AddWithValue(
                "@idCliente",
                idCliente);


            await using var reader =
                await cmd.ExecuteReaderAsync();


            if (!await reader.ReadAsync())
            {
                return NotFound();
            }


            ClientePromosMembresias modelo =
                new ClientePromosMembresias();


            // =====================================
            // CLIENTE
            // =====================================

            modelo.IdCliente =
                Convert.ToInt32(
                    reader["IdCliente"]);


            modelo.NombreCompleto =
                reader["NombreCompleto"]?.ToString()
                ?? "";


            modelo.TipoCliente =
                reader["TipoCliente"]?.ToString();


            modelo.PuntosFidelidad =
                Convert.ToInt32(
                    reader["PuntosFidelidad"]);


            // =====================================
            // FECHA
            // =====================================

            modelo.FechaReferencia =
                (DateOnly)reader["FechaReferencia"];


            // =====================================
            // MEMBRESÍA
            // =====================================

            modelo.IdMembresiaActual =
                reader["IdMembresiaActual"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(
                        reader["IdMembresiaActual"]);


            modelo.MembresiaActual =
                reader["MembresiaActual"]?.ToString();


            modelo.BeneficiosMembresiaActual =
                reader["BeneficiosMembresiaActual"]?.ToString();


            modelo.DuracionMembresiaActual =
                reader["DuracionMembresiaActual"]?.ToString();


            modelo.FechaInicioMembresia =
                reader["FechaInicioMembresia"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(
                        reader["FechaInicioMembresia"]);


            modelo.FechaFinMembresia =
                reader["FechaFinMembresia"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(
                        reader["FechaFinMembresia"]);


            modelo.EstadoMembresia =
                reader["EstadoMembresia"]?.ToString();


            // =====================================
            // INDICADORES
            // =====================================

            modelo.TotalPromocionesActivas =
                Convert.ToInt32(
                    reader["TotalPromocionesActivas"]);


            modelo.TotalPlanes =
                Convert.ToInt32(
                    reader["TotalPlanes"]);


            // =====================================
            // JSON
            // =====================================

            modelo.PromocionesActivasJson =
                reader["PromocionesActivasJson"] == DBNull.Value
                    ? "[]"
                    : reader["PromocionesActivasJson"]?.ToString()
                      ?? "[]";


            modelo.OfertasTemporadaJson =
                reader["OfertasTemporadaJson"] == DBNull.Value
                    ? "[]"
                    : reader["OfertasTemporadaJson"]?.ToString()
                      ?? "[]";


            modelo.PlanesMembresiaJson =
                reader["PlanesMembresiaJson"] == DBNull.Value
                    ? "[]"
                    : reader["PlanesMembresiaJson"]?.ToString()
                      ?? "[]";


            // =====================================
            // CONVERTIR JSON A LISTAS
            // =====================================

            JsonSerializerOptions opciones =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };


            modelo.PromocionesActivas =
                JsonSerializer.Deserialize<
                    List<ClientePromosMembresias.PromocionCliente>
                >(
                    modelo.PromocionesActivasJson,
                    opciones)
                ?? new();


            modelo.OfertasTemporada =
                JsonSerializer.Deserialize<
                    List<ClientePromosMembresias.PromocionCliente>
                >(
                    modelo.OfertasTemporadaJson,
                    opciones)
                ?? new();


            modelo.PlanesMembresia =
                JsonSerializer.Deserialize<
                    List<ClientePromosMembresias.PlanMembresia>
                >(
                    modelo.PlanesMembresiaJson,
                    opciones)
                ?? new();

          

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContratarMembresia(int idMembresia)
        {
            int idCliente = 1;

            if (idMembresia <= 0)
            {
                return BadRequest();
            }

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            // Verificar que la membresía exista y esté activa
            string sqlMembresia = @"
        SELECT
            id_membresia,
            nombre,
            duracion
        FROM public.membresia
        WHERE id_membresia = @idMembresia
          AND activo = true
        LIMIT 1;
    ";

            int idMembresiaReal;
            string duracion;

            await using (var cmdMembresia =
                new NpgsqlCommand(sqlMembresia, conn))
            {
                cmdMembresia.Parameters.AddWithValue(
                    "@idMembresia",
                    idMembresia);

                await using var reader =
                    await cmdMembresia.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound();
                }

                idMembresiaReal =
                    Convert.ToInt32(reader["id_membresia"]);

                duracion =
                    reader["duracion"]?.ToString()
                    ?? "1 mes";
            }

            // Determinar duración
            int meses = 1;

            if (duracion.ToLower().Contains("3"))
            {
                meses = 3;
            }
            else if (duracion.ToLower().Contains("6"))
            {
                meses = 6;
            }
            else if (duracion.ToLower().Contains("12"))
            {
                meses = 12;
            }

            DateTime fechaInicio = DateTime.Now;
            DateTime fechaFin = fechaInicio.AddMonths(meses);

            await using var transaction =
                await conn.BeginTransactionAsync();

            try
            {
                // Desactivar membresías anteriores
                string sqlDesactivar = @"
            UPDATE public.identificador
            SET estado = 'Inactivo'
            WHERE id_cliente = @idCliente
              AND LOWER(estado) = 'activo';
        ";

                await using (var cmdDesactivar =
                    new NpgsqlCommand(
                        sqlDesactivar,
                        conn,
                        transaction))
                {
                    cmdDesactivar.Parameters.AddWithValue(
                        "@idCliente",
                        idCliente);

                    await cmdDesactivar.ExecuteNonQueryAsync();
                }

                // Registrar nueva membresía
                string sqlInsertar = @"
            INSERT INTO public.identificador
            (
                id_cliente,
                id_membresia,
                fecha_inicio,
                fecha_fin,
                estado
            )
            VALUES
            (
                @idCliente,
                @idMembresia,
                @fechaInicio,
                @fechaFin,
                'Activo'
            );
        ";

                await using (var cmdInsertar =
                    new NpgsqlCommand(
                        sqlInsertar,
                        conn,
                        transaction))
                {
                    cmdInsertar.Parameters.AddWithValue(
                        "@idCliente",
                        idCliente);

                    cmdInsertar.Parameters.AddWithValue(
                        "@idMembresia",
                        idMembresiaReal);

                    cmdInsertar.Parameters.AddWithValue(
                        "@fechaInicio",
                        fechaInicio);

                    cmdInsertar.Parameters.AddWithValue(
                        "@fechaFin",
                        fechaFin);

                    await cmdInsertar.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Promociones));
            }
            catch
            {
                await transaction.RollbackAsync();

                return RedirectToAction(nameof(Promociones));
            }
        }

        public async Task<IActionResult> ReservarCita(
       int? idServicio,
       int? idEspecialista,
       int? idSucursal,
       DateTime? fechaCita,
       string? especialista,
       string? servicio,
       string? sucursal,
       int? idPromocion)
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            var modelo = new ReservarCita
            {
                IdCliente = 1,
                IdServicio = idServicio,
                IdEspecialista = idEspecialista,
                IdSucursal = idSucursal,
                FechaCita = fechaCita
            };

            string sql = @"
        SELECT *
        FROM public.vw_reservar_cita
        ORDER BY
            ""Servicio"",
            ""Especialista"",
            ""Sucursal"";
    ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var opcion = new ReservarCita.ReservaOpcion
                {

                    IdServicioPadre = reader["IdServicioPadre"] == DBNull.Value
    ? null
    : Convert.ToInt32(reader["IdServicioPadre"]),
                    IdServicio = Convert.ToInt32(reader["IdServicio"]),

                    Servicio =
                        reader["Servicio"]?.ToString() ?? "",

                    CostoServicio =
                        reader["CostoServicio"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(reader["CostoServicio"]),

                    IdEspecialista =
                        Convert.ToInt32(reader["IdEspecialista"]),

                    Especialista =
                        reader["Especialista"]?.ToString() ?? "",

                    RolEspecialista =
                        reader["RolEspecialista"]?.ToString(),

                    AniosExperiencia =
                        reader["AniosExperiencia"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["AniosExperiencia"]),

                    HorarioEspecialista =
                        reader["HorarioEspecialista"]?.ToString(),

                    IdSucursal =
                        Convert.ToInt32(reader["IdSucursal"]),

                    Sucursal =
                        reader["Sucursal"]?.ToString() ?? "",

                    DireccionSucursal =
                        reader["DireccionSucursal"]?.ToString(),

                    HorarioApertura =
                        reader["HorarioApertura"] == DBNull.Value
                            ? null
                            : ((TimeOnly)reader["HorarioApertura"]).ToTimeSpan(),

                    HorarioCierre =
                        reader["HorarioCierre"] == DBNull.Value
                            ? null
                            : ((TimeOnly)reader["HorarioCierre"]).ToTimeSpan(),

                    IdEspecialidad =
                        reader["IdEspecialidad"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["IdEspecialidad"]),

                    Especialidad =
                        reader["Especialidad"]?.ToString(),

                    ServicioDisponible =
    reader["ServicioDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["ServicioDisponible"]),

                    EspecialistaDisponible =
    reader["EspecialistaDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["EspecialistaDisponible"]),

                    ServicioSucursalDisponible =
    reader["ServicioSucursalDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["ServicioSucursalDisponible"]),

                    SucursalAbierta =
    reader["SucursalAbierta"] != DBNull.Value
        && Convert.ToBoolean(reader["SucursalAbierta"])
                };

                modelo.OpcionesDisponibles.Add(opcion);
            }

            // IMPORTANTE:
            // El reader se cierra DESPUÉS de terminar el while.
            await reader.CloseAsync();

            // Obtener puntos del cliente
            string sqlPuntos = @"
        SELECT COALESCE(puntos_fidelidad, 0)
        FROM public.cliente
        WHERE id_cliente = @IdCliente;
    ";

            await using var cmdPuntos =
                new NpgsqlCommand(sqlPuntos, conn);

            cmdPuntos.Parameters.AddWithValue(
                "@IdCliente",
                modelo.IdCliente);

            var puntos =
                await cmdPuntos.ExecuteScalarAsync();

            modelo.PuntosFidelidad =
                puntos == null || puntos == DBNull.Value
                    ? 0
                    : Convert.ToInt32(puntos);

            // Compatibilidad con enlaces antiguos del Dashboard
            if (!idServicio.HasValue &&
                !string.IsNullOrWhiteSpace(servicio))
            {
                var opcion = modelo.OpcionesDisponibles
                    .FirstOrDefault(x =>
                        x.Servicio.Equals(
                            servicio,
                            StringComparison.OrdinalIgnoreCase));

                if (opcion != null)
                    modelo.IdServicio = opcion.IdServicio;
            }

            if (!idEspecialista.HasValue &&
                !string.IsNullOrWhiteSpace(especialista))
            {
                var opcion = modelo.OpcionesDisponibles
                    .FirstOrDefault(x =>
                        x.Especialista.Equals(
                            especialista,
                            StringComparison.OrdinalIgnoreCase));

                if (opcion != null)
                    modelo.IdEspecialista = opcion.IdEspecialista;
            }

            if (!idSucursal.HasValue &&
                !string.IsNullOrWhiteSpace(sucursal))
            {
                var opcion = modelo.OpcionesDisponibles
                    .FirstOrDefault(x =>
                        x.Sucursal.Equals(
                            sucursal,
                            StringComparison.OrdinalIgnoreCase));

                if (opcion != null)
                    modelo.IdSucursal = opcion.IdSucursal;
            }

            return View(modelo);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerHorasDisponibles(
            int idEspecialista,
            int idSucursal,
            DateTime fecha)
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            var horas = new List<string>();

            // Buscar horario del especialista
            string sqlHorario = @"
        SELECT
            ""HorarioEspecialista"",
            ""HorarioApertura"",
            ""HorarioCierre""
        FROM public.vw_reservar_cita
        WHERE ""IdEspecialista"" = @IdEspecialista
          AND ""IdSucursal"" = @IdSucursal
        LIMIT 1;
    ";

            await using var cmdHorario =
                new NpgsqlCommand(sqlHorario, conn);

            cmdHorario.Parameters.AddWithValue(
                "@IdEspecialista",
                idEspecialista);

            cmdHorario.Parameters.AddWithValue(
                "@IdSucursal",
                idSucursal);

            await using var reader =
                await cmdHorario.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return Json(horas);

            string horarioEspecialista =
                reader["HorarioEspecialista"]?.ToString() ?? "";

            TimeSpan? apertura =
                reader["HorarioApertura"] == DBNull.Value
                    ? null
                    : ((TimeOnly)reader["HorarioApertura"]).ToTimeSpan();

            TimeSpan? cierre =
                reader["HorarioCierre"] == DBNull.Value
                    ? null
                    : ((TimeOnly)reader["HorarioCierre"]).ToTimeSpan();

            await reader.CloseAsync();

            // El especialista debe trabajar ese día.
            if (!EspecialistaTrabajaEseDia(
                horarioEspecialista,
                fecha.DayOfWeek))
            {
                return Json(horas);
            }

            // Obtener horario del especialista.
            if (!ObtenerRangoHorario(
                horarioEspecialista,
                out TimeSpan inicioEspecialista,
                out TimeSpan finEspecialista))
            {
                return Json(horas);
            }

            // Ajustar al horario de la sucursal.
            TimeSpan inicio = inicioEspecialista;
            TimeSpan fin = finEspecialista;

            if (apertura.HasValue && inicio < apertura.Value)
                inicio = apertura.Value;

            if (cierre.HasValue && fin > cierre.Value)
                fin = cierre.Value;

            // Obtener citas ya ocupadas.
            string sqlCitas = @"
        SELECT c.hora_cita
        FROM public.cita c
        INNER JOIN public.especialistas_en_cita ec
            ON c.id_cita = ec.id_cita
        WHERE ec.id_especialista = @IdEspecialista
          AND c.fecha_cita = @Fecha
          AND LOWER(COALESCE(c.estado, '')) NOT IN
              ('cancelada', 'cancelado');
    ";

            var horasOcupadas = new HashSet<TimeSpan>();

            await using var cmdCitas =
                new NpgsqlCommand(sqlCitas, conn);

            cmdCitas.Parameters.AddWithValue(
                "@IdEspecialista",
                idEspecialista);

            cmdCitas.Parameters.AddWithValue(
                "@Fecha",
                fecha.Date);

            await using var readerCitas =
                await cmdCitas.ExecuteReaderAsync();

            while (await readerCitas.ReadAsync())
            {
                if (readerCitas["hora_cita"] != DBNull.Value)
                {
                    horasOcupadas.Add(
    ((TimeOnly)readerCitas["hora_cita"]).ToTimeSpan());
                }
            }

            // Generamos bloques de una hora.
            for (TimeSpan hora = inicio;
                 hora < fin;
                 hora = hora.Add(TimeSpan.FromHours(1)))
            {
                if (!horasOcupadas.Contains(hora))
                {
                    horas.Add(hora.ToString(@"hh\:mm"));
                }
            }

            return Json(horas);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarReserva(
    ReservarCita modelo,
    int? idPromocion)
        {
            modelo.IdCliente = 1;

            if (!modelo.IdServicio.HasValue ||
                !modelo.IdEspecialista.HasValue ||
                !modelo.IdSucursal.HasValue ||
                !modelo.FechaCita.HasValue ||
                !modelo.HoraCita.HasValue)
            {
                modelo.Mensaje =
                    "Debes completar todos los datos de la reserva.";

                return await RecargarReserva(modelo);
            }

            if (modelo.FechaCita.Value.Date < DateTime.Today)
            {
                modelo.Mensaje =
                    "No puedes reservar una fecha anterior a hoy.";

                return await RecargarReserva(modelo);
            }

            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            // Comprobar que la combinación exista realmente.
            string sqlValidacion = @"
        SELECT
            ""Servicio"",
            ""Especialista"",
            ""Sucursal"",
            ""HorarioEspecialista"",
            ""ServicioDisponible"",
            ""EspecialistaDisponible"",
            ""ServicioSucursalDisponible"",
            ""SucursalAbierta""
        FROM public.vw_reservar_cita
        WHERE ""IdServicio"" = @IdServicio
          AND ""IdEspecialista"" = @IdEspecialista
          AND ""IdSucursal"" = @IdSucursal
        LIMIT 1;
    ";

            await using var cmdValidacion =
                new NpgsqlCommand(sqlValidacion, conn);

            cmdValidacion.Parameters.AddWithValue(
                "@IdServicio",
                modelo.IdServicio.Value);

            cmdValidacion.Parameters.AddWithValue(
                "@IdEspecialista",
                modelo.IdEspecialista.Value);

            cmdValidacion.Parameters.AddWithValue(
                "@IdSucursal",
                modelo.IdSucursal.Value);

            await using var reader =
                await cmdValidacion.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                modelo.Mensaje =
                    "La combinación seleccionada no está disponible.";

                return await RecargarReserva(modelo);
            }

            bool servicioDisponible =
                Convert.ToBoolean(reader["ServicioDisponible"]);

            bool especialistaDisponible =
                Convert.ToBoolean(reader["EspecialistaDisponible"]);

            bool servicioSucursalDisponible =
                Convert.ToBoolean(reader["ServicioSucursalDisponible"]);

            bool sucursalAbierta =
                Convert.ToBoolean(reader["SucursalAbierta"]);

            string horarioEspecialista =
                reader["HorarioEspecialista"]?.ToString() ?? "";

            await reader.CloseAsync();

            if (!servicioDisponible ||
                !especialistaDisponible ||
                !servicioSucursalDisponible ||
                !sucursalAbierta)
            {
                modelo.Mensaje =
                    "El servicio, especialista o sucursal no está disponible.";

                return await RecargarReserva(modelo);
            }

            if (!EspecialistaTrabajaEseDia(
                horarioEspecialista,
                modelo.FechaCita.Value.DayOfWeek))
            {
                modelo.Mensaje =
                    "El especialista no trabaja ese día.";

                return await RecargarReserva(modelo);
            }

            if (!ObtenerRangoHorario(
                horarioEspecialista,
                out TimeSpan inicio,
                out TimeSpan fin))
            {
                modelo.Mensaje =
                    "No se pudo determinar el horario del especialista.";

                return await RecargarReserva(modelo);
            }

            if (modelo.HoraCita.Value < inicio ||
                modelo.HoraCita.Value >= fin)
            {
                modelo.Mensaje =
                    "La hora seleccionada está fuera del horario del especialista.";

                return await RecargarReserva(modelo);
            }

            // Comprobar que el horario no esté ocupado.
            string sqlOcupada = @"
        SELECT COUNT(*)
        FROM public.cita c
        INNER JOIN public.especialistas_en_cita ec
            ON c.id_cita = ec.id_cita
        WHERE ec.id_especialista = @IdEspecialista
          AND c.fecha_cita = @Fecha
          AND c.hora_cita = @Hora
          AND LOWER(COALESCE(c.estado, '')) NOT IN
              ('cancelada', 'cancelado');
    ";

            await using var cmdOcupada =
                new NpgsqlCommand(sqlOcupada, conn);

            cmdOcupada.Parameters.AddWithValue(
                "@IdEspecialista",
                modelo.IdEspecialista.Value);

            cmdOcupada.Parameters.AddWithValue(
                "@Fecha",
                modelo.FechaCita.Value.Date);

            cmdOcupada.Parameters.AddWithValue(
                "@Hora",
                modelo.HoraCita.Value);

            int cantidadOcupada =
                Convert.ToInt32(await cmdOcupada.ExecuteScalarAsync());

            if (cantidadOcupada > 0)
            {
                modelo.Mensaje =
                    "Esa hora ya fue reservada. Selecciona otra.";

                return await RecargarReserva(modelo);
            }

            await using var transaction =
                await conn.BeginTransactionAsync();

            try
            {
                // Crear la cita.
                string sqlCita = @"
            INSERT INTO public.cita
            (
                id_cliente,
                id_servicio,
                id_empleado,
                nombre,
                fecha_cita,
                hora_cita,
                estado,
                id_sucursal
            )
            VALUES
            (
                @IdCliente,
                @IdServicio,
                NULL,
                NULL,
                @Fecha,
                @Hora,
                'Pendiente',
                @IdSucursal
            )
            RETURNING id_cita;
        ";

                int idCita;

                await using (var cmdCita =
                    new NpgsqlCommand(
                        sqlCita,
                        conn,
                        transaction))
                {
                    cmdCita.Parameters.AddWithValue(
                        "@IdCliente",
                        modelo.IdCliente);

                    cmdCita.Parameters.AddWithValue(
                        "@IdServicio",
                        modelo.IdServicio.Value);

                    cmdCita.Parameters.AddWithValue(
                        "@Fecha",
                        modelo.FechaCita.Value.Date);

                    cmdCita.Parameters.AddWithValue(
                        "@Hora",
                        modelo.HoraCita.Value);

                    cmdCita.Parameters.AddWithValue(
                        "@IdSucursal",
                        modelo.IdSucursal.Value);

                    idCita = Convert.ToInt32(
                        await cmdCita.ExecuteScalarAsync());
                }

                // Relacionar especialista con la cita.
                string sqlEspecialista = @"
            INSERT INTO public.especialistas_en_cita
            (
                id_cita,
                id_especialista
            )
            VALUES
            (
                @IdCita,
                @IdEspecialista
            );
        ";

                await using (var cmdEspecialista =
                    new NpgsqlCommand(
                        sqlEspecialista,
                        conn,
                        transaction))
                {
                    cmdEspecialista.Parameters.AddWithValue(
                        "@IdCita",
                        idCita);

                    cmdEspecialista.Parameters.AddWithValue(
                        "@IdEspecialista",
                        modelo.IdEspecialista.Value);

                    await cmdEspecialista.ExecuteNonQueryAsync();
                }

                // Registrar también el servicio en la tabla intermedia.
                string sqlServicio = @"
            INSERT INTO public.servicios_en_cita
            (
                id_cita,
                id_servicios
            )
            VALUES
            (
                @IdCita,
                @IdServicio
            );
        ";

                await using (var cmdServicio =
                    new NpgsqlCommand(
                        sqlServicio,
                        conn,
                        transaction))
                {
                    cmdServicio.Parameters.AddWithValue(
                        "@IdCita",
                        idCita);

                    cmdServicio.Parameters.AddWithValue(
                        "@IdServicio",
                        modelo.IdServicio.Value);

                    await cmdServicio.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                modelo.ReservaExitosa = true;
                modelo.Mensaje =
                    "Cita reservada correctamente.";

                return RedirectToAction(
     nameof(Pago),
     new
     {
         idCita = idCita,
         idPromocion = idPromocion
     });
            }
            catch
            {
                await transaction.RollbackAsync();

                modelo.Mensaje =
                    "Ocurrió un error al crear la reserva.";

                return await RecargarReserva(modelo);
            }
        }


        private async Task<IActionResult> RecargarReserva(
ReservarCita modelo)
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT *
        FROM public.vw_reservar_cita
        ORDER BY
            ""Servicio"",
            ""Especialista"",
            ""Sucursal"";
    ";

            await using var cmd =
                new NpgsqlCommand(sql, conn);

            await using var reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                modelo.OpcionesDisponibles.Add(
                    new ReservarCita.ReservaOpcion
                    {
                        IdServicio =
                            Convert.ToInt32(
                                reader["IdServicio"]),

                        IdServicioPadre =
                            reader["IdServicioPadre"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(
                                    reader["IdServicioPadre"]),

                        Servicio =
                            reader["Servicio"]?.ToString() ?? "",

                        CostoServicio =
                            reader["CostoServicio"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(
                                    reader["CostoServicio"]),

                        IdEspecialista =
                            Convert.ToInt32(
                                reader["IdEspecialista"]),

                        Especialista =
                            reader["Especialista"]?.ToString() ?? "",

                        RolEspecialista =
                            reader["RolEspecialista"]?.ToString(),

                        AniosExperiencia =
                            reader["AniosExperiencia"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(
                                    reader["AniosExperiencia"]),

                        HorarioEspecialista =
                            reader["HorarioEspecialista"]?.ToString(),

                        IdSucursal =
                            Convert.ToInt32(
                                reader["IdSucursal"]),

                        Sucursal =
                            reader["Sucursal"]?.ToString() ?? "",

                        DireccionSucursal =
                            reader["DireccionSucursal"]?.ToString(),

                        HorarioApertura =
                            reader["HorarioApertura"] == DBNull.Value
                                ? null
                                : ((TimeOnly)
                                    reader["HorarioApertura"])
                                    .ToTimeSpan(),

                        HorarioCierre =
                            reader["HorarioCierre"] == DBNull.Value
                                ? null
                                : ((TimeOnly)
                                    reader["HorarioCierre"])
                                    .ToTimeSpan(),

                        IdEspecialidad =
                            reader["IdEspecialidad"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(
                                    reader["IdEspecialidad"]),

                        Especialidad =
                            reader["Especialidad"]?.ToString(),

                        ServicioDisponible =
    reader["ServicioDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["ServicioDisponible"]),

                        EspecialistaDisponible =
    reader["EspecialistaDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["EspecialistaDisponible"]),

                        ServicioSucursalDisponible =
    reader["ServicioSucursalDisponible"] != DBNull.Value
        && Convert.ToBoolean(reader["ServicioSucursalDisponible"]),

                        SucursalAbierta =
    reader["SucursalAbierta"] != DBNull.Value
        && Convert.ToBoolean(reader["SucursalAbierta"])
                    });
            }

            return View("ReservarCita", modelo);
        }

        private bool EspecialistaTrabajaEseDia(
      string horario,
      DayOfWeek dia)
        {
            if (string.IsNullOrWhiteSpace(horario))
                return false;

            string texto = horario
                .ToLowerInvariant()
                .Normalize(
                    System.Text.NormalizationForm.FormD)
                .Replace(
                    "á", "a")
                .Replace(
                    "é", "e")
                .Replace(
                    "í", "i")
                .Replace(
                    "ó", "o")
                .Replace(
                    "ú", "u");

            var nombresDias = new Dictionary<DayOfWeek, string[]>
    {
        {
            DayOfWeek.Monday,
            new[] { "lunes", "lun" }
        },
        {
            DayOfWeek.Tuesday,
            new[] { "martes", "mar" }
        },
        {
            DayOfWeek.Wednesday,
            new[] { "miercoles", "mie" }
        },
        {
            DayOfWeek.Thursday,
            new[] { "jueves", "jue" }
        },
        {
            DayOfWeek.Friday,
            new[] { "viernes", "vie" }
        },
        {
            DayOfWeek.Saturday,
            new[] { "sabado", "sab" }
        },
        {
            DayOfWeek.Sunday,
            new[] { "domingo", "dom" }
        }
    };

            // Caso: "Lunes a Viernes"
            if (texto.Contains("lunes") &&
                texto.Contains("viernes"))
            {
                return dia >= DayOfWeek.Monday &&
                       dia <= DayOfWeek.Friday;
            }

            // Caso: "Lunes a Sábado"
            if (texto.Contains("lunes") &&
                texto.Contains("sabado"))
            {
                return dia >= DayOfWeek.Monday &&
                       dia <= DayOfWeek.Saturday;
            }

            // Caso: "Lunes a Domingo"
            if (texto.Contains("lunes") &&
                texto.Contains("domingo"))
            {
                return true;
            }

            // Revisar días individuales y abreviaturas
            if (nombresDias.TryGetValue(dia, out var nombres))
            {
                foreach (var nombre in nombres)
                {
                    if (texto.Contains(nombre))
                        return true;
                }
            }

            return false;
        }

        private bool ObtenerRangoHorario(
            string horario,
            out TimeSpan inicio,
            out TimeSpan fin)
        {
            inicio = TimeSpan.Zero;
            fin = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(horario))
                return false;

            string texto =
                horario.ToLowerInvariant();

            int separador =
                texto.LastIndexOf('-');

            if (separador < 0)
                return false;

            string parteInicio =
                texto.Substring(0, separador);

            string parteFin =
                texto.Substring(separador + 1);

            // Buscar la última hora encontrada antes del "-"
            string[] posiblesInicio =
                parteInicio.Split(
                    new[] { ',', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

            string horaInicioTexto =
                posiblesInicio.Length > 0
                    ? posiblesInicio[^1]
                    : "";

            string horaFinTexto =
                parteFin.Trim();

            // Intentar encontrar una hora con AM/PM.
            var matchInicio =
                System.Text.RegularExpressions.Regex.Match(
                    parteInicio,
                    @"\d{1,2}(?::\d{2})?\s*(?:am|pm)?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var matchFin =
                System.Text.RegularExpressions.Regex.Match(
                    parteFin,
                    @"\d{1,2}(?::\d{2})?\s*(?:am|pm)?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!matchInicio.Success ||
                !matchFin.Success)
            {
                return false;
            }

            string inicioTexto =
                matchInicio.Value.Trim();

            string finTexto =
                matchFin.Value.Trim();

            if (!TimeSpan.TryParse(
                ConvertirHora(inicioTexto),
                out inicio))
            {
                return false;
            }

            if (!TimeSpan.TryParse(
                ConvertirHora(finTexto),
                out fin))
            {
                return false;
            }

            return true;
        }


        private string ConvertirHora(string hora)
        {
            hora = hora.Trim().ToUpperInvariant();

            bool pm = hora.EndsWith("PM");
            bool am = hora.EndsWith("AM");

            hora = hora
                .Replace("AM", "")
                .Replace("PM", "")
                .Trim();

            if (!hora.Contains(":"))
                hora += ":00";

            if (!TimeSpan.TryParse(
                hora,
                out TimeSpan resultado))
            {
                return hora;
            }

            if (pm && resultado.Hours < 12)
            {
                resultado = resultado.Add(
                    TimeSpan.FromHours(12));
            }

            if (am && resultado.Hours == 12)
            {
                resultado = resultado.Subtract(
                    TimeSpan.FromHours(12));
            }

            return resultado.ToString();
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(
    int idCita,
    int? idPromocion,
    string metodoPago,
    bool AceptaTerminos,
    bool AutorizaCobro,
    bool usarPuntos = false,
    string? numeroTarjeta = null,
    string? nombreTitular = null,
    string? vencimiento = null,
    string? cvv = null)
        {
            if (idCita <= 0)
            {
                return BadRequest();
            }

            // =====================================
            // VALIDACIÓN DEL FORMULARIO DE PAGO
            // =====================================

            if (!AceptaTerminos)
            {
                TempData["ErrorPago"] =
                    "Debes aceptar los términos y condiciones.";

                return RedirectToAction(
                    nameof(Pago),
                    new
                    {
                        idCita = idCita,
                        idPromocion = idPromocion
                    });
            }


            if (!AutorizaCobro)
            {
                TempData["ErrorPago"] =
                    "Debes autorizar el cobro.";

                return RedirectToAction(
                    nameof(Pago),
                    new
                    {
                        idCita = idCita,
                        idPromocion = idPromocion
                    });
            }


            string[] metodosPermitidos =
            {
    "Tarjeta",
    "Pago Móvil",
    "Transferencia Bancaria",
    "Pagar en Sucursal"
};


            if (string.IsNullOrWhiteSpace(metodoPago) ||
                !metodosPermitidos.Contains(metodoPago))
            {
                TempData["ErrorPago"] =
                    "Selecciona un método de pago válido.";

                return RedirectToAction(
                    nameof(Pago),
                    new
                    {
                        idCita = idCita,
                        idPromocion = idPromocion
                    });
            }


            // =====================================
            // VALIDAR DATOS DE TARJETA
            // SOLO SI SE USA TARJETA
            // =====================================

            if (metodoPago == "Tarjeta")
            {
                string numeroLimpio =
                    (numeroTarjeta ?? "")
                        .Replace(" ", "")
                        .Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        numeroLimpio,
                        @"^\d{13,19}$"))
                {
                    TempData["ErrorPago"] =
                        "El número de tarjeta no es válido.";

                    return RedirectToAction(
                        nameof(Pago),
                        new
                        {
                            idCita = idCita,
                            idPromocion = idPromocion
                        });
                }


                if (string.IsNullOrWhiteSpace(nombreTitular) ||
                    nombreTitular.Trim().Length < 3)
                {
                    TempData["ErrorPago"] =
                        "Ingresa el nombre del titular.";

                    return RedirectToAction(
                        nameof(Pago),
                        new
                        {
                            idCita = idCita,
                            idPromocion = idPromocion
                        });
                }


                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        vencimiento ?? "",
                        @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    TempData["ErrorPago"] =
                        "La fecha de vencimiento debe tener el formato MM/AA.";

                    return RedirectToAction(
                        nameof(Pago),
                        new
                        {
                            idCita = idCita,
                            idPromocion = idPromocion
                        });
                }


                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        cvv ?? "",
                        @"^\d{3,4}$"))
                {
                    TempData["ErrorPago"] =
                        "El CVV debe tener 3 o 4 dígitos.";

                    return RedirectToAction(
                        nameof(Pago),
                        new
                        {
                            idCita = idCita,
                            idPromocion = idPromocion
                        });
                }
            }

            if (!AceptaTerminos || !AutorizaCobro)
            {
                return RedirectToAction(
                    nameof(Pago),
                    new { idCita = idCita });
            }

            await using var conn =
                _conexion.GetConnection();

            await conn.OpenAsync();


            // =====================================
            // DATOS DE LA CITA
            // =====================================

            string sqlDatos = @"
        SELECT
            ""IdCliente"",
            ""Subtotal"",
            ""TotalAPagar""
        FROM public.vw_proceso_pago_cita
        WHERE ""IdCita"" = @idCita;
    ";

            await using var cmdDatos =
                new NpgsqlCommand(
                    sqlDatos,
                    conn);

            cmdDatos.Parameters.AddWithValue(
                "@idCita",
                idCita);

            await using var reader =
                await cmdDatos.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound();
            }

            int idCliente =
                Convert.ToInt32(
                    reader["IdCliente"]);

            decimal subtotal =
                Convert.ToDecimal(
                    reader["Subtotal"]);

            decimal total =
                Convert.ToDecimal(
                    reader["TotalAPagar"]);

            await reader.CloseAsync();


            // =====================================
            // TRANSACCIÓN
            // =====================================

            await using var transaction =
                await conn.BeginTransactionAsync();

            try
            {
                // =====================================
                // PUNTOS ACTUALES
                // =====================================

                string sqlPuntos = @"
            SELECT puntos_fidelidad
            FROM public.cliente
            WHERE id_cliente = @idCliente
            FOR UPDATE;
        ";

                int puntosActuales;

                await using (var cmdPuntos =
                    new NpgsqlCommand(
                        sqlPuntos,
                        conn,
                        transaction))
                {
                    cmdPuntos.Parameters.AddWithValue(
                        "@idCliente",
                        idCliente);

                    puntosActuales =
                        Convert.ToInt32(
                            await cmdPuntos.ExecuteScalarAsync());
                }


                // =====================================
                // CALCULAR PUNTOS USADOS
                // =====================================

                int puntosUsados = 0;

                if (usarPuntos)
                {
                    int maximoPorPrecio =
                        (int)Math.Floor(
                            total * 10m);

                    puntosUsados =
                        Math.Min(
                            puntosActuales,
                            maximoPorPrecio);
                }


                // =====================================
                // DESCUENTO POR PUNTOS
                // =====================================

                decimal descuentoPuntos =
                    puntosUsados / 10m;

                decimal totalFinal =
                    total - descuentoPuntos;


                if (totalFinal < 0)
                {
                    totalFinal = 0;
                }


                // =====================================
                // PUNTOS QUE GANA
                // =====================================

                int puntosGanados =
    (int)Math.Floor(
        totalFinal / 20m);

                // =====================================
                // REGISTRAR PAGO
                // =====================================

                string sqlPago = @"
            INSERT INTO public.pago
            (
                id_cita,
                id_cliente,
                subtotal,
                total,
                monto,
                moneda,
                metodo_pago,
                estado_pago,
                fecha_pago
            )
            VALUES
            (
                @idCita,
                @idCliente,
                @subtotal,
                @total,
                @total,
                'USD',
                @metodoPago,
                'Pagado',
                CURRENT_TIMESTAMP
            );
        ";

                await using var cmdPago =
                    new NpgsqlCommand(
                        sqlPago,
                        conn,
                        transaction);

                cmdPago.Parameters.AddWithValue(
                    "@idCita",
                    idCita);

                cmdPago.Parameters.AddWithValue(
                    "@idCliente",
                    idCliente);

                cmdPago.Parameters.AddWithValue(
                    "@subtotal",
                    subtotal);

                cmdPago.Parameters.AddWithValue(
                    "@total",
                    totalFinal);

                cmdPago.Parameters.AddWithValue(
                    "@metodoPago",
                    metodoPago);

                await cmdPago.ExecuteNonQueryAsync();


                // =====================================
                // ACTUALIZAR PUNTOS DEL CLIENTE
                // =====================================

                string sqlActualizarPuntos = @"
            UPDATE public.cliente
            SET puntos_fidelidad =
                puntos_fidelidad
                - @puntosUsados
                + @puntosGanados
            WHERE id_cliente = @idCliente;
        ";

                await using var cmdActualizarPuntos =
                    new NpgsqlCommand(
                        sqlActualizarPuntos,
                        conn,
                        transaction);

                cmdActualizarPuntos.Parameters.AddWithValue(
                    "@puntosUsados",
                    puntosUsados);

                cmdActualizarPuntos.Parameters.AddWithValue(
                    "@puntosGanados",
                    puntosGanados);

                cmdActualizarPuntos.Parameters.AddWithValue(
                    "@idCliente",
                    idCliente);

                await cmdActualizarPuntos.ExecuteNonQueryAsync();


                // =====================================
                // CONFIRMAR CITA
                // =====================================

                string sqlCita = @"
            UPDATE public.cita
            SET estado = 'Confirmada'
            WHERE id_cita = @idCita;
        ";

                await using var cmdCita =
                    new NpgsqlCommand(
                        sqlCita,
                        conn,
                        transaction);

                cmdCita.Parameters.AddWithValue(
                    "@idCita",
                    idCita);

                await cmdCita.ExecuteNonQueryAsync();



                await transaction.CommitAsync();

                return RedirectToAction(
                    nameof(MisReservas));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return Content(
                    "Error al procesar el pago: "
                    + ex.Message);
            }
        }
    }
}