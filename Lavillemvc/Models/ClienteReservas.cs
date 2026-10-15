
using System;

namespace Lavillemvc.Models
{
    public class ClienteReservas
    {
        // =====================================
        // IDENTIFICADORES
        // =====================================

        public int IdCita { get; set; }

        public int IdCliente { get; set; }

        public int IdServicio { get; set; }

        public int? IdEmpleado { get; set; }


        // =====================================
        // INFORMACIÓN DEL CLIENTE
        // =====================================

        public string NombreCliente { get; set; }
            = string.Empty;


        // =====================================
        // INFORMACIÓN DEL SERVICIO
        // =====================================

        public string Servicio { get; set; }
            = string.Empty;

        public decimal CostoEstimado { get; set; }


        // =====================================
        // ESPECIALISTA ASIGNADO
        // =====================================

        public int? IdEspecialista { get; set; }

        public string? Especialista { get; set; }


        // =====================================
        // SUCURSAL
        // =====================================

        public int? IdSucursal { get; set; }

        public string? Sucursal { get; set; }

        public string? DireccionSucursal { get; set; }


        // =====================================
        // FECHA Y HORA DE LA RESERVA
        // =====================================

        public DateOnly FechaCita { get; set; }

        public TimeOnly HoraCita { get; set; }


        // =====================================
        // ESTADO Y CLASIFICACIÓN
        // =====================================

        public string? Estado { get; set; }

        public string CategoriaReserva { get; set; }
            = string.Empty;

        public bool EsProxima { get; set; }

        public bool EsHistorial { get; set; }
    }
}
