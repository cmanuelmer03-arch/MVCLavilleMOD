using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Controllers
{
    public class InventarioController : Controller
    {
        public IActionResult GestionProductos()
        {
            return View();
        }

        public IActionResult PanelInventario()
        {
            var alertas = new List<InventarioPanelInventario.AlertaInventario>
            {
                new InventarioPanelInventario.AlertaInventario
                {
                    IdProducto = 1,
                    NombreProducto = "Shampoo nutritivo",
                    Cantidad = 4,
                    Precio = 12.50m,
                    NivelAlerta = "Stock bajo"
                },
                new InventarioPanelInventario.AlertaInventario
                {
                    IdProducto = 2,
                    NombreProducto = "Tinte castaño",
                    Cantidad = 0,
                    Precio = 8.75m,
                    NivelAlerta = "Sin stock"
                },
                new InventarioPanelInventario.AlertaInventario
                {
                    IdProducto = 3,
                    NombreProducto = "Acondicionador",
                    Cantidad = 3,
                    Precio = 10.00m,
                    NivelAlerta = "Stock bajo"
                }
            };

            var categorias = new List<InventarioPanelInventario.CategoriaInventario>
            {
                new InventarioPanelInventario.CategoriaInventario
                {
                    IdCategoria = 1,
                    Categoria = "Cuidado capilar",
                    CantidadProductos = 45,
                    TotalUnidades = 240
                },
                new InventarioPanelInventario.CategoriaInventario
                {
                    IdCategoria = 2,
                    Categoria = "Coloración",
                    CantidadProductos = 35,
                    TotalUnidades = 180
                },
                new InventarioPanelInventario.CategoriaInventario
                {
                    IdCategoria = 3,
                    Categoria = "Herramientas",
                    CantidadProductos = 28,
                    TotalUnidades = 95
                },
                new InventarioPanelInventario.CategoriaInventario
                {
                    IdCategoria = 4,
                    Categoria = "Accesorios",
                    CantidadProductos = 48,
                    TotalUnidades = 328
                }
            };

            var movimientos = new List<InventarioPanelInventario.MovimientoInventario>
            {
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 1,
                    Fecha = DateTime.Now.AddMinutes(-20),
                    Producto = "Shampoo nutritivo",
                    Tipo = "Entrada",
                    Cantidad = 15,
                    Responsable = "Carlos Mendoza"
                },
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 2,
                    Fecha = DateTime.Now.AddHours(-2),
                    Producto = "Tinte castaño",
                    Tipo = "Salida",
                    Cantidad = 5,
                    Responsable = "Ana Martínez"
                },
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 3,
                    Fecha = DateTime.Now.AddHours(-4),
                    Producto = "Acondicionador",
                    Tipo = "Entrada",
                    Cantidad = 20,
                    Responsable = "Carlos Mendoza"
                }
            };

            var modelo = new InventarioPanelInventario
            {
                IdSucursal = 1,
                NombreSucursal = "L'Avielle San Salvador",
                Direccion = "San Salvador",
                IdInventario = 1,
                Encargado = "Carlos Mendoza",
                UltimaActualizacion = DateTime.Now,

                TotalProductos = 156,
                ProductosEnDetalle = 156,
                TotalUnidades = 843,
                StockBajo = 12,
                SinStock = 3,
                ValorTotal = 45800m,

                AlertasJson = JsonSerializer.Serialize(alertas),
                CategoriasJson = JsonSerializer.Serialize(categorias),
                MovimientosJson = JsonSerializer.Serialize(movimientos)
            };

            return View(modelo);
        }
    }
}
