using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class ReservarCita
    {
        // =====================================
        // DATOS DE LA RESERVA
        // =====================================

        public int IdCliente { get; set; }

        public int PuntosFidelidad { get; set; }

        public int? IdServicio { get; set; }

        public int? IdEspecialista { get; set; }

        public int? IdSucursal { get; set; }

        public DateTime? FechaCita { get; set; }

        public TimeSpan? HoraCita { get; set; }


        // =====================================
        // DATOS DEL SERVICIO
        // =====================================

        public string? Servicio { get; set; }

        public string? DescripcionServicio { get; set; }

        public decimal CostoServicio { get; set; }


        // =====================================
        // DATOS DEL ESPECIALISTA
        // =====================================

        public string? Especialista { get; set; }

        public string? RolEspecialista { get; set; }

        public int? AniosExperiencia { get; set; }

        public string? HorarioEspecialista { get; set; }


        // =====================================
        // DATOS DE LA SUCURSAL
        // =====================================

        public string? Sucursal { get; set; }

        public string? DireccionSucursal { get; set; }

        public TimeSpan? HorarioApertura { get; set; }

        public TimeSpan? HorarioCierre { get; set; }


        // =====================================
        // DATOS DE LA ESPECIALIDAD
        // =====================================

        public int? IdEspecialidad { get; set; }

        public string? Especialidad { get; set; }


        // =====================================
        // DISPONIBILIDAD
        // =====================================

        public bool ServicioDisponible { get; set; }

        public bool EspecialistaDisponible { get; set; }

        public bool ServicioSucursalDisponible { get; set; }

        public bool SucursalAbierta { get; set; }


        // =====================================
        // HORAS DISPONIBLES
        // =====================================

        public List<TimeSpan> HorasDisponibles { get; set; }
            = new List<TimeSpan>();


        // =====================================
        // OPCIONES DISPONIBLES PARA RESERVAR
        // =====================================

        public List<ReservaOpcion> OpcionesDisponibles { get; set; }
            = new List<ReservaOpcion>();


        // =====================================
        // MENSAJE DE OPERACIÓN
        // =====================================

        public string? Mensaje { get; set; }

        public bool ReservaExitosa { get; set; }


        // =====================================
        // OPCIÓN DE RESERVA
        // =====================================

        public class ReservaOpcion
        {
            public int IdServicio { get; set; }

            public string Servicio { get; set; } = "";

            public decimal CostoServicio { get; set; }

            public int IdEspecialista { get; set; }

            public string Especialista { get; set; } = "";

            public string? RolEspecialista { get; set; }

            public int? AniosExperiencia { get; set; }

            public string? HorarioEspecialista { get; set; }

            public int IdSucursal { get; set; }

            public string Sucursal { get; set; } = "";

            public string? DireccionSucursal { get; set; }

            public TimeSpan? HorarioApertura { get; set; }

            public TimeSpan? HorarioCierre { get; set; }

            public int? IdEspecialidad { get; set; }

            public string? Especialidad { get; set; }
        }
    }
}
