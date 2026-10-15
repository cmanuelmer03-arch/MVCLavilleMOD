
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Lavillemvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IConfiguration config,
            ILogger<AdminController> logger)
        {
            _config = config;
            _logger = logger;
        }

        private NpgsqlConnection ConexionBD()
        {
            return new Conexion(_config).GetConnection();
        }

        private static string Texto(
            NpgsqlDataReader r, string c, string defecto = "")
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? defecto
                : Convert.ToString(r.GetValue(i)) ?? defecto;
        }

        private static int Numero(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? 0
                : Convert.ToInt32(r.GetValue(i));
        }

        private static int? NumeroN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? null
                : Convert.ToInt32(r.GetValue(i));
        }

        private static decimal Dinero(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? 0
                : Convert.ToDecimal(r.GetValue(i));
        }

        private static DateTime Fecha(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? DateTime.MinValue
                : r.GetDateTime(i);
        }

        private static DateTime? FechaN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? null
                : r.GetDateTime(i);
        }

        private static bool? BoolN(NpgsqlDataReader r, string c)
        {
            int i = r.GetOrdinal(c);
            return r.IsDBNull(i)
                ? null
                : r.GetBoolean(i);
        }

        private static void SepararNombre(
            string completo,
            out string nombre,
            out string apellido)
        {
            completo = (completo ?? "").Trim();
            int posicion = completo.LastIndexOf(' ');

            if (posicion < 0)
            {
                nombre = completo;
                apellido = completo;
            }
            else
            {
                nombre = completo.Substring(0, posicion).Trim();
                apellido = completo.Substring(posicion + 1).Trim();
            }
        }

        public class FormUsuario
        {
            public string IdUsuario { get; set; } = "";
            public string TipoOrigen { get; set; } = "";
            public string NombreCompleto { get; set; } = "";
            public string? Correo { get; set; }
            public string Rol { get; set; } = "";
            public int? IdSucursal { get; set; }
            public int? IdEspecialidad { get; set; }
            public bool Disponibilidad { get; set; } = true;
        }

        // ================================
        // DASHBOARD
        // ================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var modelo = new AdminDashboardGeneral();

            try
            {
                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    SELECT *
                    FROM public.vw_dashboard_administrador_general
                    LIMIT 1";

                using var comando = new NpgsqlCommand(sql, conexion);
                using var r = await comando.ExecuteReaderAsync();

                if (await r.ReadAsync())
                {
                    modelo.FechaReferencia =
                        Fecha(r, "FechaReferencia");

                    modelo.ReservasTotales =
                        Numero(r, "ReservasTotales");

                    modelo.UsuariosRegistrados =
                        Numero(r, "UsuariosRegistrados");

                    modelo.SucursalesActivas =
                        Numero(r, "SucursalesActivas");

                    modelo.ServiciosDisponibles =
                        Numero(r, "ServiciosDisponibles");

                    modelo.PromocionesActivas =
                        Numero(r, "PromocionesActivas");

                    modelo.IngresosMes =
                        Dinero(r, "IngresosMes");

                    modelo.ReservasMes =
                        Numero(r, "ReservasMes");

                    modelo.ReservasConfirmadasMes =
                        Numero(r, "ReservasConfirmadasMes");

                    modelo.ReservasCompletadasMes =
                        Numero(r, "ReservasCompletadasMes");

                    modelo.ReservasCanceladasMes =
                        Numero(r, "ReservasCanceladasMes");

                    modelo.ReservasPendientesMes =
                        Numero(r, "ReservasPendientesMes");

                    modelo.ReservasOtrosMes =
                        Numero(r, "ReservasOtrosMes");

                    modelo.ClientesNuevosMes =
                        Numero(r, "ClientesNuevosMes");

                    modelo.TasaRepeticionClientes =
                        Dinero(r, "TasaRepeticionClientes");

                    modelo.ServicioMasPopular =
                        Texto(r, "ServicioMasPopular");

                    modelo.SucursalTop =
                        Texto(r, "SucursalTop");

                    modelo.RendimientoSucursalesJson =
                        Texto(r, "RendimientoSucursalesJson", "[]");

                    modelo.ReservasPorEstadoJson =
                        Texto(r, "ReservasPorEstadoJson", "[]");

                    modelo.ActividadRecienteJson =
                        Texto(r, "ActividadRecienteJson", "[]");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Dashboard");

                ViewBag.ErrorDashboard =
                    "No se pudo cargar el Dashboard: " + ex.Message;
            }

            return View(modelo);
        }

        // ================================
        // GESTIÓN DE USUARIOS
        // ================================

        [HttpGet]
        public async Task<IActionResult> GestionUsuarios()
        {
            var usuarios = new List<AdminGestionUsuarios>();
            var sucursales = new List<(int Id, string Nombre)>();
            var especialidades = new List<(int Id, string Nombre)>();

            try
            {
                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    SELECT *
                    FROM public.vw_gestion_usuarios
                    ORDER BY ""NombreCompleto""";

                using (var cmd = new NpgsqlCommand(sql, conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        usuarios.Add(new AdminGestionUsuarios
                        {
                            IdUsuario =
                                Texto(r, "IdUsuario"),

                            IdRegistroOrigen =
                                Numero(r, "IdRegistroOrigen"),

                            TipoOrigen =
                                Texto(r, "TipoOrigen"),

                            NombreCompleto =
                                Texto(r, "NombreCompleto"),

                            Correo =
                                Texto(r, "Correo"),

                            Rol =
                                Texto(r, "Rol"),

                            IdSucursal =
                                NumeroN(r, "IdSucursal"),

                            Sucursal =
                                Texto(r, "Sucursal"),

                            Especialidad =
                                Texto(r, "Especialidad"),

                            DisponibilidadEspecialista =
                                BoolN(r, "DisponibilidadEspecialista"),

                            IdEmpleadoRelacionado =
                                NumeroN(r, "IdEmpleadoRelacionado"),

                            EstadoCuenta =
                                Texto(r, "EstadoCuenta"),

                            UltimoAcceso =
                                FechaN(r, "UltimoAcceso"),

                            TotalUsuarios =
                                Numero(r, "TotalUsuarios"),

                            TotalClientes =
                                Numero(r, "TotalClientes"),

                            TotalEspecialistas =
                                Numero(r, "TotalEspecialistas"),

                            TotalRecepcionistas =
                                Numero(r, "TotalRecepcionistas"),

                            TotalGerentes =
                                Numero(r, "TotalGerentes"),

                            TotalInventario =
                                Numero(r, "TotalInventario"),

                            TotalAdministradores =
                                Numero(r, "TotalAdministradores")
                        });
                    }
                }

                using (var cmd = new NpgsqlCommand(
                    @"SELECT id_sucursal, nombre_sucursal
                      FROM public.sucursal
                      ORDER BY nombre_sucursal",
                    conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        sucursales.Add(
                            (r.GetInt32(0), r.GetString(1)));
                    }
                }

                using (var cmd = new NpgsqlCommand(
                    @"SELECT id_especialidad, nombre_especialidad
                      FROM public.especialidad
                      ORDER BY nombre_especialidad",
                    conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        especialidades.Add(
                            (r.GetInt32(0), r.GetString(1)));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Gestión Usuarios");

                ViewBag.ErrorUsuarios =
                    "No se pudieron cargar los usuarios: " + ex.Message;
            }

            ViewBag.Sucursales = sucursales;
            ViewBag.Especialidades = especialidades;

            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(
            FormUsuario datos)
        {
            try
            {
                SepararNombre(
                    datos.NombreCompleto,
                    out string nombre,
                    out string apellido);

                if (string.IsNullOrWhiteSpace(nombre))
                    throw new Exception("Escribe el nombre.");

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                using var tx =
                    await conexion.BeginTransactionAsync();

                using (var bloqueo = new NpgsqlCommand(
                    "SELECT pg_advisory_xact_lock(42174, 1)",
                    conexion, tx))
                {
                    await bloqueo.ExecuteNonQueryAsync();
                }

                string sql;

                switch (datos.Rol)
                {
                    case "Cliente":
                        sql = @"
                            INSERT INTO public.cliente
                            (id_cliente, nombre, apellido, correo)
                            VALUES
                            ((SELECT COALESCE(MAX(id_cliente), 0) + 1
                              FROM public.cliente),
                             @nombre, @apellido, @correo)";
                        break;

                    case "Especialista":
                        if (!datos.IdEspecialidad.HasValue)
                            throw new Exception(
                                "Selecciona una especialidad.");

                        sql = @"
                            INSERT INTO public.especialista
                            (id_especialista, id_especialidad,
                             nombre, disponibilidad)
                            VALUES
                            ((SELECT COALESCE(MAX(id_especialista), 0) + 1
                              FROM public.especialista),
                             @especialidad, @completo, @disponibilidad)";
                        break;

                    case "Recepcionista":
                    case "Gerente":
                    case "Inventario":
                    case "Administrador":
                        if (!datos.IdSucursal.HasValue)
                            throw new Exception(
                                "Selecciona una sucursal.");

                        sql = @"
                            INSERT INTO public.empleado
                            (id_empleado, id_sucursal,
                             nombre, apellido, cargo, correo)
                            VALUES
                            ((SELECT COALESCE(MAX(id_empleado), 0) + 1
                              FROM public.empleado),
                             @sucursal, @nombre, @apellido,
                             @rol, @correo)";
                        break;

                    default:
                        throw new Exception("Rol no reconocido.");
                }

                using (var cmd = new NpgsqlCommand(
                    sql, conexion, tx))
                {
                    AgregarParametros(
                        cmd, datos, nombre, apellido);

                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                TempData["MensajeUsuarios"] =
                    "Usuario creado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");

                TempData["ErrorUsuarios"] =
                    "No se pudo crear: " + ex.Message;
            }

            return RedirectToAction(nameof(GestionUsuarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(
            FormUsuario datos)
        {
            try
            {
                var partes = datos.IdUsuario.Split(':');

                if (partes.Length != 2 ||
                    !int.TryParse(partes[1], out int id))
                {
                    throw new Exception(
                        "Identificador incorrecto.");
                }

                SepararNombre(
                    datos.NombreCompleto,
                    out string nombre,
                    out string apellido);

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                string sql = partes[0] switch
                {
                    "cliente" => @"
                        UPDATE public.cliente
                        SET nombre = @nombre,
                            apellido = @apellido,
                            correo = @correo
                        WHERE id_cliente = @id",

                    "empleado" => @"
                        UPDATE public.empleado
                        SET nombre = @nombre,
                            apellido = @apellido,
                            correo = @correo,
                            cargo = @rol,
                            id_sucursal = @sucursal
                        WHERE id_empleado = @id",

                    "especialista" => @"
                        UPDATE public.especialista
                        SET nombre = @completo,
                            disponibilidad = @disponibilidad,
                            id_especialidad =
                                COALESCE(@especialidad, id_especialidad)
                        WHERE id_especialista = @id",

                    _ => throw new Exception(
                        "Tipo de usuario no reconocido.")
                };

                if (partes[0] == "empleado" &&
                    !datos.IdSucursal.HasValue)
                {
                    throw new Exception(
                        "Selecciona una sucursal.");
                }

                using var cmd =
                    new NpgsqlCommand(sql, conexion);

                AgregarParametros(
                    cmd, datos, nombre, apellido);

                cmd.Parameters.AddWithValue("id", id);

                int afectados =
                    await cmd.ExecuteNonQueryAsync();

                if (afectados == 0)
                    throw new Exception(
                        "Usuario no encontrado.");

                TempData["MensajeUsuarios"] =
                    "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando usuario");

                TempData["ErrorUsuarios"] =
                    "No se pudo editar: " + ex.Message;
            }

            return RedirectToAction(nameof(GestionUsuarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(
            string idUsuario)
        {
            try
            {
                var partes = (idUsuario ?? "").Split(':');

                if (partes.Length != 2 ||
                    !int.TryParse(partes[1], out int id))
                {
                    throw new Exception(
                        "Identificador incorrecto.");
                }

                string sql = partes[0] switch
                {
                    "cliente" =>
                        "DELETE FROM public.cliente WHERE id_cliente = @id",

                    "empleado" =>
                        "DELETE FROM public.empleado WHERE id_empleado = @id",

                    "especialista" =>
                        "DELETE FROM public.especialista WHERE id_especialista = @id",

                    _ => throw new Exception(
                        "Tipo de usuario no reconocido.")
                };

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                using var cmd =
                    new NpgsqlCommand(sql, conexion);

                cmd.Parameters.AddWithValue("id", id);

                int afectados =
                    await cmd.ExecuteNonQueryAsync();

                if (afectados == 0)
                    throw new Exception(
                        "Usuario no encontrado.");

                TempData["MensajeUsuarios"] =
                    "Usuario eliminado correctamente.";
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["ErrorUsuarios"] =
                    "No se puede eliminar porque este usuario " +
                    "tiene registros relacionados en la base de datos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario");

                TempData["ErrorUsuarios"] =
                    "No se pudo eliminar: " + ex.Message;
            }

            return RedirectToAction(nameof(GestionUsuarios));
        }

        private static void AgregarParametros(
            NpgsqlCommand cmd,
            FormUsuario datos,
            string nombre,
            string apellido)
        {
            cmd.Parameters.AddWithValue("nombre", nombre);
            cmd.Parameters.AddWithValue("apellido", apellido);

            cmd.Parameters.AddWithValue(
                "completo", datos.NombreCompleto.Trim());

            cmd.Parameters.AddWithValue(
                "correo",
                (object?)datos.Correo ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "rol", datos.Rol ?? "");

            cmd.Parameters.AddWithValue(
                "sucursal",
                datos.IdSucursal.HasValue
                    ? datos.IdSucursal.Value
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "especialidad",
                datos.IdEspecialidad.HasValue
                    ? datos.IdEspecialidad.Value
                    : DBNull.Value);

            cmd.Parameters.AddWithValue(
                "disponibilidad",
                datos.Disponibilidad);
        }

        // ================================
        // GESTIÓN DE SUCURSALES
        // ================================

        public class FormSucursal
        {
            public int IdSucursal { get; set; }
            public string NombreSucursal { get; set; } = "";
            public string Direccion { get; set; } = "";
            public string? Telefono { get; set; }
            public TimeSpan? HorarioApertura { get; set; }
            public TimeSpan? HorarioCierre { get; set; }
            public bool Abierto { get; set; } = true;
        }

        public class EmpleadoSucursalOpcion
        {
            public int IdEmpleado { get; set; }
            public int IdSucursal { get; set; }
            public string Nombre { get; set; } = "";
            public string Cargo { get; set; } = "";
        }

        [HttpGet]
        public async Task<IActionResult> GestionSucursales()
        {
            var sucursales =
                new List<AdminGestionSucursales>();

            var empleados =
                new List<EmpleadoSucursalOpcion>();

            try
            {
                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    SELECT *
                    FROM public.vw_admin_gestion_sucursales
                    ORDER BY ""IdSucursal""";

                using (var cmd =
                    new NpgsqlCommand(sql, conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        int ia =
                            r.GetOrdinal("HorarioApertura");

                        int ic =
                            r.GetOrdinal("HorarioCierre");

                        sucursales.Add(
                            new AdminGestionSucursales
                            {
                                IdSucursal =
                                    Numero(r, "IdSucursal"),

                                NombreSucursal =
                                    Texto(r, "NombreSucursal"),

                                Direccion =
                                    Texto(r, "Direccion"),

                                Telefono =
                                    Texto(r, "Telefono"),

                                HorarioApertura =
                                    r.IsDBNull(ia)
                                        ? null
                                        : r.GetTimeSpan(ia),

                                HorarioCierre =
                                    r.IsDBNull(ic)
                                        ? null
                                        : r.GetTimeSpan(ic),

                                Abierto =
                                    BoolN(r, "Abierto"),

                                EstadoSucursal =
                                    Texto(r, "EstadoSucursal"),

                                Gerente =
                                    Texto(r, "Gerente"),

                                CantidadEmpleados =
                                    Numero(r, "CantidadEmpleados"),

                                ReservasMes =
                                    Numero(r, "ReservasMes"),

                                IngresosMes =
                                    Dinero(r, "IngresosMes"),

                                TotalSucursales =
                                    Numero(r, "TotalSucursales"),

                                SucursalesAbiertas =
                                    Numero(r, "SucursalesAbiertas"),

                                SucursalesCerradas =
                                    Numero(r, "SucursalesCerradas"),

                                TotalEmpleados =
                                    Numero(r, "TotalEmpleados")
                            });
                    }
                }

                const string sqlEmpleados = @"
                    SELECT
                        id_empleado,
                        id_sucursal,
                        CONCAT_WS(' ', nombre, apellido) AS nombre,
                        COALESCE(cargo, 'Sin cargo') AS cargo
                    FROM public.empleado
                    ORDER BY nombre, apellido, id_empleado";

                using (var cmd =
                    new NpgsqlCommand(sqlEmpleados, conexion))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        empleados.Add(
                            new EmpleadoSucursalOpcion
                            {
                                IdEmpleado = r.GetInt32(0),
                                IdSucursal = r.GetInt32(1),
                                Nombre = r.GetString(2),
                                Cargo = r.GetString(3)
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error cargando sucursales");

                ViewBag.ErrorSucursales =
                    "No se pudieron cargar las sucursales: " +
                    ex.Message;
            }

            ViewBag.EmpleadosSucursales = empleados;

            return View(sucursales);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarSucursal(
            FormSucursal datos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datos.NombreSucursal) ||
                    string.IsNullOrWhiteSpace(datos.Direccion))
                {
                    throw new Exception(
                        "Nombre y dirección son obligatorios.");
                }

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                using var tx =
                    await conexion.BeginTransactionAsync();

                using (var lockCmd = new NpgsqlCommand(
                    "SELECT pg_advisory_xact_lock(42174, 2)",
                    conexion, tx))
                {
                    await lockCmd.ExecuteNonQueryAsync();
                }

                const string sql = @"
                    INSERT INTO public.sucursal
                    (
                        id_sucursal,
                        nombre_sucursal,
                        direccion,
                        telefono,
                        horario_apertura,
                        horario_cierre,
                        abierto
                    )
                    SELECT
                        COALESCE(MAX(id_sucursal), 0) + 1,
                        @nombre,
                        @direccion,
                        @telefono,
                        @apertura,
                        @cierre,
                        @abierto
                    FROM public.sucursal";

                using (var cmd =
                    new NpgsqlCommand(sql, conexion, tx))
                {
                    ParametrosSucursal(cmd, datos);

                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                TempData["MensajeSucursales"] =
                    "Sucursal agregada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error agregando sucursal");

                TempData["ErrorSucursales"] =
                    "No se pudo agregar la sucursal: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(GestionSucursales));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSucursal(
            FormSucursal datos)
        {
            try
            {
                if (datos.IdSucursal <= 0 ||
                    string.IsNullOrWhiteSpace(datos.NombreSucursal) ||
                    string.IsNullOrWhiteSpace(datos.Direccion))
                {
                    throw new Exception(
                        "Verifica el nombre, la dirección y la sucursal.");
                }

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    UPDATE public.sucursal
                    SET
                        nombre_sucursal = @nombre,
                        direccion = @direccion,
                        telefono = @telefono,
                        horario_apertura = @apertura,
                        horario_cierre = @cierre,
                        abierto = @abierto
                    WHERE id_sucursal = @id";

                using var cmd =
                    new NpgsqlCommand(sql, conexion);

                ParametrosSucursal(cmd, datos);

                cmd.Parameters.AddWithValue(
                    "id", datos.IdSucursal);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "No se encontró la sucursal.");
                }

                TempData["MensajeSucursales"] =
                    "Sucursal actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error editando sucursal");

                TempData["ErrorSucursales"] =
                    "No se pudo editar la sucursal: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(GestionSucursales));
        }

        // ================================
        // NUEVO: ELIMINAR SUCURSAL
        // ================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSucursal(
            int idSucursal)
        {
            try
            {
                if (idSucursal <= 0)
                {
                    throw new Exception(
                        "La sucursal seleccionada no es válida.");
                }

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    DELETE FROM public.sucursal s
                    WHERE s.id_sucursal = @id

                    AND NOT EXISTS (
                        SELECT 1
                        FROM public.empleado e
                        WHERE e.id_sucursal = s.id_sucursal
                    )

                    AND NOT EXISTS (
                        SELECT 1
                        FROM public.vw_mis_reservas_cliente r
                        WHERE r.""IdSucursal"" = s.id_sucursal
                    )

                    RETURNING s.id_sucursal";

                using var cmd =
                    new NpgsqlCommand(sql, conexion);

                cmd.Parameters.AddWithValue(
                    "id", idSucursal);

                var resultado =
                    await cmd.ExecuteScalarAsync();

                if (resultado == null)
                {
                    using var consulta = new NpgsqlCommand(
                        @"SELECT EXISTS (
                            SELECT 1
                            FROM public.sucursal
                            WHERE id_sucursal = @id
                        )",
                        conexion);

                    consulta.Parameters.AddWithValue(
                        "id", idSucursal);

                    bool existe = (bool)(
                        await consulta.ExecuteScalarAsync()
                        ?? false);

                    throw new Exception(
                        existe
                            ? "No se puede eliminar esta sucursal porque tiene empleados o reservas relacionados."
                            : "La sucursal ya no existe.");
                }

                TempData["MensajeSucursales"] =
                    "Sucursal eliminada correctamente.";
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["ErrorSucursales"] =
                    "No se puede eliminar la sucursal porque " +
                    "tiene registros relacionados. " +
                    "No se borró ningún dato.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error eliminando sucursal");

                TempData["ErrorSucursales"] =
                    "No se pudo eliminar la sucursal: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(GestionSucursales));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarEmpleadoSucursal(
            int idEmpleado,
            int idSucursal)
        {
            try
            {
                if (idEmpleado <= 0 || idSucursal <= 0)
                {
                    throw new Exception(
                        "Selecciona un empleado y una sucursal.");
                }

                using var conexion = ConexionBD();
                await conexion.OpenAsync();

                const string sql = @"
                    UPDATE public.empleado
                    SET id_sucursal = @sucursal
                    WHERE id_empleado = @empleado
                    AND EXISTS (
                        SELECT 1
                        FROM public.sucursal
                        WHERE id_sucursal = @sucursal
                    )";

                using var cmd =
                    new NpgsqlCommand(sql, conexion);

                cmd.Parameters.AddWithValue(
                    "sucursal", idSucursal);

                cmd.Parameters.AddWithValue(
                    "empleado", idEmpleado);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    throw new Exception(
                        "No se encontró el empleado o la sucursal.");
                }

                TempData["MensajeSucursales"] =
                    "Empleado asignado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error asignando empleado");

                TempData["ErrorSucursales"] =
                    "No se pudo asignar el empleado: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(GestionSucursales));
        }

        private static void ParametrosSucursal(
            NpgsqlCommand cmd,
            FormSucursal datos)
        {
            cmd.Parameters.AddWithValue(
                "nombre",
                datos.NombreSucursal.Trim());

            cmd.Parameters.AddWithValue(
                "direccion",
                datos.Direccion.Trim());

            cmd.Parameters.AddWithValue(
                "telefono",
                string.IsNullOrWhiteSpace(datos.Telefono)
                    ? (object)DBNull.Value
                    : datos.Telefono.Trim());

            cmd.Parameters.Add(
                "apertura",
                NpgsqlTypes.NpgsqlDbType.Time).Value =
                datos.HorarioApertura.HasValue
                    ? (object)datos.HorarioApertura.Value
                    : DBNull.Value;

            cmd.Parameters.Add(
                "cierre",
                NpgsqlTypes.NpgsqlDbType.Time).Value =
                datos.HorarioCierre.HasValue
                    ? (object)datos.HorarioCierre.Value
                    : DBNull.Value;

            cmd.Parameters.AddWithValue(
                "abierto", datos.Abierto);
        }

        // ================================
        // SERVICIOS Y REPORTES
        // ================================

        public IActionResult GestionServicios()
        {
            return View();
        }

        public IActionResult ReportesGenerales()
        {
            return View();
        }
    }
}
