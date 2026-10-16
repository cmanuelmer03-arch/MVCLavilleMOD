using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lavillemvc.Models;
using Npgsql;

namespace Lavillemvc.DAL
{
    public class EspecialistaAgendaDAL
    {
        private readonly Conexion _conexion;

        public EspecialistaAgendaDAL(Conexion conexion)
        {
            _conexion = conexion;
        }

        private const string ColumnasCita = @"
            SELECT c.id_cita, c.id_cliente,
                   CONCAT_WS(' ', cl.nombre, cl.apellido) AS nombre_cliente,
                   cl.tipo_cliente,
                   c.id_servicio,
                   COALESCE(s.nombre, 'Servicio sin identificar') AS servicio,
                   c.fecha_cita::text AS fecha_cita,
                   TO_CHAR(c.hora_cita, 'HH24:MI') AS hora_cita,
                   COALESCE(c.estado, 'Sin estado') AS estado,
                   e.id_sucursal,
                   su.nombre_sucursal
            FROM public.cita c
            INNER JOIN public.cliente cl ON cl.id_cliente = c.id_cliente
            LEFT JOIN public.servicios s ON s.id_servicios = c.id_servicio
            LEFT JOIN public.empleado e ON e.id_empleado = c.id_empleado
            LEFT JOIN public.sucursal su ON su.id_sucursal = e.id_sucursal
            WHERE EXISTS (
                SELECT 1
                FROM public.especialistas_en_cita a
                WHERE a.id_cita = c.id_cita
                  AND a.id_especialista = @idEspecialista
            )";

        public async Task<EspecialistaAgenda?> ObtenerAgendaAsync(
            int idEspecialista,
            DateOnly fechaReferencia,
            DateOnly inicio,
            DateOnly fin,
            DateOnly fechaActual,
            TimeOnly horaActual)
        {
            if (idEspecialista <= 0 || inicio > fin)
                throw new ArgumentException("El especialista o el rango no son válidos.");

            var agenda = new EspecialistaAgenda { FechaHoy = fechaReferencia };

            await using var con = _conexion.GetConnection();
            await con.OpenAsync();

            // La columna opcional horario_especialistas no se exige en el esquema original.
            const string perfilSql = @"
                SELECT e.id_especialista, e.nombre, e.rol_especialista,
                       e.id_especialidad, es.nombre_especialidad,
                       e.anios_experiencia, e.disponibilidad,
                       TO_JSONB(e)->>'horario_especialistas' AS horario_especialistas
                FROM public.especialista e
                JOIN public.especialidad es
                  ON es.id_especialidad = e.id_especialidad
                WHERE e.id_especialista = @idEspecialista";

            await using (var cmd = new NpgsqlCommand(perfilSql, con))
            {
                cmd.Parameters.AddWithValue("idEspecialista", idEspecialista);
                await using var r = await cmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return null;

                agenda.IdEspecialista = r.GetInt32(0);
                agenda.NombreEspecialista = LeerTexto(r, 1) ?? "";
                agenda.RolEspecialista = LeerTexto(r, 2);
                agenda.IdEspecialidad = r.GetInt32(3);
                agenda.NombreEspecialidad = LeerTexto(r, 4) ?? "";
                agenda.AniosExperiencia = r.IsDBNull(5) ? null : r.GetInt32(5);
                agenda.Disponibilidad = r.IsDBNull(6) ? null : r.GetBoolean(6);
                agenda.HorarioEspecialistas = LeerTexto(r, 7);
            }

            var sqlCitas = ColumnasCita + @"
                AND c.fecha_cita BETWEEN @inicio AND @fin
                ORDER BY c.fecha_cita, c.hora_cita, c.id_cita";

            await using (var cmd = new NpgsqlCommand(sqlCitas, con))
            {
                cmd.Parameters.AddWithValue("idEspecialista", idEspecialista);
                cmd.Parameters.AddWithValue("inicio", inicio);
                cmd.Parameters.AddWithValue("fin", fin);
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    agenda.Citas.Add(MapearCita(r));
            }

            agenda.CitasHoy = agenda.Citas.Count(c => c.EstadoNormalizado != "cancelada");
            agenda.PendientesHoy = agenda.Citas.Count(c => c.EstadoNormalizado == "pendiente");

            // La próxima cita se busca independientemente del filtro Hoy/Semana/Mes.
            var sqlProxima = ColumnasCita + @"
                AND (c.fecha_cita > @fechaActual
                     OR (c.fecha_cita = @fechaActual AND c.hora_cita >= @horaActual))
                AND LOWER(TRIM(COALESCE(c.estado, ''))) NOT IN
                    ('cancelada', 'cancelado', 'completada', 'completado', 'finalizada')
                ORDER BY c.fecha_cita, c.hora_cita, c.id_cita
                LIMIT 1";

            await using (var cmd = new NpgsqlCommand(sqlProxima, con))
            {
                cmd.Parameters.AddWithValue("idEspecialista", idEspecialista);
                cmd.Parameters.AddWithValue("fechaActual", fechaActual);
                cmd.Parameters.AddWithValue("horaActual", horaActual);
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    agenda.ProximaCita = MapearCita(r);
            }

            await CargarHorarioSiExisteAsync(con, agenda, fechaReferencia);
            return agenda;
        }

