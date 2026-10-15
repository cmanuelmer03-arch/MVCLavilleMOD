
using System;

namespace Lavillemvc.Models
{
    public class VwInventarioProducto
    {
        public int IdInventario { get; set; }

        public int IdSucursal { get; set; }

        public string? Encargado { get; set; }

        public DateTime? UltimaActualizacion { get; set; }

        public int IdDetalleInventario { get; set; }

        public int IdProducto { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Marca { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public decimal Precio { get; set; }

        public string? Descripcion { get; set; }

        public bool? Disponible { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
