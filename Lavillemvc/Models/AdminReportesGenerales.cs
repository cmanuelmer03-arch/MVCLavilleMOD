
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class AdminReportesGenerales
    {
        public int Anio { get; set; }
        public int Mes { get; set; }

        public int Reservas { get; set; }
        public int ReservasMesAnterior { get; set; }

        public decimal Ingresos { get; set; }
        public decimal IngresosMesAnterior { get; set; }

        public int ClientesConReservas { get; set; }
        public int ClientesMesAnterior { get; set; }

        public int PagosConfirmados { get; set; }

        public decimal TicketPromedio
        {
            get
            {
                return PagosConfirmados > 0
                    ? Ingresos / PagosConfirmados
                    : 0;
            }
        }

        // No se calcula ocupación sin capacidad disponible.
        public decimal? TasaOcupacion { get; set; }

        public List<AdminReporteMes> IngresosMensuales
        { get; set; } = new();

        public List<AdminReporteServicio> ServiciosPopulares
        { get; set; } = new();

        public List<AdminReporteSucursal> Sucursales
        { get; set; } = new();

        public List<AdminReporteEstado> EstadosReservas
        { get; set; } = new();

        public List<AdminReporteActividad> Actividad
        { get; set; } = new();
    }

    public class AdminReporteMes
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Etiqueta { get; set; } = "";
        public decimal Ingresos { get; set; }
    }

    public class AdminReporteServicio
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; } = "";
        public int Reservas { get; set; }
    }

    public class AdminReporteSucursal
    {
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = "";
        public int Reservas { get; set; }
        public int ReservasMesAnterior { get; set; }
        public decimal Ingresos { get; set; }
        public string ServicioTop { get; set; } = "";
        public decimal? Ocupacion { get; set; }
        public decimal? Valoracion { get; set; }

        public decimal? TendenciaReservas
        {
            get
            {
                if (ReservasMesAnterior == 0)
                    return null;

                return 100m *
                    (Reservas - ReservasMesAnterior) /
                    ReservasMesAnterior;
            }
        }
    }

    public class AdminReporteEstado
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
    }

    public class AdminReporteActividad
    {
        public DateTime Fecha { get; set; }
        public int CitasProgramadas { get; set; }
        public int ClientesRegistrados { get; set; }
    }
}
