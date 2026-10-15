using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    // Modelo de lectura para public.vw_reportes_generales (Supabase/PostgreSQL).
    public class AdminReportesGenerales
    {
        // Selector del mes y tarjetas superiores.
        public DateOnly MesReferencia { get; set; }
        public string Periodo { get; set; } = string.Empty; // YYYY-MM

        public int ReservasMes { get; set; }
        public decimal? VariacionReservasPorcentaje { get; set; }

        // Pagos de citas fechadas en el mes (no fecha efectiva del cobro).
        public decimal IngresosTotales { get; set; }
        public decimal? VariacionIngresosPorcentaje { get; set; }

        // Clientes distintos con citas no canceladas ni no-show.
        public int ClientesActivos { get; set; }
        public decimal? VariacionClientesPorcentaje { get; set; }

        // No calculables con las tablas originales.
        public decimal? TasaOcupacion { get; set; }
        public decimal? VariacionOcupacionPorcentaje { get; set; }

        public decimal TicketPromedio { get; set; }
        public decimal? VariacionTicketPorcentaje { get; set; }

        // Columnas JSON retornadas como TEXT por la vista SQL.
        public string IngresosMensualesJson { get; set; } = "[]";
        public string ServiciosMasSolicitadosJson { get; set; } = "[]";
        public string RendimientoSucursalesJson { get; set; } = "[]";
        public string ReservasPorEstadoJson { get; set; } = "[]";
        public string ActividadCitasDiariasJson { get; set; } = "[]";
        public string ActividadSistemaJson { get; set; } = "[]";

        // Listas para utilizar directamente desde una vista Razor .cshtml.
        public List<IngresoMensual> IngresosMensuales =>
            LeerLista<IngresoMensual>(IngresosMensualesJson);

        public List<ServicioSolicitado> ServiciosMasSolicitados =>
            LeerLista<ServicioSolicitado>(ServiciosMasSolicitadosJson);

        public List<RendimientoSucursal> RendimientoSucursales =>
            LeerLista<RendimientoSucursal>(RendimientoSucursalesJson);

        public List<EstadoReserva> ReservasPorEstado =>
            LeerLista<EstadoReserva>(ReservasPorEstadoJson);

        public List<ActividadCitaDiaria> ActividadCitasDiarias =>
            LeerLista<ActividadCitaDiaria>(ActividadCitasDiariasJson);

        public List<ActividadSistemaItem> ActividadSistema =>
            LeerLista<ActividadSistemaItem>(ActividadSistemaJson);

        private static List<T> LeerLista<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public class IngresoMensual
        {
            public string Mes { get; set; } = string.Empty; // YYYY-MM
            public decimal Ingresos { get; set; }
        }

        public class ServicioSolicitado
        {
            public int IdServicio { get; set; }
            public string Servicio { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        public class RendimientoSucursal
        {
            public int IdSucursal { get; set; }
            public string NombreSucursal { get; set; } = string.Empty;
            public int Reservas { get; set; }
            public decimal Ingresos { get; set; }
            public decimal? OcupacionPorcentaje { get; set; }
            public string? ServicioTop { get; set; }
            public decimal? Valoracion { get; set; }
            public decimal? TendenciaPorcentaje { get; set; }
        }

        public class EstadoReserva
        {
            public string Estado { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal Porcentaje { get; set; }
        }

        public class ActividadCitaDiaria
        {
            public string Fecha { get; set; } = string.Empty; // YYYY-MM-DD
            public int Reservas { get; set; }
            public int Confirmadas { get; set; }
            public int Canceladas { get; set; }
        }

        // La base original no tiene bitácora; por eso ActividadSistemaJson = [].
        public class ActividadSistemaItem
        {
            public string Fecha { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
        }
    }
}
