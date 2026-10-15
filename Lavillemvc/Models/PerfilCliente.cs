
using System;

namespace Lavillemvc.Models
{
    public class PerfilCliente
    {
        // =====================================
        // INFORMACIÓN PERSONAL
        // =====================================

        public int IdCliente { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string? Direccion { get; set; }

        public string? FotoPerfilUrl { get; set; }


        // =====================================
        // TIPO DE CLIENTE Y FIDELIDAD
        // =====================================

        public string? TipoCliente { get; set; }

        public int PuntosFidelidad { get; set; }

        public int TotalPedidos { get; set; }


        // =====================================
        // MEMBRESÍA
        // =====================================

        public string? Membresia { get; set; }

        public string? BeneficiosMembresia { get; set; }

        public DateTime? InicioMembresia { get; set; }

        public DateTime? FinMembresia { get; set; }

        public DateTime? MiembroDesde { get; set; }


        // =====================================
        // PREFERENCIAS DE BELLEZA
        // =====================================

        public string? TipoCabello { get; set; }

        public int? IdEspecialistaPreferido { get; set; }

        public string? EspecialistaPreferido { get; set; }

        public int? IdSucursalFavorita { get; set; }

        public string? SucursalFavorita { get; set; }

        public string? AlergiasCondiciones { get; set; }


        // =====================================
        // PREFERENCIAS DE CONTACTO
        // =====================================

        public bool NotificacionesCorreo { get; set; } = true;

        public bool RecordatoriosSms { get; set; } = true;

        public bool PromocionesEspeciales { get; set; } = false;

        public bool BoletinMensual { get; set; } = false;


        // =====================================
        // CONFIGURACIÓN DE LA CUENTA
        // =====================================

        public string Idioma { get; set; } = "es";
    }
}
