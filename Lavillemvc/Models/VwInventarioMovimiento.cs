
using System;
using System.Text.Json.Serialization;

namespace Lavillemvc.Models
{
    public class VwInventarioMovimiento
    {
        [JsonPropertyName("id_movimiento")]
        public long IdMovimiento { get; set; }

        [JsonPropertyName("id_detalle_inventario")]
        public int IdDetalleInventario { get; set; }

        [JsonPropertyName("id_inventario")]
        public int IdInventario { get; set; }

        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("id_producto")]
        public int IdProducto { get; set; }

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = "";

        [JsonPropertyName("nombre_producto")]
        public string NombreProducto { get; set; } = "";

        [JsonPropertyName("tipo_movimiento")]
        public string TipoMovimiento { get; set; } = "";

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("stock_anterior")]
        public int StockAnterior { get; set; }

        [JsonPropertyName("stock_nuevo")]
        public int StockNuevo { get; set; }

        [JsonPropertyName("motivo")]
        public string? Motivo { get; set; }

        [JsonPropertyName("registrado_por")]
        public string? RegistradoPor { get; set; }

        [JsonPropertyName("fecha_movimiento")]
        public DateTimeOffset FechaMovimiento { get; set; }
    }
}
