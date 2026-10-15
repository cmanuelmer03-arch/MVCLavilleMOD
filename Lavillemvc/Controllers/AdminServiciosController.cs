
using System;
using System.Collections.Generic;
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
    public class AdminServiciosController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AdminServiciosController> _logger;

        public AdminServiciosController(
            IConfiguration config,
            ILogger<AdminServiciosController> logger)
        {
            _config = config;
            _logger = logger;
        }

        private NpgsqlConnection ConexionBD()
        {
            return new Conexion(_config).GetConnection();
        }

        private static string Texto(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? ""
                : Convert.ToString(r.GetValue(i)) ?? "";
        }

        private static int Entero(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? 0
                : Convert.ToInt32(r.GetValue(i));
        }

        private static int? EnteroN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? null
                : Convert.ToInt32(r.GetValue(i));
        }

        private static bool? BooleanoN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? null
                : Convert.ToBoolean(r.GetValue(i));
        }

        private static DateTime? FechaN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? null
                : r.GetDateTime(i);
        }

        private static decimal? DecimalN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);

            return r.IsDBNull(i)
                ? null
                : Convert.ToDecimal(r.GetValue(i));
        }

        // =========================================
        // FORMULARIOS
        // =========================================

        public class FormServicio
        {
            public int IdServicio { get; set; }

            public int? IdEspecialidad { get; set; }

            public string Nombre { get; set; } = "";

            public string? Descripcion { get; set; }

            public decimal Costo { get; set; }

            public int? DuracionMinutos { get; set; }

            public bool Disponible { get; set; } = true;
        }

        public class FormMembresia
        {
            public int IdMembresia { get; set; }

            public string Nombre { get; set; } = "";

            public string? Beneficios { get; set; }

            public string? Duracion { get; set; }

            public decimal? PrecioMensual { get; set; }

            public bool Activo { get; set; } = true;
        }

        // =========================================
        // CONSULTAR SERVICIOS Y MEMBRESÍAS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Datos()
        {
            try
            {
                var datos = new AdminGestionServicios();

                var miembros = new List<object>();

                var especialidades = new List<object>();

                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                // SERVICIOS
                using (var cmd = new NpgsqlCommand(@"
                    SELECT *
                    FROM public.vw_admin_gestion_servicios
                    ORDER BY ""IdServicio""", conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        datos.Servicios.Add(new AdminServicioItem
                        {
                            IdServicio =
                                Entero(r, "IdServicio"),

                            IdEspecialidad =
                                EnteroN(r, "IdEspecialidad"),

                            IdServicioPadre =
                                EnteroN(r, "IdServicioPadre"),

                            Nombre =
                                Texto(r, "Nombre"),

                            Descripcion =
                                Texto(r, "Descripcion"),

                            Costo =
                                DecimalN(r, "Costo") ?? 0,

                            DuracionMinutos =
                                EnteroN(r, "DuracionMinutos"),

                            Disponible =
                                BooleanoN(r, "Disponible"),

                            EstadoServicio =
                                Texto(r, "EstadoServicio"),

                            ReservasAsociadas =
                                Entero(r, "ReservasAsociadas"),

                            SucursalesAsociadas =
                                Entero(r, "SucursalesAsociadas"),

                            SucursalesDisponibles =
                                Entero(r, "SucursalesDisponibles"),

                            PromocionesAsociadas =
                                Entero(r, "PromocionesAsociadas")
                        });
                    }
                }

                // MEMBRESÍAS
                using (var cmd = new NpgsqlCommand(@"
                    SELECT *
                    FROM public.vw_admin_gestion_membresias
                    ORDER BY ""IdMembresia""", conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        datos.Membresias.Add(new AdminMembresiaItem
                        {
                            IdMembresia =
                                Entero(r, "IdMembresia"),

                            Nombre =
                                Texto(r, "Nombre"),

                            Beneficios =
                                Texto(r, "Beneficios"),

                            Duracion =
                                Texto(r, "Duracion"),

                            PrecioMensual =
                                DecimalN(r, "PrecioMensual"),

                            Activo =
                                BooleanoN(r, "Activo"),

                            EstadoMembresia =
                                Texto(r, "EstadoMembresia"),

                            MiembrosActivos =
                                Entero(r, "MiembrosActivos")
                        });
                    }
                }

                // MIEMBROS REALES DE CADA PLAN
                using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        ""IdCliente"",
                        ""IdMembresia"",
                        ""NombreCompleto"",
                        ""EstadoMembresia"",
                        ""FechaInicio"",
                        ""FechaFin"",
                        ""EsVigente""
                    FROM public.vw_admin_miembros_membresia
                    ORDER BY ""NombreCompleto""", conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        miembros.Add(new
                        {
                            IdCliente =
                                Entero(r, "IdCliente"),

                            IdMembresia =
                                Entero(r, "IdMembresia"),

                            NombreCompleto =
                                Texto(r, "NombreCompleto"),

                            EstadoMembresia =
                                Texto(r, "EstadoMembresia"),

                            FechaInicio =
                                FechaN(r, "FechaInicio"),

                            FechaFin =
                                FechaN(r, "FechaFin"),

                            EsVigente =
                                BooleanoN(r, "EsVigente") == true
                        });
                    }
                }

                // ESPECIALIDADES DISPONIBLES
                using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        id_especialidad,
                        nombre_especialidad
                    FROM public.especialidad
                    ORDER BY nombre_especialidad", conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        especialidades.Add(new
                        {
                            Id = r.GetInt32(0),
                            Nombre = r.GetString(1)
                        });
                    }
                }

                return Json(new
                {
                    datos.Servicios,
                    datos.Membresias,
                    Miembros = miembros,
                    Especialidades = especialidades
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error consultando servicios");

                return StatusCode(500, new
                {
                    mensaje =
                        "No se pudieron cargar los datos: "
                        + ex.Message
                });
            }
        }

        // =========================================
        // VALIDACIONES Y PARÁMETROS
        // =========================================

        private static void ValidarServicio(FormServicio d)
        {
            if (string.IsNullOrWhiteSpace(d.Nombre))
            {
                throw new Exception(
                    "El nombre del servicio es obligatorio.");
            }

            if (d.Costo < 0)
            {
                throw new Exception(
                    "El precio no puede ser negativo.");
            }

            if (d.DuracionMinutos.HasValue &&
                d.DuracionMinutos.Value <= 0)
            {
                throw new Exception(
                    "La duración debe ser mayor que cero.");
            }
        }

        private static void ParametrosServicio(
            NpgsqlCommand cmd,
            FormServicio d)
        {
            cmd.Parameters.AddWithValue(
                "nombre", d.Nombre.Trim());

            cmd.Parameters.AddWithValue(
                "descripcion",
                (object?)d.Descripcion?.Trim()
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "costo", d.Costo);

            cmd.Parameters.Add(
                "duracion",
                NpgsqlDbType.Integer).Value =
                d.DuracionMinutos.HasValue
                    ? (object)d.DuracionMinutos.Value
                    : DBNull.Value;

            cmd.Parameters.Add(
                "especialidad",
                NpgsqlDbType.Integer).Value =
                d.IdEspecialidad.HasValue
                    ? (object)d.IdEspecialidad.Value
                    : DBNull.Value;

            cmd.Parameters.AddWithValue(
                "disponible", d.Disponible);
        }

        private static void ValidarMembresia(FormMembresia d)
        {
            if (string.IsNullOrWhiteSpace(d.Nombre))
            {
                throw new Exception(
                    "El nombre de la membresía es obligatorio.");
            }

            if (d.PrecioMensual.HasValue &&
                d.PrecioMensual.Value < 0)
            {
                throw new Exception(
                    "El precio no puede ser negativo.");
            }
        }

        private static void ParametrosMembresia(
            NpgsqlCommand cmd,
            FormMembresia d)
        {
            cmd.Parameters.AddWithValue(
                "nombre", d.Nombre.Trim());

            cmd.Parameters.AddWithValue(
                "beneficios",
                (object?)d.Beneficios?.Trim()
                ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "duracion",
                (object?)d.Duracion?.Trim()
                ?? DBNull.Value);

            cmd.Parameters.Add(
                "precio",
                NpgsqlDbType.Numeric).Value =
                d.PrecioMensual.HasValue
                    ? (object)d.PrecioMensual.Value
                    : DBNull.Value;

            cmd.Parameters.AddWithValue(
                "activo", d.Activo);
        }

        private IActionResult Volver()
        {
            return RedirectToAction(
                "GestionServicios", "Admin");
        }

        // =========================================
        // AGREGAR SERVICIO
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarServicio(
            FormServicio datos)
        {
            try
            {
                ValidarServicio(datos);

                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var tx =
                    await conexion.BeginTransactionAsync();

                using (var bloqueo = new NpgsqlCommand(
                    "SELECT pg_advisory_xact_lock(42174, 3)",
                    conexion, tx))
                {
                    await bloqueo.ExecuteNonQueryAsync();
                }

                int id;

                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO public.servicios
                    (
                        id_servicios,
                        id_especialidad,
                        nombre,
                        descripcion,
                        costo,
                        duracion_minutos,
                        disponible
                    )
                    SELECT
                        COALESCE(MAX(id_servicios), 0) + 1,
                        @especialidad,
                        @nombre,
                        @descripcion,
                        @costo,
                        @duracion,
                        @disponible
                    FROM public.servicios
                    RETURNING id_servicios",
                    conexion, tx))
                {
                    ParametrosServicio(cmd, datos);

                    id = Convert.ToInt32(
                        await cmd.ExecuteScalarAsync());
                }

                // Se registra el nuevo servicio
                // en las sucursales abiertas.

                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO public.sucursal_servicios
                    (
                        id_sucursal_servicios,
                        id_sucursal,
                        id_servicios,
                        disponible
                    )
                    SELECT
                        base.ultimo_id
                        + ROW_NUMBER() OVER (
                            ORDER BY s.id_sucursal
                        ),
                        s.id_sucursal,
                        @servicio,
                        @disponible
                    FROM public.sucursal s
                    CROSS JOIN (
                        SELECT
                            COALESCE(
                                MAX(id_sucursal_servicios),
                                0
                            ) AS ultimo_id
                        FROM public.sucursal_servicios
                    ) base
                    WHERE s.abierto IS TRUE",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue(
                        "servicio", id);

                    cmd.Parameters.AddWithValue(
                        "disponible", datos.Disponible);

                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                TempData["MensajeServicios"] =
                    "Servicio agregado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error agregando servicio");

                TempData["ErrorServicios"] =
                    "No se pudo agregar el servicio: "
                    + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // EDITAR SERVICIO
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarServicio(
            FormServicio datos)
        {
            try
            {
                ValidarServicio(datos);

                if (datos.IdServicio <= 0)
                {
                    throw new Exception(
                        "Selecciona un servicio válido.");
                }

                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE public.servicios
                    SET
                        nombre = @nombre,
                        descripcion = @descripcion,
                        costo = @costo,
                        duracion_minutos = @duracion,
                        id_especialidad = @especialidad,
                        disponible = @disponible
                    WHERE id_servicios = @id",
                    conexion);

                ParametrosServicio(cmd, datos);

                cmd.Parameters.AddWithValue(
                    "id", datos.IdServicio);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "El servicio no existe.");
                }

                TempData["MensajeServicios"] =
                    "Servicio actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error editando servicio");

                TempData["ErrorServicios"] =
                    "No se pudo editar: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // ACTIVAR / DESACTIVAR SERVICIO
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoServicio(
            int idServicio,
            bool activar)
        {
            try
            {
                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE public.servicios
                    SET disponible = @activo
                    WHERE id_servicios = @id",
                    conexion);

                cmd.Parameters.AddWithValue(
                    "activo", activar);

                cmd.Parameters.AddWithValue(
                    "id", idServicio);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "El servicio no existe.");
                }

                TempData["MensajeServicios"] = activar
                    ? "Servicio activado correctamente."
                    : "Servicio desactivado; conserva sus datos históricos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error cambiando estado del servicio");

                TempData["ErrorServicios"] =
                    "No se pudo cambiar el estado: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // ELIMINAR SERVICIO
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarServicio(
            int idServicio)
        {
            try
            {
                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var tx =
                    await conexion.BeginTransactionAsync();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT id_servicios
                    FROM public.servicios
                    WHERE id_servicios = @id
                    FOR UPDATE",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue(
                        "id", idServicio);

                    if (await cmd.ExecuteScalarAsync() == null)
                    {
                        throw new Exception(
                            "El servicio no existe.");
                    }
                }

                using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        EXISTS(
                            SELECT 1 FROM public.cita
                            WHERE id_servicio = @id
                        )
                        OR EXISTS(
                            SELECT 1 FROM public.servicios_en_cita
                            WHERE id_servicios = @id
                        )
                        OR EXISTS(
                            SELECT 1 FROM public.promocion_servicio
                            WHERE id_servicios = @id
                        )
                        OR EXISTS(
                            SELECT 1 FROM public.servicios
                            WHERE id_servicio_padre = @id
                        )",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue(
                        "id", idServicio);

                    bool tieneRelaciones =
                        Convert.ToBoolean(
                            await cmd.ExecuteScalarAsync());

                    if (tieneRelaciones)
                    {
                        throw new Exception(
                            "El servicio tiene citas, promociones " +
                            "o servicios relacionados. Desactívalo " +
                            "para conservar su historial.");
                    }
                }

                // Solo elimina las asignaciones de este
                // servicio a sucursales, si no tiene
                // registros históricos relacionados.

                using (var cmd = new NpgsqlCommand(@"
                    DELETE FROM public.sucursal_servicios
                    WHERE id_servicios = @id",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue(
                        "id", idServicio);

                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand(@"
                    DELETE FROM public.servicios
                    WHERE id_servicios = @id",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue(
                        "id", idServicio);

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                    {
                        throw new Exception(
                            "El servicio no existe.");
                    }
                }

                await tx.CommitAsync();

                TempData["MensajeServicios"] =
                    "Servicio eliminado correctamente.";
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["ErrorServicios"] =
                    "No se puede eliminar porque tiene " +
                    "registros relacionados. Puedes desactivarlo.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error eliminando servicio");

                TempData["ErrorServicios"] =
                    "No se pudo eliminar: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // AGREGAR MEMBRESÍA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMembresia(
            FormMembresia datos)
        {
            try
            {
                ValidarMembresia(datos);

                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var tx =
                    await conexion.BeginTransactionAsync();

                using (var bloqueo = new NpgsqlCommand(
                    "SELECT pg_advisory_xact_lock(42174, 4)",
                    conexion, tx))
                {
                    await bloqueo.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO public.membresia
                    (
                        id_membresia,
                        nombre,
                        beneficios,
                        duracion,
                        precio_mensual,
                        activo
                    )
                    SELECT
                        COALESCE(MAX(id_membresia), 0) + 1,
                        @nombre,
                        @beneficios,
                        @duracion,
                        @precio,
                        @activo
                    FROM public.membresia",
                    conexion, tx))
                {
                    ParametrosMembresia(cmd, datos);

                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                TempData["MensajeServicios"] =
                    "Membresía agregada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error agregando membresía");

                TempData["ErrorServicios"] =
                    "No se pudo agregar: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // EDITAR MEMBRESÍA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMembresia(
            FormMembresia datos)
        {
            try
            {
                ValidarMembresia(datos);

                if (datos.IdMembresia <= 0)
                {
                    throw new Exception(
                        "Selecciona una membresía válida.");
                }

                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE public.membresia
                    SET
                        nombre = @nombre,
                        beneficios = @beneficios,
                        duracion = @duracion,
                        precio_mensual = @precio,
                        activo = @activo
                    WHERE id_membresia = @id",
                    conexion);

                ParametrosMembresia(cmd, datos);

                cmd.Parameters.AddWithValue(
                    "id", datos.IdMembresia);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "La membresía no existe.");
                }

                TempData["MensajeServicios"] =
                    "Membresía actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error editando membresía");

                TempData["ErrorServicios"] =
                    "No se pudo editar: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // ACTIVAR / DESACTIVAR MEMBRESÍA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoMembresia(
            int idMembresia,
            bool activar)
        {
            try
            {
                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE public.membresia
                    SET activo = @activo
                    WHERE id_membresia = @id",
                    conexion);

                cmd.Parameters.AddWithValue(
                    "activo", activar);

                cmd.Parameters.AddWithValue(
                    "id", idMembresia);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "La membresía no existe.");
                }

                TempData["MensajeServicios"] = activar
                    ? "Membresía activada correctamente."
                    : "Membresía desactivada; se conserva su historial.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error cambiando estado de membresía");

                TempData["ErrorServicios"] =
                    "No se pudo cambiar el estado: " + ex.Message;
            }

            return Volver();
        }

        // =========================================
        // ELIMINAR MEMBRESÍA
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMembresia(
            int idMembresia)
        {
            try
            {
                using var conexion = ConexionBD();

                await conexion.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    DELETE FROM public.membresia m

                    WHERE m.id_membresia = @id

                    AND NOT EXISTS (
                        SELECT 1
                        FROM public.identificador i
                        WHERE i.id_membresia = m.id_membresia
                    )

                    AND NOT EXISTS (
                        SELECT 1
                        FROM public.vw_admin_miembros_membresia v
                        WHERE v.""IdMembresia"" = m.id_membresia
                    )

                    RETURNING m.id_membresia",
                    conexion);

                cmd.Parameters.AddWithValue(
                    "id", idMembresia);

                if (await cmd.ExecuteScalarAsync() == null)
                {
                    throw new Exception(
                        "La membresía no existe o tiene clientes " +
                        "y registros relacionados. Puedes desactivarla.");
                }

                TempData["MensajeServicios"] =
                    "Membresía eliminada correctamente.";
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["ErrorServicios"] =
                    "La membresía tiene registros relacionados; " +
                    "puedes desactivarla sin borrar el historial.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error eliminando membresía");

                TempData["ErrorServicios"] =
                    "No se pudo eliminar: " + ex.Message;
            }

            return Volver();
        }
    }
}
