
using System;

namespace Lavillemvc.Models
{
    public class AdminGestionSucursales
    {
        // =====================================
        // IDENTIFICACIÓN DE SUCURSAL
        // =====================================

        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }
            = string.Empty;

        public string Direccion { get; set; }
            = string.Empty;

        public string? Telefono { get; set; }


        // =====================================
        // HORARIOS
        // =====================================

        public TimeSpan? HorarioApertura { get; set; }

        public TimeSpan? HorarioCierre { get; set; }


        // =====================================
        // ESTADO DE SUCURSAL
        // =====================================

        public bool? Abierto { get; set; }

        public string EstadoSucursal { get; set; }
            = string.Empty;


        // =====================================
        // GERENTE ASIGNADO
        // =====================================

        public int? IdGerente { get; set; }

        public string? NombreGerente { get; set; }


        // =====================================
        // PERSONAL
        // =====================================

        public int TotalEmpleados { get; set; }


        // =====================================
        // RENDIMIENTO MENSUAL
        // =====================================

        public int ReservasMes { get; set; }

        public decimal IngresosCitasMes { get; set; }

        public decimal? Valoracion { get; set; }


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int TotalSucursales { get; set; }

        public int SucursalesActivas { get; set; }

        public int SucursalesCerradas { get; set; }

        public int EmpleadosTotales { get; set; }


        // =====================================
        // PERIODO DEL DASHBOARD
        // =====================================

        public DateOnly MesReferencia { get; set; }
    }
}
