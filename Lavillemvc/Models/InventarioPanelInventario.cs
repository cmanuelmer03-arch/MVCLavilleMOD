
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class InventarioPanelInventario
    {
        // INFORMACIÓN DE SUCURSAL
        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }
            = string.Empty;

        public string Direccion { get; set; }
            = string.Empty;

        // INFORMACIÓN DEL INVENTARIO
        public int? IdInventario { get; set; }

        public string? Encargado { get; set; }

        public DateTime? UltimaActualizacion
        { get; set; }

        // INDICADORES PRINCIPALES
        public int TotalProductos { get; set; }

        public int ProductosEnDetalle { get; set; }

        public int TotalUnidades { get; set; }

        public int StockBajo { get; set; }

        public int SinStock { get; set; }

        public decimal ValorTotal { get; set; }

        // DATOS JSON
        public string AlertasJson { get; set; } = "[]";

        public string CategoriasJson { get; set; } = "[]";

        public string MovimientosJson { get; set; } = "[]";

        // LISTA DE ALERTAS
        public List<AlertaInventario> Alertas
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<AlertaInventario>>(
                        AlertasJson ?? "[]"
                    ) ?? new List<AlertaInventario>();
            }
        }

        // LISTA DE CATEGORÍAS
        public List<CategoriaInventario> Categorias
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<CategoriaInventario>>(
                        CategoriasJson ?? "[]"
                    ) ?? new List<CategoriaInventario>();
            }
        }

        // LISTA DE MOVIMIENTOS
        public List<MovimientoInventario> Movimientos
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<MovimientoInventario>>(
                        MovimientosJson ?? "[]"
                    ) ?? new List<MovimientoInventario>();
            }
        }

        // CLASE INTERNA: ALERTAS
        public class AlertaInventario
        {
            public int IdProducto { get; set; }

            public string NombreProducto { get; set; }
                = string.Empty;

            public int Cantidad { get; set; }

            public decimal Precio { get; set; }

            public string NivelAlerta { get; set; }
                = string.Empty;
        }

        // CLASE INTERNA: CATEGORÍAS
        public class CategoriaInventario
        {
            public int IdCategoria { get; set; }

            public string Categoria { get; set; }
                = string.Empty;

            public int CantidadProductos { get; set; }

            public int TotalUnidades { get; set; }
        }

        // CLASE INTERNA: MOVIMIENTOS
        public class MovimientoInventario
        {
            public int IdMovimiento { get; set; }

            public DateTime Fecha { get; set; }

            public string Producto { get; set; }
                = string.Empty;

            public string Tipo { get; set; }
                = string.Empty;

            public int Cantidad { get; set; }

            public string Responsable { get; set; }
                = string.Empty;
        }
    }
}


