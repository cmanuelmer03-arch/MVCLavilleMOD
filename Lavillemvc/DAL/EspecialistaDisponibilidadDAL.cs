using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Lavillemvc.Models;
using Npgsql;
using NpgsqlTypes;

namespace Lavillemvc.DAL
{
    // Todas las consultas y actualizaciones de disponibilidad se realizan aquí.
    public class EspecialistaDisponibilidadDAL
    {
        private readonly Conexion _conexion;
        public EspecialistaDisponibilidadDAL(Conexion conexion) => _conexion = conexion;

        private static readonly string[] NombresDias =
        {
            "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
        };

        public async Task<EspecialistaDisponibilidad?> ObtenerAsync(int idEspecialista, DateOnly hoy)
        {
            var model = new EspecialistaDisponibilidad { FechaHoy = hoy };
            await using var con = _conexion.GetConnection();
            await con.OpenAsync();

            const string perfil = @"
                SELECT e.id_especialista, e.nombre, e.rol_especialista,
                       e.id_especialidad, es.nombre_especialidad, e.disponibilidad,
                       TO_JSONB(e)->>'horario_especialistas'
                FROM public.especialista e
                JOIN public.especialidad es ON es.id_especialidad = e.id_especialidad
                WHERE e.id_especialista = @id";

            await using (var cmd = new NpgsqlCommand(perfil, con))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                await using var r = await cmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return null;
                model.IdEspecialista = r.GetInt32(0);
                model.NombreEspecialista = r.IsDBNull(1) ? "" : r.GetString(1);
                model.RolEspecialista = r.IsDBNull(2) ? null : r.GetString(2);
                model.IdEspecialidad = r.GetInt32(3);
                model.NombreEspecialidad = r.IsDBNull(4) ? "" : r.GetString(4);
                model.Disponibilidad = r.IsDBNull(5) ? null : r.GetBoolean(5);
                model.HorarioGeneralTexto = r.IsDBNull(6) ? null : r.GetString(6);
            }

            // Mostramos los 7 días, aunque un especialista todavía no tenga horarios guardados.
            for (int i = 1; i <= 7; i++)
                model.Horarios.Add(new EspecialistaDisponibilidad.HorarioDia
                {
                    DiaSemana = i,
                    NombreDia = NombresDias[i]
                });

            const string horariosSql = @"
                SELECT dia_semana, habilitado,
                       TO_CHAR(hora_inicio, 'HH24:MI'),
                       TO_CHAR(hora_fin, 'HH24:MI'),
                       TO_CHAR(inicio_almuerzo, 'HH24:MI'),
                       TO_CHAR(fin_almuerzo, 'HH24:MI')
                FROM public.horario_semanal_especialista
                WHERE id_especialista = @id
                ORDER BY dia_semana";

