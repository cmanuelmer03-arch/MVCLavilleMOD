
namespace Lavillemvc.Models
{
    public class CuentaUsuario
    {
        // Identificador de la cuenta
        public int IdUsuario { get; set; }

        // Tabla de origen: cliente o empleado
        public string TipoCuenta { get; set; } = "";

        // Información personal
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";

        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {Apellido}".Trim();
            }
        }

        // Datos de acceso
        public string Correo { get; set; } = "";

        public int? IdEspecialista { get; set; }

        // Hash de contraseña almacenado en Supabase
        public string PasswordHash { get; set; } = "";

        // Rol asignado al usuario
        public string Rol { get; set; } = "";

        // Cargo del empleado, si corresponde
        public string? Cargo { get; set; }

        // Identificador de sucursal
        public int? IdSucursal { get; set; }
    }
}
