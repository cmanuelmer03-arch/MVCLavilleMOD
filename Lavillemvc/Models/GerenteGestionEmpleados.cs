
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class GerenteGestionEmpleados
    {
        // =====================================
        // IDENTIFICADORES
        // =====================================

        public int IdEspecialista { get; set; }

        public int? IdEmpleado { get; set; }

        public int? IdSucursal { get; set; }

        public string? NombreSucursal { get; set; }


        // =====================================
        // INFORMACIÓN DEL ESPECIALISTA
        // =====================================

        public string NombreCompleto { get; set; }
            = string.Empty;

        public string? Cargo { get; set; }

        public int IdEspecialidad { get; set; }

        public string Especialidad { get; set; }
            = string.Empty;

        public int? AniosExperiencia { get; set; }

        public string CategoriaFiltro { get; set; }
            = string.Empty;

        public string? FotoUrl { get; set; }


        // =====================================
        // ESTADO
        // =====================================

        public bool? Disponibilidad { get; set; }

        public string EstadoDisponibilidad
        { get; set; } = string.Empty;


        // =====================================
        // INFORMACIÓN DE HORARIOS
        // =====================================

        public string? HorarioGeneral { get; set; }

        public int? DiasConfigurados { get; set; }

        public int? DiasActivos { get; set; }

        public decimal? HorasSemanales { get; set; }

        public string? TipoJornada { get; set; }


        // =====================================
        // HORARIO SEMANAL EN JSON
        // =====================================

        public string HorarioSemanalJson
        { get; set; } = "[]";

        public List<HorarioSemanalEmpleado> Horarios
        { get; set; } = new();


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int TotalEspecialistas { get; set; }

        public int TotalTiempoCompleto { get; set; }

        public int TotalMedioTiempo { get; set; }

        public int TotalConHorarioConfigurado
        { get; set; }


        // =====================================
        // CLASE INTERNA: HORARIO SEMANAL
        // =====================================

        public class HorarioSemanalEmpleado
        {
            public int DiaSemana { get; set; }

            public string NombreDia { get; set; }
                = string.Empty;

            public bool Configurado { get; set; }

            public bool Habilitado { get; set; }

            public string? HoraInicio { get; set; }

            public string? HoraFin { get; set; }

            public string? InicioAlmuerzo { get; set; }

            public string? FinAlmuerzo { get; set; }
        }
    }
}
