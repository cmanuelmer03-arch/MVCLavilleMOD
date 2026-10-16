
using System;

namespace Lavillemvc.Models
{
    public class RecepcionistaGestionClientes
    {
        // =====================================
        // IDENTIFICACIÓN DEL CLIENTE
        // =====================================

        public int IdCliente { get; set; }

        public string Nombre { get; set; }
            = string.Empty;

        public string Apellido { get; set; }
            = string.Empty;

        public string NombreCompleto { get; set; }
            = string.Empty;


        // =====================================
        // DATOS DE CONTACTO
        // =====================================

        public string? Correo { get; set; }

        public string? Telefono { get; set; }


        // =====================================
        // TIPO DE CLIENTE Y FIDELIDAD
        // =====================================

        public string? TipoCliente { get; set; }

        public int PuntosFidelidad { get; set; }

        public int TotalPedidos { get; set; }


        // =====================================
        // MEMBRESÍA VIGENTE
        // =====================================

        public int? IdMembresiaVigente { get; set; }

        public string? MembresiaVigente { get; set; }

        public DateTime? FechaFinMembresia
        { get; set; }


        // =====================================
        // ESTADO Y REGISTRO
        // =====================================

        public string? EstadoCuenta { get; set; }

        public DateTime? FechaRegistro { get; set; }


        // =====================================
        // HISTORIAL DE CITAS
        // =====================================

        public int CantidadCitas { get; set; }

        public DateOnly? UltimaVisita { get; set; }

        public DateOnly? UltimaCitaRegistrada
        { get; set; }


        // =====================================
        // FILTROS
        // =====================================

        public bool EsGoldPlatinum { get; set; }

        public bool? EsNuevoEsteMes { get; set; }


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int TotalClientes { get; set; }

        public int ClientesGoldPlatinum { get; set; }

        public int? NuevosEsteMes { get; set; }

        public int? ClientesActivos { get; set; }

        public int? ClientesInactivos { get; set; }
    }
}
