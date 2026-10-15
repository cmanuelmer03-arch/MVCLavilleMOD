
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class EspecialistaAgenda
    {
        // =====================================
        // INFORMACIÓN DEL ESPECIALISTA
        // =====================================

        public int IdEspecialista { get; set; }

        public string NombreEspecialista { get; set; }
            = string.Empty;

        public string? RolEspecialista { get; set; }

        public int IdEspecialidad { get; set; }

        public string NombreEspecialidad { get; set; }
            = string.Empty;

        public int? AniosExperiencia { get; set; }

        public bool? Disponibilidad { get; set; }

        public string? HorarioEspecialistas { get; set; }


        // =====================================
        // FECHA DEL PANEL
        // =====================================

        public DateOnly FechaHoy { get; set; }


        // =====================================
        // INDICADORES PRINCIPALES
        // =====================================

        public int CitasHoy { get; set; }

        public int PendientesHoy { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string CitasJson { get; set; } = "[]";

        public string? ProximaCitaJson { get; set; }


        // =====================================
        // LISTAS PARA LA INTERFAZ
        // =====================================

        public List<CitaAgenda> Citas { get; set; }
            = new();

        public CitaAgenda? ProximaCita { get; set; }


        // =====================================
        // RESUMEN DEL DÍA
        // =====================================

        public decimal? HorasAtencion { get; set; }

        public decimal? HorasDescanso { get; set; }

        public decimal? HorasDisponibles { get; set; }


        // =====================================
        // CLASE INTERNA: CITA DE LA AGENDA
        // =====================================

        public class CitaAgenda
        {
            public int IdCita { get; set; }

            public int IdCliente { get; set; }

            public string NombreCliente { get; set; }
                = string.Empty;

            public string? TipoCliente { get; set; }

            public int IdServicio { get; set; }

            public string Servicio { get; set; }
                = string.Empty;

            public DateOnly FechaCita { get; set; }

            public TimeOnly HoraCita { get; set; }

            public DateTime FechaHoraCita { get; set; }

            public string? Estado { get; set; }

            public string EstadoNormalizado { get; set; }
                = string.Empty;

            public int? IdSucursal { get; set; }

            public string? NombreSucursal { get; set; }

            public int? DuracionMinutos { get; set; }

            public string? NotasDiagnostico { get; set; }

            public string? ProductosNecesarios { get; set; }
        }
    }
}
