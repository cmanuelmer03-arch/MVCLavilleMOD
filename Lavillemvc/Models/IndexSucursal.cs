
using System;

namespace Lavillemvc.Models
{
    public class SucursalModel
    {
        // Identificador de la sucursal
        public int IdSucursal { get; set; }

        // Nombre de la sucursal
        public string NombreSucursal { get; set; }
            = string.Empty;

        // Dirección
        public string Direccion { get; set; }
            = string.Empty;

        // Teléfono
        public string? Telefono { get; set; }

        // Horario de apertura
        public TimeSpan? HorarioApertura { get; set; }

        // Horario de cierre
        public TimeSpan? HorarioCierre { get; set; }

        // Estado de la sucursal
        public bool? Abierto { get; set; }
    }
}
