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

        public async Task<IActionResult> PerfilCliente()
        {
            await using var conn = _conexion.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT *
        FROM public.vw_perfil_cliente
        WHERE ""IdCliente"" = @IdCliente
        LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, conn);

            // Temporalmente usamos el cliente 1
            cmd.Parameters.AddWithValue("@IdCliente", 1);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("No se encontraron datos del perfil.");
            }

            var modelo = new PerfilCliente
            {
                IdCliente = Convert.ToInt32(reader["IdCliente"]),
                Nombre = reader["Nombre"]?.ToString() ?? "",
                Apellido = reader["Apellido"]?.ToString() ?? "",
                NombreCompleto = reader["NombreCompleto"]?.ToString() ?? "",
                Correo = reader["Correo"]?.ToString(),
                Telefono = reader["Telefono"]?.ToString(),

                FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value
    ? null
    : ((DateOnly)reader["FechaNacimiento"]).ToDateTime(TimeOnly.MinValue),

                Direccion = reader["Direccion"]?.ToString(),
                FotoPerfilUrl = reader["FotoPerfilUrl"]?.ToString(),
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
    : ((DateOnly)reader["InicioMembresia"]).ToDateTime(TimeOnly.MinValue),

                FinMembresia = reader["FinMembresia"] == DBNull.Value
    ? null
    : ((DateOnly)reader["FinMembresia"]).ToDateTime(TimeOnly.MinValue),

                MiembroDesde = reader["MiembroDesde"] == DBNull.Value
    ? null
    : ((DateOnly)reader["MiembroDesde"]).ToDateTime(TimeOnly.MinValue),

                TipoCabello = reader["TipoCabello"]?.ToString(),

                IdEspecialistaPreferido = reader["IdEspecialistaPreferido"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["IdEspecialistaPreferido"]),

                EspecialistaPreferido = reader["EspecialistaPreferido"]?.ToString(),

                IdSucursalFavorita = reader["IdSucursalFavorita"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["IdSucursalFavorita"]),

                SucursalFavorita = reader["SucursalFavorita"]?.ToString(),

                AlergiasCondiciones = reader["AlergiasCondiciones"]?.ToString(),

                NotificacionesCorreo = reader["NotificacionesCorreo"] != DBNull.Value
                    && Convert.ToBoolean(reader["NotificacionesCorreo"]),

                RecordatoriosSms = reader["RecordatoriosSms"] != DBNull.Value
                    && Convert.ToBoolean(reader["RecordatoriosSms"]),

                PromocionesEspeciales = reader["PromocionesEspeciales"] != DBNull.Value
                    && Convert.ToBoolean(reader["PromocionesEspeciales"]),

                BoletinMensual = reader["BoletinMensual"] != DBNull.Value
                    && Convert.ToBoolean(reader["BoletinMensual"]),

                Idioma = reader["Idioma"]?.ToString() ?? "es"
            };

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

        public IActionResult Promociones()
        {
            return View();
        }

        public async Task<IActionResult> ReservarCita(
    int? idServicio,
    int? idEspecialista,
    int? idSucursal,
    DateTime? fechaCita,
    string? especialista,
    string? servicio,
    string? sucursal)
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
                    IdServicio = Convert.ToInt32(reader["IdServicio"]),

                    IdServicioPadre =
                        reader["IdServicioPadre"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["IdServicioPadre"]),

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
                        reader["Especialidad"]?.ToString()
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
            ReservarCita modelo)
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
                    nameof(MisReservas));
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
                            reader["Especialidad"]?.ToString()
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

            string texto =
                horario.ToLowerInvariant();

            var dias = new Dictionary<string, DayOfWeek>
    {
        { "domingo", DayOfWeek.Sunday },
        { "lunes", DayOfWeek.Monday },
        { "martes", DayOfWeek.Tuesday },
        { "miércoles", DayOfWeek.Wednesday },
        { "miercoles", DayOfWeek.Wednesday },
        { "jueves", DayOfWeek.Thursday },
        { "viernes", DayOfWeek.Friday },
        { "sábado", DayOfWeek.Saturday },
        { "sabado", DayOfWeek.Saturday }
    };

            foreach (var item in dias)
            {
                if (!texto.Contains(item.Key))
                    continue;

                if (item.Value == dia)
                    return true;
            }

            // Manejar horarios del tipo:
            // "Lunes a Viernes"
            if (texto.Contains("lunes") &&
                texto.Contains("viernes"))
            {
                return dia >= DayOfWeek.Monday &&
                       dia <= DayOfWeek.Friday;
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
    }
}