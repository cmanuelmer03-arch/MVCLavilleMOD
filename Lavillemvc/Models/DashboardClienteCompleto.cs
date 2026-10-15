
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class DashboardClienteCompleto
    {
        // DATOS DEL CLIENTE
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? TipoCliente { get; set; }
        public int PuntosFidelidad { get; set; }
        public int TotalPedidos { get; set; }

        // MEMBRESÍA
        public string? Membresia { get; set; }
        public string? BeneficiosMembresia { get; set; }
        public DateTime? InicioMembresia { get; set; }
        public DateTime? FinMembresia { get; set; }

        // PRÓXIMA CITA
        public int? IdProximaCita { get; set; }
        public string? ProximoServicio { get; set; }
        public DateTime? FechaProximaCita { get; set; }
        public TimeSpan? HoraProximaCita { get; set; }
        public string? EstadoProximaCita { get; set; }
        public string? EspecialistaProximaCita { get; set; }
        public string? SucursalProximaCita { get; set; }

        // RESUMEN DE CITAS
        public int CitasConfirmadas { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasCanceladasMes { get; set; }

        // DATOS JSON
        public string HistorialJson { get; set; } = "[]";
        public string PromocionesJson { get; set; } = "[]";

        public List<HistorialItem> HistorialReciente =>
            JsonSerializer.Deserialize<List<HistorialItem>>(
                HistorialJson ?? "[]"
            ) ?? new List<HistorialItem>();

        public List<PromocionItem> Promociones =>
            JsonSerializer.Deserialize<List<PromocionItem>>(
                PromocionesJson ?? "[]"
            ) ?? new List<PromocionItem>();

        // HISTORIAL
        public class HistorialItem
        {
            public int IdCita { get; set; }
            public string Servicio { get; set; } = "";
            public string? Especialista { get; set; }
            public DateTime FechaCita { get; set; }
            public string HoraCita { get; set; } = "";
            public string? Estado { get; set; }
            public decimal PrecioServicio { get; set; }
            public decimal? TotalPagado { get; set; }
        }

        // PROMOCIONES
        public class PromocionItem
        {
            public int IdPromocion { get; set; }
            public string NombrePromocion { get; set; } = "";
            public string? Descripcion { get; set; }
            public decimal? Descuento { get; set; }
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public string Sucursal { get; set; } = "";
        }
    }
}
