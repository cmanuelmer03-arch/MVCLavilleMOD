using System.Text.Json;
using Lavillemvc.Models;
using Npgsql;
using NpgsqlTypes;

namespace Lavillemvc.DAL
{
    public class GerenteEmpleadosDAL
    {
        private readonly Conexion _conexion;

        public GerenteEmpleadosDAL(IConfiguration configuration)
        {
            _conexion = new Conexion(configuration);
        }

        // Tipos de datos de los formularios, sin lógica de negocio.
        public class CrearEntrada
        {
            public string Nombre { get; set; } = "";
            public string Apellido { get; set; } = "";
            public string Cargo { get; set; } = "";
            public int IdEspecialidad { get; set; }
            public int? AniosExperiencia { get; set; }
            public bool Disponibilidad { get; set; }
        }

        public class EditarEntrada
        {
            public int IdEspecialista { get; set; }
            public string Cargo { get; set; } = "";
            public int IdEspecialidad { get; set; }
            public int? AniosExperiencia { get; set; }
            public bool Disponibilidad { get; set; }
        }

        public class HorarioEntrada
        {
            public int IdEspecialista { get; set; }
            public int DiaSemana { get; set; }
            public bool Habilitado { get; set; }
            public TimeSpan? HoraInicio { get; set; }
            public TimeSpan? HoraFin { get; set; }
            public TimeSpan? InicioAlmuerzo { get; set; }
            public TimeSpan? FinAlmuerzo { get; set; }
        }

        public class EspecialidadOpcion
        {
            public int IdEspecialidad { get; set; }
            public string Nombre { get; set; } = "";
        }

