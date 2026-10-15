using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace Lavillemvc.Controllers
{
    public class ProductoInventarioItem
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe el nombre del producto.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una categoría.")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe la marca.")]
        [StringLength(80, ErrorMessage = "La marca no puede superar los 80 caracteres.")]
        public string Marca { get; set; } = string.Empty;

        [Range(0, 1000000, ErrorMessage = "El stock actual debe ser igual o mayor que cero.")]
        public int StockActual { get; set; }

        [Range(0, 1000000, ErrorMessage = "El stock mínimo debe ser igual o mayor que cero.")]
        public int StockMinimo { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El precio debe ser igual o mayor que cero.")]
        public decimal Precio { get; set; }

        public string Estado
        {
            get
            {
                if (StockActual <= 0) return "Sin stock";
                if (StockActual * 2 < StockMinimo) return "Crítico";
                if (StockActual < StockMinimo) return "Bajo";
                return "En Stock";
            }
        }
    }

    public class InventarioController : Controller
    {
        private static readonly object Candado = new object();

        private static readonly string[] Categorias =
        {
            "Coloración",
            "Cuidado Capilar",
            "Herramientas",
            "Consumibles",
            "Mobiliario"
        };

        // Datos temporales en memoria mientras se conecta la base de datos.
        private static readonly List<ProductoInventarioItem> Productos =
            CrearProductosIniciales();

        private static readonly List<InventarioPanelInventario.MovimientoInventario> Movimientos =
            CrearMovimientosIniciales();

        private static int SiguienteMovimientoId = 4;
        private static DateTime UltimaActualizacion = DateTime.Now;

        public IActionResult PanelInventario()
        {
            lock (Candado)
            {
                var productos = Productos
                    .Select(CopiarProducto)
                    .ToList();

                var movimientos = Movimientos
                    .OrderByDescending(m => m.Fecha)
                    .Take(3)
                    .Select(CopiarMovimiento)
                    .ToList();

                var alertas = productos
                    .Where(p => p.StockActual <= 0 || p.StockActual < p.StockMinimo)
                    .OrderBy(p => p.Id)
                    .Take(3)
                    .Select(p => new InventarioPanelInventario.AlertaInventario
                    {
                        IdProducto = p.Id,
                        NombreProducto = p.Nombre,
                        Cantidad = p.StockActual,
                        Precio = p.Precio,
                        NivelAlerta = p.Estado
                    })
                    .ToList();

                var categorias = Categorias
                    .Select((nombre, indice) =>
                        new InventarioPanelInventario.CategoriaInventario
                        {
                            IdCategoria = indice + 1,
                            Categoria = nombre,
                            CantidadProductos = productos.Count(
                                p => p.Categoria == nombre),
                            TotalUnidades = productos
                                .Where(p => p.Categoria == nombre)
                                .Sum(p => p.StockActual)
                        })
                    .ToList();

                var modelo = new InventarioPanelInventario
                {
                    IdSucursal = 1,
                    NombreSucursal = "L'Avielle San Salvador",
                    Direccion = "San Salvador",
                    IdInventario = 1,
                    Encargado = "Carlos Mendoza",
                    UltimaActualizacion = UltimaActualizacion,

                    TotalProductos = productos.Count,
                    ProductosEnDetalle = productos.Count,
                    TotalUnidades = productos.Sum(p => p.StockActual),

                    StockBajo = productos.Count(
                        p => p.StockActual > 0 &&
                             p.StockActual < p.StockMinimo),

                    SinStock = productos.Count(
                        p => p.StockActual <= 0),

                    ValorTotal = productos.Sum(
                        p => p.StockActual * p.Precio),

                    AlertasJson = JsonSerializer.Serialize(alertas),
                    CategoriasJson = JsonSerializer.Serialize(categorias),
                    MovimientosJson = JsonSerializer.Serialize(movimientos)
                };

                return View(modelo);
            }
        }

        [HttpGet]
        public IActionResult GestionProductos(
            string busqueda = "",
            string categoria = "Todos",
            string ordenarPor = "Nombre",
            int pagina = 1,
            bool mostrarFormulario = false,
            int? editarId = null,
            int? detalleId = null)
        {
            return CargarVistaGestion(
                busqueda,
                categoria,
                ordenarPor,
                pagina,
                mostrarFormulario || editarId.HasValue,
                editarId,
                detalleId,
                null,
                null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarProducto(
            ProductoInventarioItem producto,
            string busquedaRegreso = "",
            string categoriaRegreso = "Todos",
            string ordenarPorRegreso = "Nombre",
            int paginaRegreso = 1)
        {
            if (!Categorias.Contains(producto.Categoria))
            {
                ModelState.AddModelError(
                    nameof(producto.Categoria),
                    "La categoría seleccionada no es válida.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MensajeError =
                    "Revisa los campos marcados antes de guardar.";

                return CargarVistaGestion(
                    busquedaRegreso,
                    categoriaRegreso,
                    ordenarPorRegreso,
                    paginaRegreso,
                    true,
                    producto.Id > 0 ? producto.Id : (int?)null,
                    null,
                    producto,
                    null);
            }

            lock (Candado)
            {
                if (producto.Id == 0)
                {
                    producto.Id = Productos.Count == 0
                        ? 1
                        : Productos.Max(p => p.Id) + 1;

                    producto.Codigo = $"PRD-{producto.Id:000}";

                    producto.Nombre = producto.Nombre.Trim();
                    producto.Marca = producto.Marca.Trim();

                    Productos.Add(CopiarProducto(producto));

                    if (producto.StockActual > 0)
                    {
                        AgregarMovimiento(
                            producto.Nombre,
                            "Entrada",
                            producto.StockActual,
                            "Carlos Mendoza");
                    }

                    TempData["Mensaje"] =
                        "Producto agregado correctamente.";
                }
                else
                {
                    var existente = Productos.FirstOrDefault(
                        p => p.Id == producto.Id);

                    if (existente == null)
                    {
                        TempData["Error"] =
                            "No se encontró el producto que intentas editar.";

                        return RedirectToAction(
                            nameof(GestionProductos),
                            new
                            {
                                busqueda = busquedaRegreso,
                                categoria = categoriaRegreso,
                                ordenarPor = ordenarPorRegreso,
                                pagina = paginaRegreso
                            });
                    }

                    int diferenciaStock =
                        producto.StockActual - existente.StockActual;

                    existente.Nombre = producto.Nombre.Trim();
                    existente.Categoria = producto.Categoria;
                    existente.Marca = producto.Marca.Trim();
                    existente.StockActual = producto.StockActual;
                    existente.StockMinimo = producto.StockMinimo;
                    existente.Precio = producto.Precio;

                    if (diferenciaStock > 0)
                    {
                        AgregarMovimiento(
                            existente.Nombre,
                            "Entrada",
                            diferenciaStock,
                            "Carlos Mendoza");
                    }
                    else if (diferenciaStock < 0)
                    {
                        AgregarMovimiento(
                            existente.Nombre,
                            "Salida",
                            Math.Abs(diferenciaStock),
                            "Carlos Mendoza");
                    }

                    TempData["Mensaje"] =
                        "Producto actualizado correctamente.";
                }

                UltimaActualizacion = DateTime.Now;
            }

            return RedirectToAction(
                nameof(GestionProductos),
                new
                {
                    busqueda = busquedaRegreso,
                    categoria = categoriaRegreso,
                    ordenarPor = ordenarPorRegreso,
                    pagina = paginaRegreso
                });
        }

        // Elimina uno o varios productos seleccionados.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarProductos(
            List<int> ids,
            string busquedaRegreso = "",
            string categoriaRegreso = "Todos",
            string ordenarPorRegreso = "Nombre",
            int paginaRegreso = 1)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Error"] =
                    "Selecciona al menos un producto para eliminar.";
            }
            else
            {
                int eliminados;

                lock (Candado)
                {
                    eliminados = Productos.RemoveAll(
                        p => ids.Contains(p.Id));

                    if (eliminados > 0)
                    {
                        UltimaActualizacion = DateTime.Now;
                    }
                }

                if (eliminados > 0)
                {
                    TempData["Mensaje"] =
                        $"Se eliminaron {eliminados} producto(s) correctamente.";
                }
                else
                {
                    TempData["Error"] =
                        "No se encontró ningún producto seleccionado.";
                }
            }

            return RedirectToAction(
                nameof(GestionProductos),
                new
                {
                    busqueda = busquedaRegreso,
                    categoria = categoriaRegreso,
                    ordenarPor = ordenarPorRegreso,
                    pagina = paginaRegreso
                });
        }

        private IActionResult CargarVistaGestion(
            string busqueda,
            string categoria,
            string ordenarPor,
            int pagina,
            bool mostrarFormulario,
            int? editarId,
            int? detalleId,
            ProductoInventarioItem formulario,
            string mensajeError)
        {
            List<ProductoInventarioItem> todos;

            lock (Candado)
            {
                todos = Productos
                    .Select(CopiarProducto)
                    .ToList();
            }

            busqueda = (busqueda ?? string.Empty).Trim();

            categoria = string.IsNullOrWhiteSpace(categoria)
                ? "Todos"
                : categoria;

            ordenarPor = string.IsNullOrWhiteSpace(ordenarPor)
                ? "Nombre"
                : ordenarPor;

            IEnumerable<ProductoInventarioItem> consulta = todos;

            if (!string.Equals(
                categoria,
                "Todos",
                StringComparison.OrdinalIgnoreCase))
            {
                consulta = consulta.Where(
                    p => p.Categoria.Equals(
                        categoria,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    p.Marca.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    p.Codigo.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase));
            }

            consulta = ordenarPor switch
            {
                "NombreDesc" => consulta.OrderByDescending(p => p.Nombre),
                "PrecioAsc" => consulta.OrderBy(p => p.Precio),
                "PrecioDesc" => consulta.OrderByDescending(p => p.Precio),
                "StockAsc" => consulta.OrderBy(p => p.StockActual),
                "StockDesc" => consulta.OrderByDescending(p => p.StockActual),
                _ => consulta.OrderBy(p => p.Nombre)
            };

            var filtrados = consulta.ToList();

            const int productosPorPagina = 7;

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    filtrados.Count / (double)productosPorPagina));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaProductos = filtrados
                .Skip((pagina - 1) * productosPorPagina)
                .Take(productosPorPagina)
                .ToList();

            ViewBag.CategoriasDisponibles = Categorias;
            ViewBag.Busqueda = busqueda;
            ViewBag.Categoria = categoria;
            ViewBag.OrdenarPor = ordenarPor;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalFiltrados = filtrados.Count;

            ViewBag.MostrarDesde = filtrados.Count == 0
                ? 0
                : ((pagina - 1) * productosPorPagina) + 1;

            ViewBag.MostrarHasta = Math.Min(
                pagina * productosPorPagina,
                filtrados.Count);

            ViewBag.TotalProductos = todos.Count;

            ViewBag.StockBajo = todos.Count(
                p => p.StockActual > 0 &&
                     p.StockActual < p.StockMinimo);

            ViewBag.SinStock = todos.Count(
                p => p.StockActual <= 0);

            ViewBag.FormularioAbierto = mostrarFormulario;

            ViewBag.ProductoFormulario =
                formulario ??
                (editarId.HasValue
                    ? todos.FirstOrDefault(p => p.Id == editarId.Value)
                    : new ProductoInventarioItem
                    {
                        Categoria = Categorias[0],
                        StockMinimo = 5
                    });

            ViewBag.ProductoDetalle = detalleId.HasValue
                ? todos.FirstOrDefault(p => p.Id == detalleId.Value)
                : null;

            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.MensajeError = mensajeError ?? TempData["Error"];

            if (editarId.HasValue && ViewBag.ProductoFormulario == null)
            {
                ViewBag.FormularioAbierto = false;
                ViewBag.MensajeError =
                    "El producto que intentas editar ya no existe.";

                ViewBag.ProductoFormulario =
                    new ProductoInventarioItem
                    {
                        Categoria = Categorias[0],
                        StockMinimo = 5
                    };
            }

            return View("GestionProductos", paginaProductos);
        }

        private static ProductoInventarioItem CopiarProducto(
            ProductoInventarioItem p)
        {
            return new ProductoInventarioItem
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Categoria = p.Categoria,
                Marca = p.Marca,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Precio = p.Precio
            };
        }

        private static InventarioPanelInventario.MovimientoInventario
            CopiarMovimiento(
                InventarioPanelInventario.MovimientoInventario m)
        {
            return new InventarioPanelInventario.MovimientoInventario
            {
                IdMovimiento = m.IdMovimiento,
                Fecha = m.Fecha,
                Producto = m.Producto,
                Tipo = m.Tipo,
                Cantidad = m.Cantidad,
                Responsable = m.Responsable
            };
        }

        private static void AgregarMovimiento(
            string producto,
            string tipo,
            int cantidad,
            string responsable)
        {
            Movimientos.Add(
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = SiguienteMovimientoId++,
                    Fecha = DateTime.Now,
                    Producto = producto,
                    Tipo = tipo,
                    Cantidad = cantidad,
                    Responsable = responsable
                });

            if (Movimientos.Count > 30)
            {
                Movimientos.RemoveAt(0);
            }
        }

        private static List<ProductoInventarioItem>
            CrearProductosIniciales()
        {
            var productos = new List<ProductoInventarioItem>
            {
                new ProductoInventarioItem
                {
                    Id = 1,
                    Codigo = "PRD-001",
                    Nombre = "Tinte L'Oréal Rubio 7.0",
                    Categoria = "Coloración",
                    Marca = "L'Oréal",
                    StockActual = 24,
                    StockMinimo = 10,
                    Precio = 35m
                },
                new ProductoInventarioItem
                {
                    Id = 2,
                    Codigo = "PRD-002",
                    Nombre = "Decolorante Premium 500g",
                    Categoria = "Coloración",
                    Marca = "Wella",
                    StockActual = 3,
                    StockMinimo = 10,
                    Precio = 55m
                },
                new ProductoInventarioItem
                {
                    Id = 3,
                    Codigo = "PRD-003",
                    Nombre = "Shampoo Kerastase 500ml",
                    Categoria = "Cuidado Capilar",
                    Marca = "Kérastase",
                    StockActual = 18,
                    StockMinimo = 8,
                    Precio = 85m
                },
                new ProductoInventarioItem
                {
                    Id = 4,
                    Codigo = "PRD-004",
                    Nombre = "Tratamiento Botox Capilar",
                    Categoria = "Cuidado Capilar",
                    Marca = "Cadiveu",
                    StockActual = 7,
                    StockMinimo = 5,
                    Precio = 120m
                },
                new ProductoInventarioItem
                {
                    Id = 5,
                    Codigo = "PRD-005",
                    Nombre = "Papel Aluminio Rollo",
                    Categoria = "Consumibles",
                    Marca = "Genérico",
                    StockActual = 2,
                    StockMinimo = 5,
                    Precio = 15m
                },
                new ProductoInventarioItem
                {
                    Id = 6,
                    Codigo = "PRD-006",
                    Nombre = "Secador Profesional",
                    Categoria = "Herramientas",
                    Marca = "BaByliss",
                    StockActual = 4,
                    StockMinimo = 2,
                    Precio = 450m
                },
                new ProductoInventarioItem
                {
                    Id = 7,
                    Codigo = "PRD-007",
                    Nombre = "Aceite de Argán 100ml",
                    Categoria = "Cuidado Capilar",
                    Marca = "Moroccan",
                    StockActual = 5,
                    StockMinimo = 10,
                    Precio = 95m
                }
            };

            for (int id = 8; id <= 156; id++)
            {
                bool stockCritico = id >= 8 && id <= 16;
                bool sinStock = id == 20 || id == 70 || id == 120;

                int stock = stockCritico
                    ? 3
                    : sinStock
                        ? 0
                        : 3 + (id % 6);

                int minimo = stockCritico || sinStock ? 10 : 3;

                if (id == 17) stock--;
                if (id == 19) stock--;

                decimal precio = stockCritico
                    ? 50m
                    : 41.23m + (id % 5) * 5m;

                if (id == 156) precio += 1.27m;

                productos.Add(
                    new ProductoInventarioItem
                    {
                        Id = id,
                        Codigo = $"PRD-{id:000}",
                        Nombre = $"Producto de prueba {id:000}",
                        Categoria = Categorias[(id - 8) % Categorias.Length],
                        Marca = "Marca genérica",
                        StockActual = stock,
                        StockMinimo = minimo,
                        Precio = precio
                    });
            }

            return productos;
        }

        private static List<InventarioPanelInventario.MovimientoInventario>
            CrearMovimientosIniciales()
        {
            return new List<InventarioPanelInventario.MovimientoInventario>
            {
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 1,
                    Fecha = DateTime.Now.AddMinutes(-20),
                    Producto = "Shampoo Kerastase 500ml",
                    Tipo = "Entrada",
                    Cantidad = 15,
                    Responsable = "Carlos Mendoza"
                },
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 2,
                    Fecha = DateTime.Now.AddHours(-2),
                    Producto = "Tinte L'Oréal Rubio 7.0",
                    Tipo = "Salida",
                    Cantidad = 5,
                    Responsable = "Ana Martínez"
                },
                new InventarioPanelInventario.MovimientoInventario
                {
                    IdMovimiento = 3,
                    Fecha = DateTime.Now.AddHours(-4),
                    Producto = "Tratamiento Botox Capilar",
                    Tipo = "Entrada",
                    Cantidad = 20,
                    Responsable = "Carlos Mendoza"
                }
            };
        }
    }
}