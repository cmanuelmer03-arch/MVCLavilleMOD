
using System;
using System.Threading.Tasks;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Lavillemvc.DAL
{
    public class CuentaRepositorio
    {
        private readonly Conexion _conexion;

        private readonly PasswordHasher<CuentaUsuario>
            _passwordHasher = new();

        public CuentaRepositorio(Conexion conexion)
        {
            _conexion = conexion;
        }

        // ==========================================
        // 1. BUSCAR USUARIO POR CORREO
        // ==========================================

        public async Task<CuentaUsuario?> BuscarPorCorreoAsync(
            string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            correo = correo.Trim().ToLowerInvariant();

            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();

            const string sql = @"
                SELECT
                    'cliente' AS tipo_cuenta,
                    id_cliente AS id_usuario,
                    nombre,
                    apellido,
                    correo,
                    ""contraseña"" AS password_hash,
                    'Cliente' AS cargo,
                    NULL::integer AS id_sucursal
                FROM public.cliente
                WHERE LOWER(TRIM(correo)) = @correo

                UNION ALL

                SELECT
                    'empleado' AS tipo_cuenta,
                    id_empleado AS id_usuario,
                    nombre,
                    apellido,
                    correo,
                    ""contraseña"" AS password_hash,
                    cargo,
                    id_sucursal
                FROM public.empleado
                WHERE LOWER(TRIM(correo)) = @correo

                LIMIT 2;
            ";

            await using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("correo", correo);

            await using var lector =
                await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync())
                return null;

            var cuenta = new CuentaUsuario
            {
                TipoCuenta = lector.GetString(0),

                IdUsuario = lector.GetInt32(1),

                Nombre = lector.IsDBNull(2)
                    ? ""
                    : lector.GetString(2),

                Apellido = lector.IsDBNull(3)
                    ? ""
                    : lector.GetString(3),

                Correo = lector.IsDBNull(4)
                    ? ""
                    : lector.GetString(4),

                PasswordHash = lector.IsDBNull(5)
                    ? ""
                    : lector.GetString(5),

                Cargo = lector.IsDBNull(6)
                    ? ""
                    : lector.GetString(6),

                IdSucursal = lector.IsDBNull(7)
                    ? null
                    : lector.GetInt32(7)
            };

            // Rechazar correos duplicados
            if (await lector.ReadAsync())
                return null;

            cuenta.Rol = cuenta.TipoCuenta == "cliente"
                ? "Cliente"
                : cuenta.Cargo?.Trim() ?? "";

            return cuenta;
        }

        // ==========================================
        // 2. REGISTRAR NUEVO CLIENTE
        // ==========================================

        public async Task<(bool Exito, string Mensaje)>
            RegistrarClienteAsync(
                string nombreCompleto,
                string correo,
                string? telefono,
                string contraseña)
        {
            nombreCompleto = nombreCompleto?.Trim() ?? "";
            correo = correo?.Trim().ToLowerInvariant() ?? "";

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombreCompleto) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contraseña))
            {
                return (
                    false,
                    "Completa todos los campos obligatorios."
                );
            }

            if (nombreCompleto.Length > 201 ||
                correo.Length > 150 ||
                (telefono?.Length ?? 0) > 20)
            {
                return (
                    false,
                    "Uno de los campos supera la longitud permitida."
                );
            }

            if (contraseña.Length < 8)
            {
                return (
                    false,
                    "La contraseña debe tener al menos 8 caracteres."
                );
            }

            // Separar nombre completo en nombre y apellido
            string[] partes = nombreCompleto.Split(
                ' ',
                2,
                StringSplitOptions.RemoveEmptyEntries
            );

            string nombre = partes[0];

            string apellido = partes.Length > 1
                ? partes[1]
                : "";

            if (nombre.Length > 100 || apellido.Length > 100)
            {
                return (
                    false,
                    "El nombre o apellido es demasiado largo."
                );
            }

            await using var conexion = _conexion.GetConnection();
            await conexion.OpenAsync();

            // ==========================================
            // COMPROBAR SI EL CORREO YA EXISTE
            // ==========================================

            const string sqlExiste = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM public.cliente
                    WHERE LOWER(TRIM(correo)) = @correo

                    UNION ALL

                    SELECT 1
                    FROM public.empleado
                    WHERE LOWER(TRIM(correo)) = @correo
                );
            ";

            await using (var comando =
                new NpgsqlCommand(sqlExiste, conexion))
            {
                comando.Parameters.AddWithValue(
                    "correo",
                    correo
                );

                bool existe = (bool)(
                    await comando.ExecuteScalarAsync() ?? false
                );

                if (existe)
                {
                    return (
                        false,
                        "Este correo ya está registrado."
                    );
                }
            }

            // ==========================================
            // GENERAR HASH SEGURO DE CONTRASEÑA
            // ==========================================

            var cuenta = new CuentaUsuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                TipoCuenta = "cliente",
                Rol = "Cliente"
            };

            string passwordHash =
                _passwordHasher.HashPassword(
                    cuenta,
                    contraseña
                );

            // ==========================================
            // INSERTAR CLIENTE EN SUPABASE
            // ==========================================

            const string sqlInsertar = @"
                INSERT INTO public.cliente
                (
                    nombre,
                    apellido,
                    correo,
                    telefono,
                    ""contraseña""
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @correo,
                    @telefono,
                    @passwordHash
                );
            ";

            await using (var comando =
                new NpgsqlCommand(sqlInsertar, conexion))
            {
                comando.Parameters.AddWithValue(
                    "nombre",
                    nombre
                );

                comando.Parameters.AddWithValue(
                    "apellido",
                    apellido
                );

                comando.Parameters.AddWithValue(
                    "correo",
                    correo
                );

                comando.Parameters.AddWithValue(
                    "telefono",
                    (object?)telefono ?? DBNull.Value
                );

                comando.Parameters.AddWithValue(
                    "passwordHash",
                    passwordHash
                );

                await comando.ExecuteNonQueryAsync();
            }

            return (
                true,
                "Cuenta creada correctamente."
            );
        }
    }
}