        private static async Task CargarHorarioSiExisteAsync(
            NpgsqlConnection con,
            EspecialistaAgenda agenda,
            DateOnly fecha)
        {
            // En instalaciones que aún no tengan esta tabla, no falla el dashboard.
            await using (var comprobar = new NpgsqlCommand(
                "SELECT TO_REGCLASS('public.horario_semanal_especialista') IS NOT NULL", con))
            {
                if (!(bool)(await comprobar.ExecuteScalarAsync() ?? false))
                    return;
            }

            // 1=Lunes ... 7=Domingo (estructura creada para Mi Disponibilidad).
            int dia = ((int)fecha.DayOfWeek + 6) % 7 + 1;
            const string sql = @"
                SELECT
                    ROUND(GREATEST(0,
                        EXTRACT(EPOCH FROM (hora_fin - hora_inicio))/3600)::numeric, 2),
                    ROUND(GREATEST(0,
                        COALESCE(EXTRACT(EPOCH FROM
                            (fin_almuerzo - inicio_almuerzo)),0)/3600)::numeric, 2)
                FROM public.horario_semanal_especialista
                WHERE id_especialista = @id
                  AND dia_semana = @dia
                  AND habilitado = TRUE
                  AND hora_inicio IS NOT NULL
                  AND hora_fin IS NOT NULL
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("id", agenda.IdEspecialista);
            cmd.Parameters.AddWithValue("dia", dia);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                decimal jornada = r.IsDBNull(0) ? 0m : r.GetDecimal(0);
                decimal descanso = r.IsDBNull(1) ? 0m : r.GetDecimal(1);
                agenda.HorasDescanso = descanso;
                agenda.HorasAtencion = Math.Max(0m, jornada - descanso);
                // No hay duración por cita en la base original, no es seguro
                // calcular horas libres reales solo restando número de citas.
                agenda.HorasDisponibles = null;
            }
        }

        public async Task<bool> ConfirmarCitaAsync(
            int idEspecialista, int idCita, DateOnly hoy)
        {
            if (idEspecialista <= 0 || idCita <= 0)
                return false;

            await using var con = _conexion.GetConnection();
            await con.OpenAsync();
            await using var tx = await con.BeginTransactionAsync();

            // Serializa confirmaciones del mismo especialista.
            await using (var bloqueo = new NpgsqlCommand(
                "SELECT pg_advisory_xact_lock(81203, @id)", con, tx))
            {
                bloqueo.Parameters.AddWithValue("id", idEspecialista);
                await bloqueo.ExecuteNonQueryAsync();
            }

            // Validaciones atómicas: cita propia, pendiente, no pasada y
            // sin otra cita ya confirmada a esa hora exacta.
            const string sql = @"
                UPDATE public.cita c
                SET estado = 'Confirmada'
                WHERE c.id_cita = @idCita
                  AND c.fecha_cita >= @hoy
                  AND LOWER(TRIM(COALESCE(c.estado, ''))) = 'pendiente'
                  AND EXISTS (
                      SELECT 1 FROM public.especialistas_en_cita a
                      WHERE a.id_cita = c.id_cita
                        AND a.id_especialista = @idEspecialista
                  )
                  AND NOT EXISTS (
                      SELECT 1
                      FROM public.cita otra
                      JOIN public.especialistas_en_cita a2
                        ON a2.id_cita = otra.id_cita
                      WHERE a2.id_especialista = @idEspecialista
                        AND otra.id_cita <> c.id_cita
                        AND otra.fecha_cita = c.fecha_cita
                        AND otra.hora_cita = c.hora_cita
                        AND LOWER(TRIM(COALESCE(otra.estado, ''))) IN
                            ('confirmada', 'en curso', 'completada')
                  )
                RETURNING c.id_cita";

            await using var cmd = new NpgsqlCommand(sql, con, tx);
            cmd.Parameters.AddWithValue("idCita", idCita);
            cmd.Parameters.AddWithValue("idEspecialista", idEspecialista);
            cmd.Parameters.AddWithValue("hoy", hoy);
            bool confirmado = await cmd.ExecuteScalarAsync() != null;
            await tx.CommitAsync();
            return confirmado;
        }

        private static EspecialistaAgenda.CitaAgenda MapearCita(NpgsqlDataReader r)
        {
            var fecha = DateOnly.ParseExact(r.GetString(6), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var hora = TimeOnly.ParseExact(r.GetString(7), "HH:mm", CultureInfo.InvariantCulture);
            var estado = LeerTexto(r, 8) ?? "Sin estado";
            return new EspecialistaAgenda.CitaAgenda
            {
                IdCita = r.GetInt32(0),
                IdCliente = r.GetInt32(1),
                NombreCliente = LeerTexto(r, 2) ?? "",
                TipoCliente = LeerTexto(r, 3),
                IdServicio = r.GetInt32(4),
                Servicio = LeerTexto(r, 5) ?? "",
                FechaCita = fecha,
                HoraCita = hora,
                FechaHoraCita = fecha.ToDateTime(hora),
                Estado = estado,
                EstadoNormalizado = NormalizarEstado(estado),
                IdSucursal = r.IsDBNull(9) ? null : r.GetInt32(9),
                NombreSucursal = LeerTexto(r, 10),
                // Estos campos no existen en las tablas originales:
                DuracionMinutos = null,
                NotasDiagnostico = null,
                ProductosNecesarios = null
            };
        }

        private static string? LeerTexto(NpgsqlDataReader r, int i) =>
            r.IsDBNull(i) ? null : r.GetString(i);

        private static string NormalizarEstado(string estado)
        {
            var e = estado.Trim().ToLowerInvariant();
            return e switch
            {
                "confirmada" or "confirmado" => "confirmada",
                "pendiente" => "pendiente",
                "en curso" => "en-curso",
                "completada" or "completado" or "finalizada" => "completada",
                "cancelada" or "cancelado" => "cancelada",
                _ => "otro"
            };
        }
    }
}
