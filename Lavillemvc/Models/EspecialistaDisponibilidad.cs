
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class EspecialistaDisponibilidad
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

        public bool? Disponibilidad { get; set; }

        public string? HorarioGeneralTexto { get; set; }


        // =====================================
        // FECHA DE REFERENCIA
        // =====================================

        public DateOnly FechaHoy { get; set; }


        // =====================================
        // RESUMEN SEMANAL
        // =====================================

        public int DiasConfigurados { get; set; }

        public int DiasActivos { get; set; }

        public bool TieneHorarioConfigurado { get; set; }

        public decimal? HorasSemanales { get; set; }

        public string? DescansoSemanal { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string HorariosJson { get; set; } = "[]";

        public string AusenciasJson { get; set; } = "[]";


        // =====================================
        // COLECCIONES PARA LA INTERFAZ
        // =====================================

        public List<HorarioDia> Horarios { get; set; }
            = new();

        public List<AusenciaEspecialista> Ausencias
        { get; set; } = new();


        // =====================================
        // PRÓXIMA AUSENCIA
        // =====================================

        public string? ProximaAusenciaTitulo
        { get; set; }

        public string? ProximaAusenciaTipo
        { get; set; }

        public DateOnly? ProximaAusenciaInicio
        { get; set; }

        public DateOnly? ProximaAusenciaFin
        { get; set; }


        // =====================================
        // CLASE INTERNA: HORARIO SEMANAL
        // =====================================

        public class HorarioDia
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


        // =====================================
        // CLASE INTERNA: AUSENCIAS
        // =====================================

        public class AusenciaEspecialista
        {
            public long IdAusencia { get; set; }

            public int IdEspecialista { get; set; }

            public string Titulo { get; set; }
                = string.Empty;

            public string Tipo { get; set; }
                = string.Empty;

            public string? Descripcion { get; set; }

            public DateOnly FechaInicio { get; set; }

            public DateOnly FechaFin { get; set; }
        }
    }
}
