
using System;

namespace Lavillemvc.Models
{
    public class AdminGestionSucursales
    {
        // IDENTIFICACIÓN DE LA SUCURSAL

        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; } = "";


        // INFORMACIÓN DE CONTACTO

        public string Direccion { get; set; } = "";

        public string? Telefono { get; set; }


        // HORARIOS DE ATENCIÓN

        public TimeSpan? HorarioApertura { get; set; }

        public TimeSpan? HorarioCierre { get; set; }


        // ESTADO DE LA SUCURSAL

        public bool? Abierto { get; set; }

        public string EstadoSucursal { get; set; } = "";


        // INFORMACIÓN DE EMPLEADOS

        public string Gerente { get; set; } = "";

        public int CantidadEmpleados { get; set; }


        // RENDIMIENTO MENSUAL

        public int ReservasMes { get; set; }

        public decimal IngresosMes { get; set; }

        public decimal? ValoracionPromedio { get; set; }


        // ESTADÍSTICAS GENERALES

        public int TotalSucursales { get; set; }

        public int SucursalesAbiertas { get; set; }

        public int SucursalesCerradas { get; set; }

        public int TotalEmpleados { get; set; }


        // MOSTRAR HORARIOS

        public string HorarioMostrar
        {
            get
            {
                if (!HorarioApertura.HasValue ||
                    !HorarioCierre.HasValue)
                {
                    return "Horario no asignado";
                }

                return
                    HorarioApertura.Value.ToString(@"hh\:mm")
                    + " - " +
                    HorarioCierre.Value.ToString(@"hh\:mm");
            }
        }


        // MOSTRAR TELÉFONO

        public string TelefonoMostrar
        {
            get
            {
                return string.IsNullOrWhiteSpace(Telefono)
                    ? "Sin teléfono registrado"
                    : Telefono;
            }
        }


        // MOSTRAR VALORACIÓN

        public string ValoracionMostrar
        {
            get
            {
                return ValoracionPromedio.HasValue
                    ? ValoracionPromedio.Value.ToString("0.0")
                    : "Sin valoración";
            }
        }
    }
}