        private static string LeerTexto(NpgsqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i)) ?? "";
        }

        private static int LeerInt(NpgsqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i));
        }

        private static int? LeerIntN(NpgsqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? null : Convert.ToInt32(r.GetValue(i));
        }

        private static decimal? LeerDecimalN(NpgsqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? null : Convert.ToDecimal(r.GetValue(i));
        }

        private static bool? LeerBoolN(NpgsqlDataReader r, string columna)
        {
            int i = r.GetOrdinal(columna);
            return r.IsDBNull(i) ? null : r.GetBoolean(i);
        }

        public async Task<List<GerenteGestionEmpleados>> ObtenerPorSucursalAsync(int idSucursal)
        {
            var resultado = new List<GerenteGestionEmpleados>();
            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();

            // Solo especialistas vinculados a ESTA sucursal.
            // Los IdSucursal NULL no se muestran como miembros de Las Mercedes.
            const string sql = @"
                SELECT *
                FROM public.vw_gerente_gestion_empleados
                WHERE ""IdSucursal"" = @idSucursal
                ORDER BY ""NombreCompleto""";

            await using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("idSucursal", idSucursal);
            await using var r = await comando.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                var e = new GerenteGestionEmpleados
                {
                    IdEspecialista = LeerInt(r, "IdEspecialista"),
                    IdEmpleado = LeerIntN(r, "IdEmpleado"),
                    IdSucursal = LeerIntN(r, "IdSucursal"),
                    NombreSucursal = LeerTexto(r, "NombreSucursal"),
                    NombreCompleto = LeerTexto(r, "NombreCompleto"),
                    Cargo = LeerTexto(r, "Cargo"),
                    IdEspecialidad = LeerInt(r, "IdEspecialidad"),
                    Especialidad = LeerTexto(r, "Especialidad"),
                    AniosExperiencia = LeerIntN(r, "AniosExperiencia"),
                    CategoriaFiltro = LeerTexto(r, "CategoriaFiltro"),
                    FotoUrl = LeerTexto(r, "FotoUrl"),
                    Disponibilidad = LeerBoolN(r, "Disponibilidad"),
                    EstadoDisponibilidad = LeerTexto(r, "EstadoDisponibilidad"),
                    HorarioGeneral = LeerTexto(r, "HorarioGeneral"),
                    DiasConfigurados = LeerIntN(r, "DiasConfigurados"),
                    DiasActivos = LeerIntN(r, "DiasActivos"),
                    HorasSemanales = LeerDecimalN(r, "HorasSemanales"),
                    TipoJornada = LeerTexto(r, "TipoJornada"),
                    HorarioSemanalJson = LeerTexto(r, "HorarioSemanalJson"),
                    TotalEspecialistas = LeerInt(r, "TotalEspecialistas"),
                    TotalTiempoCompleto = LeerInt(r, "TotalTiempoCompleto"),
                    TotalMedioTiempo = LeerInt(r, "TotalMedioTiempo"),
                    TotalConHorarioConfigurado = LeerInt(r, "TotalConHorarioConfigurado")
                };

                // La vista SQL devuelve un JSON de los siete días.
                e.Horarios = JsonSerializer.Deserialize<
                    List<GerenteGestionEmpleados.HorarioSemanalEmpleado>>(
                    e.HorarioSemanalJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<GerenteGestionEmpleados.HorarioSemanalEmpleado>();

                resultado.Add(e);
            }

            return resultado;
        }

        public async Task<List<EspecialidadOpcion>> ObtenerEspecialidadesAsync()
        {
            var resultado = new List<EspecialidadOpcion>();
            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();
            const string sql = @"
                SELECT id_especialidad, nombre_especialidad
                FROM public.especialidad
                ORDER BY nombre_especialidad";
            await using var comando = new NpgsqlCommand(sql, conexion);
            await using var r = await comando.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                resultado.Add(new EspecialidadOpcion
                {
                    IdEspecialidad = r.GetInt32(0),
                    Nombre = r.GetString(1)
                });
            }
            return resultado;
        }

        public async Task CrearAsync(int idSucursal, CrearEntrada e)
        {
            string nombre = e.Nombre.Trim();
            string apellido = e.Apellido.Trim();
            string nombreCompleto = $"{nombre} {apellido}";
            if (nombreCompleto.Length > 100 || e.Cargo.Length > 100)
                throw new ArgumentException("El nombre completo o cargo supera 100 caracteres.");

            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();
            await using var transaccion = await conexion.BeginTransactionAsync();

            // El admin ya usa este mismo candado al crear especialistas.
            await using (var bloqueo = new NpgsqlCommand(
                "SELECT pg_advisory_xact_lock(42174, 1)", conexion, transaccion))
                await bloqueo.ExecuteNonQueryAsync();

            // Impide duplicar un nombre: la vista actual une por nombre,
            // no mediante una clave foránea explícita.
            const string verificarNombre = @"
                SELECT EXISTS(
                    SELECT 1 FROM public.especialista
                    WHERE LOWER(TRIM(nombre)) = LOWER(TRIM(@nombreCompleto)))";
            await using (var verificar = new NpgsqlCommand(
                verificarNombre, conexion, transaccion))
            {
                verificar.Parameters.AddWithValue("nombreCompleto", nombreCompleto);
                if ((bool)(await verificar.ExecuteScalarAsync() ?? false))
                    throw new InvalidOperationException(
                        "Ya existe un especialista con ese nombre.");
            }

            const string crearEspecialista = @"
                INSERT INTO public.especialista
                    (id_especialista, id_especialidad, nombre,
                     rol_especialista, anios_experiencia, disponibilidad)
                VALUES (
                    (SELECT COALESCE(MAX(id_especialista),0)+1 FROM public.especialista),
                    @especialidad, @nombreCompleto, @cargo, @experiencia, @disponibilidad)";
            await using (var cmd = new NpgsqlCommand(crearEspecialista, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("especialidad", e.IdEspecialidad);
                cmd.Parameters.AddWithValue("nombreCompleto", nombreCompleto);
                cmd.Parameters.AddWithValue("cargo", e.Cargo.Trim());
                cmd.Parameters.Add("experiencia", NpgsqlDbType.Integer).Value =
                    (object?)e.AniosExperiencia ?? DBNull.Value;
                cmd.Parameters.AddWithValue("disponibilidad", e.Disponibilidad);
                await cmd.ExecuteNonQueryAsync();
            }

            // La vista existente relaciona especialista y empleado por nombre
            // y exige que empleado.cargo contenga la palabra 'especialista'.
            const string crearEmpleado = @"
                INSERT INTO public.empleado
                    (id_empleado, id_sucursal, nombre, apellido, cargo)
                VALUES (
                    (SELECT COALESCE(MAX(id_empleado),0)+1 FROM public.empleado),
                    @sucursal, @nombre, @apellido, 'Especialista')";
            await using (var cmd = new NpgsqlCommand(crearEmpleado, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("sucursal", idSucursal);
                cmd.Parameters.AddWithValue("nombre", nombre);
                cmd.Parameters.AddWithValue("apellido", apellido);
                await cmd.ExecuteNonQueryAsync();
            }
            await transaccion.CommitAsync();
        }

        private static async Task VerificarPertenenciaAsync(
            NpgsqlConnection conexion, int idSucursal, int idEspecialista)
        {
            const string sql = @"
                SELECT EXISTS(
                    SELECT 1 FROM public.vw_gerente_gestion_empleados
                    WHERE ""IdEspecialista"" = @especialista
                      AND ""IdSucursal"" = @sucursal)";
            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("especialista", idEspecialista);
            cmd.Parameters.AddWithValue("sucursal", idSucursal);
            var existe = (bool)(await cmd.ExecuteScalarAsync() ?? false);
            if (!existe)
                throw new InvalidOperationException(
                    "El especialista no está vinculado a esta sucursal.");
        }

        public async Task EditarAsync(int idSucursal, EditarEntrada e)
        {
            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();
            await VerificarPertenenciaAsync(conexion, idSucursal, e.IdEspecialista);

            const string sql = @"
                UPDATE public.especialista
                SET rol_especialista = @cargo,
                    id_especialidad = @especialidad,
                    anios_experiencia = @experiencia,
                    disponibilidad = @disponibilidad
                WHERE id_especialista = @id";
            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("id", e.IdEspecialista);
            cmd.Parameters.AddWithValue("cargo", e.Cargo.Trim());
            cmd.Parameters.AddWithValue("especialidad", e.IdEspecialidad);
            cmd.Parameters.Add("experiencia", NpgsqlDbType.Integer).Value =
                (object?)e.AniosExperiencia ?? DBNull.Value;
            cmd.Parameters.AddWithValue("disponibilidad", e.Disponibilidad);
            if (await cmd.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Especialista no encontrado.");
        }

        private static void AgregarHora(NpgsqlCommand cmd, string nombre, TimeSpan? hora)
        {
            cmd.Parameters.Add(nombre, NpgsqlDbType.Time).Value =
                (object?)hora ?? DBNull.Value;
        }

        public async Task GuardarHorarioAsync(int idSucursal, HorarioEntrada e)
        {
            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();
            await VerificarPertenenciaAsync(conexion, idSucursal, e.IdEspecialista);
            await using var transaccion = await conexion.BeginTransactionAsync();

            // Serializa las modificaciones simultáneas del mismo especialista/día.
            await using (var bloqueo = new NpgsqlCommand(
                "SELECT pg_advisory_xact_lock(@id, @dia)", conexion, transaccion))
            {
                bloqueo.Parameters.AddWithValue("id", e.IdEspecialista);
                bloqueo.Parameters.AddWithValue("dia", e.DiaSemana);
                await bloqueo.ExecuteNonQueryAsync();
            }

            const string actualizar = @"
                UPDATE public.horario_semanal_especialista
                SET habilitado = @habilitado,
                    hora_inicio = @inicio,
                    hora_fin = @fin,
                    inicio_almuerzo = @almuerzoInicio,
                    fin_almuerzo = @almuerzoFin
                WHERE id_especialista = @id AND dia_semana = @dia";

            async Task<int> EjecutarAsync(string sql)
            {
                await using var cmd = new NpgsqlCommand(sql, conexion, transaccion);
                cmd.Parameters.AddWithValue("id", e.IdEspecialista);
                cmd.Parameters.AddWithValue("dia", e.DiaSemana);
                cmd.Parameters.AddWithValue("habilitado", e.Habilitado);
                AgregarHora(cmd, "inicio", e.Habilitado ? e.HoraInicio : null);
                AgregarHora(cmd, "fin", e.Habilitado ? e.HoraFin : null);
                AgregarHora(cmd, "almuerzoInicio", e.Habilitado ? e.InicioAlmuerzo : null);
                AgregarHora(cmd, "almuerzoFin", e.Habilitado ? e.FinAlmuerzo : null);
                return await cmd.ExecuteNonQueryAsync();
            }

            int filas = await EjecutarAsync(actualizar);
            if (filas == 0)
            {
                const string insertar = @"
                    INSERT INTO public.horario_semanal_especialista
                        (id_especialista, dia_semana, habilitado,
                         hora_inicio, hora_fin, inicio_almuerzo, fin_almuerzo)
                    VALUES (@id, @dia, @habilitado,
                            @inicio, @fin, @almuerzoInicio, @almuerzoFin)";
                await EjecutarAsync(insertar);
            }
            await transaccion.CommitAsync();
        }
    }
}