            await using (var cmd = new NpgsqlCommand(horariosSql, con))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int dia = Convert.ToInt32(r.GetValue(0));
                    if (dia < 1 || dia > 7) continue;
                    var h = model.Horarios[dia - 1];
                    h.Configurado = true;
                    h.Habilitado = !r.IsDBNull(1) && r.GetBoolean(1);
                    h.HoraInicio = Txt(r, 2);
                    h.HoraFin = Txt(r, 3);
                    h.InicioAlmuerzo = Txt(r, 4);
                    h.FinAlmuerzo = Txt(r, 5);
                }
            }

            const string ausenciasSql = @"
                SELECT id_ausencia, id_especialista, titulo, tipo, descripcion,
                       TO_CHAR(fecha_inicio::date, 'YYYY-MM-DD'),
                       TO_CHAR(fecha_fin::date, 'YYYY-MM-DD')
                FROM public.ausencia_especialista
                WHERE id_especialista = @id
                ORDER BY fecha_inicio DESC, id_ausencia DESC";

            await using (var cmd = new NpgsqlCommand(ausenciasSql, con))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    model.Ausencias.Add(new EspecialistaDisponibilidad.AusenciaEspecialista
                    {
                        IdAusencia = Convert.ToInt64(r.GetValue(0)),
                        IdEspecialista = Convert.ToInt32(r.GetValue(1)),
                        Titulo = Txt(r, 2) ?? "",
                        Tipo = Txt(r, 3) ?? "Otro",
                        Descripcion = Txt(r, 4),
                        FechaInicio = LeerFecha(r, 5),
                        FechaFin = LeerFecha(r, 6)
                    });
                }
            }

            model.DiasConfigurados = model.Horarios.Count(h => h.Configurado);
            model.TieneHorarioConfigurado = model.DiasConfigurados > 0;
            model.DiasActivos = model.Horarios.Count(h => h.Habilitado);
            model.HorasSemanales = model.TieneHorarioConfigurado
                ? model.Horarios.Where(h => h.Habilitado).Sum(HorasNetas)
                : null;
            var inactivos = model.Horarios.Where(h => !h.Habilitado)
                .Select(h => h.NombreDia).ToList();
            model.DescansoSemanal = model.TieneHorarioConfigurado
                ? (inactivos.Count == 0 ? "Sin día fijo" : string.Join(", ", inactivos))
                : "Sin configurar";

            var proxima = model.Ausencias
                .Where(a => a.FechaFin >= hoy)
                .OrderBy(a => a.FechaInicio)
                .FirstOrDefault();
            if (proxima != null)
            {
                model.ProximaAusenciaTitulo = proxima.Titulo;
                model.ProximaAusenciaTipo = proxima.Tipo;
                model.ProximaAusenciaInicio = proxima.FechaInicio;
                model.ProximaAusenciaFin = proxima.FechaFin;
            }
            model.HorariosJson = JsonSerializer.Serialize(model.Horarios);
            model.AusenciasJson = JsonSerializer.Serialize(model.Ausencias);
            return model;
        }

        public async Task GuardarHorarioAsync(int idEspecialista, List<HorarioEditado> dias, DateOnly hoy, TimeOnly ahora)
        {
            if (idEspecialista <= 0 || dias == null || dias.Count != 7 ||
                dias.Select(d => d.DiaSemana).Distinct().Count() != 7 ||
                dias.Any(d => d.DiaSemana < 1 || d.DiaSemana > 7))
                throw new ArgumentException("Se deben enviar los siete días, sin repetirlos.");

            var normales = dias.Select(ValidarHorario).ToDictionary(d => d.DiaSemana);
            await using var con = _conexion.GetConnection();
            await con.OpenAsync();
            await using var tx = await con.BeginTransactionAsync();
            await BloquearEspecialistaAsync(con, tx, idEspecialista);

            // No permitir cambios que dejen citas futuras fuera del nuevo horario.
            // Sin una duración real por cita solo podemos verificar su hora de inicio.
            const string citasSql = @"
                SELECT c.id_cita, TO_CHAR(c.fecha_cita::date, 'YYYY-MM-DD'),
                       TO_CHAR(c.hora_cita, 'HH24:MI')
                FROM public.cita c
                WHERE EXISTS (
                    SELECT 1 FROM public.especialistas_en_cita ec
                    WHERE ec.id_cita = c.id_cita AND ec.id_especialista = @id
                )
                AND (c.fecha_cita > @hoy OR (c.fecha_cita = @hoy AND c.hora_cita >= @ahora))
                AND LOWER(TRIM(COALESCE(c.estado, ''))) NOT IN
                    ('cancelada', 'cancelado', 'completada', 'completado', 'finalizada')";
            await using (var cmd = new NpgsqlCommand(citasSql, con, tx))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                cmd.Parameters.AddWithValue("hoy", hoy);
                cmd.Parameters.Add("ahora", NpgsqlDbType.Time).Value = ahora.ToTimeSpan();
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int idCita = r.GetInt32(0);
                    DateOnly fecha = LeerFecha(r, 1);
                    var hora = TimeOnly.ParseExact(r.GetString(2), "HH:mm", CultureInfo.InvariantCulture);
                    int dia = ((int)fecha.DayOfWeek + 6) % 7 + 1;
                    var h = normales[dia];
                    if (!h.Habilitado || hora < h.Inicio || hora >= h.Fin ||
                        (h.AlmuerzoInicio.HasValue && hora >= h.AlmuerzoInicio.Value &&
                         hora < h.AlmuerzoFin!.Value))
                    {
                        throw new InvalidOperationException(
                            $"El cambio dejaría fuera de horario la cita #{idCita} del {fecha:dd/MM/yyyy} a las {hora:HH:mm}. Reprograma esa cita antes de guardar.");
                    }
                }
            }

            const string actualizar = @"
                UPDATE public.horario_semanal_especialista
                   SET habilitado = @habilitado, hora_inicio = @inicio, hora_fin = @fin,
                       inicio_almuerzo = @almuerzoInicio, fin_almuerzo = @almuerzoFin
                 WHERE id_especialista = @id AND dia_semana = @dia";
            const string insertar = @"
                INSERT INTO public.horario_semanal_especialista
                    (id_especialista, dia_semana, habilitado, hora_inicio, hora_fin,
                     inicio_almuerzo, fin_almuerzo)
                VALUES (@id, @dia, @habilitado, @inicio, @fin, @almuerzoInicio, @almuerzoFin)";

            foreach (var horario in normales.Values.OrderBy(h => h.DiaSemana))
            {
                await using var up = NuevoCmdHorario(actualizar, con, tx, idEspecialista, horario);
                if (await up.ExecuteNonQueryAsync() == 0)
                {
                    await using var ins = NuevoCmdHorario(insertar, con, tx, idEspecialista, horario);
                    await ins.ExecuteNonQueryAsync();
                }
            }
            await tx.CommitAsync();
        }

        public async Task AgregarAusenciaAsync(int idEspecialista, NuevaAusencia datos, DateOnly hoy)
        {
            if (idEspecialista <= 0) throw new ArgumentException("Especialista inválido.");
            if (string.IsNullOrWhiteSpace(datos.Titulo) || datos.Titulo.Trim().Length < 3 || datos.Titulo.Trim().Length > 100)
                throw new ArgumentException("El título debe tener entre 3 y 100 caracteres.");
            if (!new[] { "Vacaciones", "Capacitación", "Personal", "Otro" }.Contains(datos.Tipo))
                throw new ArgumentException("Selecciona un tipo de ausencia válido.");
            if (datos.Descripcion?.Length > 500)
                throw new ArgumentException("La descripción no puede superar los 500 caracteres.");
            if (datos.Inicio < hoy || datos.Fin < datos.Inicio || datos.Inicio > hoy.AddYears(2) ||
                datos.Fin.DayNumber - datos.Inicio.DayNumber > 365)
                throw new ArgumentException("Elige fechas futuras válidas y un intervalo de hasta 366 días.");

            await using var con = _conexion.GetConnection();
            await con.OpenAsync();
            await using var tx = await con.BeginTransactionAsync();
            await BloquearEspecialistaAsync(con, tx, idEspecialista);

            const string solapadas = @"
                SELECT EXISTS (
                    SELECT 1 FROM public.ausencia_especialista a
                    WHERE a.id_especialista = @id
                    AND a.fecha_inicio::date <= @fin AND a.fecha_fin::date >= @inicio
                )";
            await using (var cmd = new NpgsqlCommand(solapadas, con, tx))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                cmd.Parameters.AddWithValue("inicio", datos.Inicio);
                cmd.Parameters.AddWithValue("fin", datos.Fin);
                if ((bool)(await cmd.ExecuteScalarAsync() ?? false))
                    throw new InvalidOperationException("Ya hay otra ausencia registrada en esas fechas.");
            }

            const string citas = @"
                SELECT COUNT(*)
                FROM public.cita c
                WHERE c.fecha_cita BETWEEN @inicio AND @fin
                AND EXISTS (SELECT 1 FROM public.especialistas_en_cita ec
                            WHERE ec.id_cita = c.id_cita AND ec.id_especialista = @id)
                AND LOWER(TRIM(COALESCE(c.estado, ''))) NOT IN
                    ('cancelada', 'cancelado', 'completada', 'completado', 'finalizada')";
            await using (var cmd = new NpgsqlCommand(citas, con, tx))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                cmd.Parameters.AddWithValue("inicio", datos.Inicio);
                cmd.Parameters.AddWithValue("fin", datos.Fin);
                long cantidad = Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0L);
                if (cantidad > 0)
                    throw new InvalidOperationException(
                        $"Hay {cantidad} cita(s) activa(s) en esas fechas. Reprográmalas antes de registrar la ausencia.");
            }

            const string sql = @"
                INSERT INTO public.ausencia_especialista
                    (id_especialista, titulo, tipo, descripcion, fecha_inicio, fecha_fin)
                VALUES (@id, @titulo, @tipo, @descripcion, @inicio, @fin)";
            await using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                cmd.Parameters.AddWithValue("id", idEspecialista);
                cmd.Parameters.AddWithValue("titulo", datos.Titulo.Trim());
                cmd.Parameters.AddWithValue("tipo", datos.Tipo);
                cmd.Parameters.AddWithValue("descripcion", (object?)datos.Descripcion?.Trim() ?? DBNull.Value);
                cmd.Parameters.AddWithValue("inicio", datos.Inicio);
                cmd.Parameters.AddWithValue("fin", datos.Fin);
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }

        public sealed class HorarioEditado
        {
            public int DiaSemana { get; set; }
            public bool Habilitado { get; set; }
            public string? HoraInicio { get; set; }
            public string? HoraFin { get; set; }
            public string? InicioAlmuerzo { get; set; }
            public string? FinAlmuerzo { get; set; }
        }

        public sealed class NuevaAusencia
        {
            public string Titulo { get; set; } = "";
            public string Tipo { get; set; } = "";
            public string? Descripcion { get; set; }
            public DateOnly Inicio { get; set; }
            public DateOnly Fin { get; set; }
        }

        private sealed class HorarioValidado
        {
            public int DiaSemana { get; set; }
            public bool Habilitado { get; set; }
            public TimeOnly? Inicio { get; set; }
            public TimeOnly? Fin { get; set; }
            public TimeOnly? AlmuerzoInicio { get; set; }
            public TimeOnly? AlmuerzoFin { get; set; }
        }

        private static HorarioValidado ValidarHorario(HorarioEditado d)
        {
            var salida = new HorarioValidado { DiaSemana = d.DiaSemana, Habilitado = d.Habilitado };
            if (!d.Habilitado) return salida;
            if (!TimeOnly.TryParseExact(d.HoraInicio, "HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var inicio) ||
                !TimeOnly.TryParseExact(d.HoraFin, "HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var fin))
                throw new ArgumentException($"Completa horas de inicio y fin válidas para {NombresDias[d.DiaSemana]}.");
            if (fin <= inicio || fin.ToTimeSpan() - inicio.ToTimeSpan() > TimeSpan.FromHours(16))
                throw new ArgumentException($"La jornada de {NombresDias[d.DiaSemana]} debe durar entre 1 minuto y 16 horas.");
            salida.Inicio = inicio;
            salida.Fin = fin;

            bool alguno = !string.IsNullOrWhiteSpace(d.InicioAlmuerzo) || !string.IsNullOrWhiteSpace(d.FinAlmuerzo);
            if (alguno)
            {
                if (!TimeOnly.TryParseExact(d.InicioAlmuerzo, "HH:mm", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var comidaInicio) ||
                    !TimeOnly.TryParseExact(d.FinAlmuerzo, "HH:mm", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var comidaFin) ||
                    comidaInicio < inicio || comidaFin > fin || comidaFin <= comidaInicio)
                    throw new ArgumentException($"El almuerzo de {NombresDias[d.DiaSemana]} debe estar dentro de la jornada y tener inicio anterior al fin.");
                salida.AlmuerzoInicio = comidaInicio;
                salida.AlmuerzoFin = comidaFin;
            }
            return salida;
        }

        private static NpgsqlCommand NuevoCmdHorario(string sql, NpgsqlConnection con,
            NpgsqlTransaction tx, int id, HorarioValidado h)
        {
            var cmd = new NpgsqlCommand(sql, con, tx);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("dia", h.DiaSemana);
            cmd.Parameters.AddWithValue("habilitado", h.Habilitado);
            Tiempo(cmd, "inicio", h.Inicio);
            Tiempo(cmd, "fin", h.Fin);
            Tiempo(cmd, "almuerzoInicio", h.AlmuerzoInicio);
            Tiempo(cmd, "almuerzoFin", h.AlmuerzoFin);
            return cmd;
        }

        private static void Tiempo(NpgsqlCommand cmd, string nombre, TimeOnly? hora) =>
            cmd.Parameters.Add(nombre, NpgsqlDbType.Time).Value =
                hora.HasValue ? (object)hora.Value.ToTimeSpan() : DBNull.Value;

        private static async Task BloquearEspecialistaAsync(NpgsqlConnection con, NpgsqlTransaction tx, int id)
        {
            await using var cmd = new NpgsqlCommand("SELECT pg_advisory_xact_lock(82351, @id)", con, tx);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        private static DateOnly LeerFecha(NpgsqlDataReader r, int i) =>
            DateOnly.ParseExact(r.GetString(i), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static string? Txt(NpgsqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);

        private static decimal HorasNetas(EspecialistaDisponibilidad.HorarioDia h)
        {
            if (!TimeSpan.TryParse(h.HoraInicio, out var inicio) ||
                !TimeSpan.TryParse(h.HoraFin, out var fin) || fin <= inicio) return 0m;
            decimal horas = (decimal)(fin - inicio).TotalHours;
            if (TimeSpan.TryParse(h.InicioAlmuerzo, out var a) &&
                TimeSpan.TryParse(h.FinAlmuerzo, out var b) && b > a)
                horas -= (decimal)(b - a).TotalHours;
            return Math.Round(Math.Max(0m, horas), 2);
        }
    }
}
