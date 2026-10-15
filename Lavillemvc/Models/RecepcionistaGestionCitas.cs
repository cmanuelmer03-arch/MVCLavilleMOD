
using System;

namespace Lavillemvc.Models
{
    public class RecepcionistaGestionCitas
    {
        // =====================================
        // IDENTIFICADORES
        // =====================================

        public int IdCita { get; set; }

        public int IdCliente { get; set; }

        public int IdServicioPrincipal { get; set; }

        public int? IdEmpleado { get; set; }


        // =====================================
        // INFORMACIÓN DEL CLIENTE
        // =====================================

        public string NombreCliente { get; set; }
            = string.Empty;

        public string? TipoCliente { get; set; }

        public int PuntosFidelidad { get; set; }


        // =====================================
        // SERVICIOS
        // =====================================

        public string ServicioPrincipal { get; set; }
            = string.Empty;

        public string Servicios { get; set; }
            = string.Empty;

        public decimal CostoEstimado { get; set; }


        // =====================================
        // ESPECIALISTAS
        // =====================================

        public int? IdEspecialista { get; set; }

        public string? NombreEspecialista { get; set; }

        public string? EspecialistasAsignados { get; set; }


        // =====================================
        // SUCURSAL
        // =====================================

        public int? IdSucursal { get; set; }

        public string? NombreSucursal { get; set; }

        public string? DireccionSucursal { get; set; }


        // =====================================
        // FECHA Y HORARIO
        // =====================================

        public DateOnly FechaCita { get; set; }

        public TimeOnly HoraCita { get; set; }

        public DateTime FechaHoraCita { get; set; }


        // =====================================
        // ESTADO DE LA CITA
        // =====================================

        public string? Estado { get; set; }

        public string EstadoNormalizado { get; set; }
            = string.Empty;


        // =====================================
        // CLASIFICACIÓN DE FECHAS
        // =====================================

        public bool EsHoy { get; set; }

        public bool EsProxima { get; set; }


        // =====================================
        // INFORMACIÓN ADICIONAL
        // =====================================

        public string? NombreRegistradoCita { get; set; }

        public string? NotasRecepcion { get; set; }

        public int? DuracionMinutos { get; set; }
    }
}
