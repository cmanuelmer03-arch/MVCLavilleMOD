
using System;

namespace Lavillemvc.Models
{
    public class AdminGestionUsuarios
    {
        // =====================================
        // IDENTIFICACIÓN
        // =====================================

        public string IdUsuario { get; set; }
            = string.Empty;

        public int IdRegistroOrigen { get; set; }

        public string TipoOrigen { get; set; }
            = string.Empty;

        // =====================================
        // DATOS DEL USUARIO
        // =====================================

        public string NombreCompleto { get; set; }
            = string.Empty;

        public string? Correo { get; set; }

        public string Rol { get; set; }
            = string.Empty;

        public int? IdSucursal { get; set; }

        public string? Sucursal { get; set; }

        public string? Especialidad { get; set; }

        public bool? DisponibilidadEspecialista
        { get; set; }

        public int? IdEmpleadoRelacionado { get; set; }

        // =====================================
        // ESTADO Y ACCESO
        // =====================================

        public string? EstadoCuenta { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        // =====================================
        // ESTADÍSTICAS GENERALES
        // =====================================

        public int TotalUsuarios { get; set; }

        public int TotalClientes { get; set; }

        public int TotalEspecialistas { get; set; }

        public int TotalRecepcionistas { get; set; }

        public int TotalGerentes { get; set; }

        public int TotalInventario { get; set; }

        public int TotalAdministradores { get; set; }
    }
}
