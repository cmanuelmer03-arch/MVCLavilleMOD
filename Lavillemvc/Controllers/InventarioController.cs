
using Lavillemvc.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lavillemvc.Controllers
{
    public class ProductoInventarioItem
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public int IdInventario { get; set; }
        public int IdSucursal { get; set; }

        public string Codigo { get; set; } = "";

        [Required(ErrorMessage = "Escribe el nombre.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Selecciona una categoría.")]
        public string Categoria { get; set; } = "";

        [Required(ErrorMessage = "Escribe la marca.")]
        public string Marca { get; set; } = "";

        [Range(0, 1000000)]
        public int StockActual { get; set; }

        [Range(0, 1000000)]
        public int StockMinimo { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal Precio { get; set; }

        public string Estado { get; set; } = "";
    }

    public class InventarioController : Controller
    {
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private static readonly JsonSerializerOptions OpcionesJson =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

        private readonly IConfiguration _config;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(
            IConfiguration configuration,
            ILogger<InventarioController> logger)
        {
            _config = configuration;
            _logger = logger;
        }

        private string? Url =>
            _config["Supabase:Url"]?.TrimEnd('/');

        private string? Clave =>
            _config["Supabase:PublishableKey"];

        private sealed class ResultadoProductos
        {
            public List<VwInventarioProducto> Productos { get; set; }
                = new List<VwInventarioProducto>();

            public string? Error { get; set; }
        }

        private sealed class ResultadoApi
        {
            public bool Correcto { get; set; }
            public string Contenido { get; set; } = "";
            public string Error { get; set; } = "";
        }

        private static string LeerError(string contenido)
        {
            try
            {
                using var documento =
                    JsonDocument.Parse(contenido);

                if (documento.RootElement.TryGetProperty(
                    "message", out var mensaje))
                {
                    return mensaje.GetString()
                        ?? "Supabase rechazó la operación.";
                }
            }
            catch (JsonException)
            {
            }

            return "Supabase rechazó la operación.";
        }

        // Ejecuta funciones SQL de Supabase
        // sin requerir un segundo login de inventario.
        private async Task<ResultadoApi> EjecutarRpcAsync(
            string nombreFuncion,
            object parametros)
        {
            var resultado = new ResultadoApi();

            if (string.IsNullOrWhiteSpace(Url) ||
                string.IsNullOrWhiteSpace(Clave))
            {
                resultado.Error =
                    "Falta configurar la API REST de Supabase.";

                return resultado;
            }

            try
            {
                using var solicitud = new HttpRequestMessage(
                    HttpMethod.Post,
                    Url + "/rest/v1/rpc/" + nombreFuncion);

                solicitud.Headers.Add("apikey", Clave);

                solicitud.Content = new StringContent(
                    JsonSerializer.Serialize(parametros),
                    Encoding.UTF8,
                    "application/json");

                using var respuesta =
                    await Http.SendAsync(solicitud);

                resultado.Contenido =
                    await respuesta.Content.ReadAsStringAsync();

                resultado.Correcto =
                    respuesta.IsSuccessStatusCode;

                if (!resultado.Correcto)
                {
                    resultado.Error = LeerError(
                        resultado.Contenido);

                    _logger.LogWarning(
                        "RPC {Funcion} devolvió HTTP {Codigo}: {Error}",
                        nombreFuncion,
                        (int)respuesta.StatusCode,
                        resultado.Error);
                }
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is UriFormatException)
            {
                resultado.Error =
                    "No se pudo comunicar con Supabase.";

                _logger.LogError(
                    ex,
                    "Error al ejecutar {Funcion}",
                    nombreFuncion);
            }

            return resultado;
        }

        // OBTENER PRODUCTOS DESDE LA VIEW

        private async Task<ResultadoProductos>
            ConsultarProductosAsync()
        {
            var resultado = new ResultadoProductos();

            if (string.IsNullOrWhiteSpace(Url) ||
                string.IsNullOrWhiteSpace(Clave))
            {
                resultado.Error =
                    "La API REST de Supabase no está configurada.";

                return resultado;
            }

            try
            {
                const int tamanoPagina = 500;
                int desplazamiento = 0;

                int? idSucursal =
                    _config.GetValue<int?>(
                        "Inventario:IdSucursal");

                while (true)
                {
                    string direccion =
                        Url +
                        "/rest/v1/vw_inventario_productos" +
                        "?select=*" +
                        "&order=id_detalle_inventario.asc" +
                        "&limit=" + tamanoPagina +
                        "&offset=" + desplazamiento;

                    if (idSucursal.HasValue)
                    {
                        direccion +=
                            "&id_sucursal=eq." +
                            idSucursal.Value;
                    }

                    using var solicitud = new HttpRequestMessage(
                        HttpMethod.Get,
                        direccion);

                    solicitud.Headers.Add("apikey", Clave);

                    using var respuesta =
                        await Http.SendAsync(solicitud);

                    if (!respuesta.IsSuccessStatusCode)
                    {
                        resultado.Productos.Clear();

                        resultado.Error =
                            "No se pudieron consultar productos. HTTP " +
                            (int)respuesta.StatusCode;

                        return resultado;
                    }

                    string contenido =
                        await respuesta.Content.ReadAsStringAsync();

                    var pagina = JsonSerializer.Deserialize<
                        List<VwInventarioProducto>>(
                            contenido,
                            OpcionesJson)
                        ?? new List<VwInventarioProducto>();

                    resultado.Productos.AddRange(pagina);

                    if (pagina.Count < tamanoPagina)
                        break;

                    desplazamiento += tamanoPagina;
                }
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException ||
                ex is UriFormatException)
            {
                resultado.Productos.Clear();

                resultado.Error =
                    "No se pudieron cargar los productos.";

                _logger.LogError(
                    ex,
                    "Error al consultar la VIEW de inventario");
            }

            return resultado;
        }

        // OBTENER MOVIMIENTOS REALES

        private async Task<
            (List<VwInventarioMovimiento> Movimientos, string? Error)>
            ConsultarMovimientosAsync()
        {
            var respuesta = await EjecutarRpcAsync(
                "inv_listar_movimientos",
                new
                {
                    p_limite = 8,
                    p_id_sucursal =
                        _config.GetValue<int?>(
                            "Inventario:IdSucursal")
                });

            if (!respuesta.Correcto)
            {
                return (
                    new List<VwInventarioMovimiento>(),
                    respuesta.Error
                );
            }

            try
            {
                var movimientos = JsonSerializer.Deserialize<
                    List<VwInventarioMovimiento>>(
                        respuesta.Contenido,
                        OpcionesJson)
                    ?? new List<VwInventarioMovimiento>();

                return (movimientos, null);
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Error interpretando los movimientos.");

                return (
                    new List<VwInventarioMovimiento>(),
                    "No se pudo leer el historial de movimientos."
                );
            }
        }

        private static ProductoInventarioItem ConvertirProducto(
            VwInventarioProducto p)
        {
            return new ProductoInventarioItem
            {
                Id = p.IdDetalleInventario,
                IdProducto = p.IdProducto,
                IdInventario = p.IdInventario,
                IdSucursal = p.IdSucursal,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Categoria = p.Categoria,
                Marca = p.Marca ?? "",
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Precio = p.Precio,
                Estado = p.Estado
            };
        }

        // PANEL DE INVENTARIO

        [HttpGet]
        public async Task<IActionResult> PanelInventario()
        {
            var resultado = await ConsultarProductosAsync();
            var productos = resultado.Productos;

            var sucursales = productos
                .Select(p => p.IdSucursal)
                .Distinct()
                .ToList();

            var inventarios = productos
                .Select(p => p.IdInventario)
                .Distinct()
                .ToList();

            var alertas = productos
                .Where(p =>
                    p.StockActual <= 0 ||
                    p.StockActual < p.StockMinimo)
                .OrderBy(p => p.StockActual)
                .Take(3)
                .Select(p =>
                    new InventarioPanelInventario.AlertaInventario
                    {
                        IdProducto = p.IdProducto,
                        NombreProducto = p.Nombre,
                        Cantidad = p.StockActual,
                        Precio = p.Precio,
                        NivelAlerta = p.Estado
                    })
                .ToList();

            var categorias = productos
                .GroupBy(p => p.Categoria)
                .OrderBy(g => g.Key)
                .Select((grupo, indice) =>
                    new InventarioPanelInventario.CategoriaInventario
                    {
                        IdCategoria = indice + 1,
                        Categoria = grupo.Key,
                        CantidadProductos = grupo.Count(),
                        TotalUnidades = grupo.Sum(
                            p => p.StockActual)
                    })
                .ToList();

            var consultaMovimientos =
                await ConsultarMovimientosAsync();

            var movimientosPanel = consultaMovimientos.Movimientos
                .Select(m =>
                    new InventarioPanelInventario.MovimientoInventario
                    {
                        IdMovimiento = (int)Math.Min(
                            m.IdMovimiento,
                            int.MaxValue),

                        Fecha = m.FechaMovimiento.LocalDateTime,
                        Producto = m.NombreProducto,
                        Tipo = m.TipoMovimiento,
                        Cantidad = m.Cantidad,

                        Responsable =
                            string.IsNullOrWhiteSpace(m.RegistradoPor)
                                ? "No especificado"
                                : m.RegistradoPor
                    })
                .ToList();

            string nombreSucursal;

            if (!string.IsNullOrWhiteSpace(resultado.Error))
            {
                nombreSucursal = "Inventario no cargado";
            }
            else if (sucursales.Count == 1)
            {
                nombreSucursal =
                    $"Sucursal {sucursales[0]}";
            }
            else if (sucursales.Count > 1)
            {
                nombreSucursal =
                    "Todas las sucursales";
            }
            else
            {
                nombreSucursal = "Sin registros";
            }

            var modelo = new InventarioPanelInventario
            {
                IdSucursal = sucursales.Count == 1
                    ? sucursales[0]
                    : 0,

                NombreSucursal = nombreSucursal,
                Direccion = "",

                IdInventario = inventarios.Count == 1
                    ? inventarios[0]
                    : null,

                Encargado = sucursales.Count == 1
                    ? productos
                        .Select(p => p.Encargado)
                        .FirstOrDefault(e =>
                            !string.IsNullOrWhiteSpace(e))
                    : null,

                UltimaActualizacion = productos.Count > 0
                    ? productos.Max(
                        p => p.UltimaActualizacion)
                    : null,

                TotalProductos = productos
                    .Select(p => p.IdProducto)
                    .Distinct()
                    .Count(),

                ProductosEnDetalle = productos.Count,

                TotalUnidades = productos.Sum(
                    p => p.StockActual),

                StockBajo = productos.Count(p =>
                    p.StockActual > 0 &&
                    p.StockActual < p.StockMinimo),

                SinStock = productos.Count(p =>
                    p.StockActual <= 0),

                ValorTotal = productos.Sum(
                    p => p.StockActual * p.Precio),

                AlertasJson = JsonSerializer.Serialize(alertas),
                CategoriasJson = JsonSerializer.Serialize(categorias),
                MovimientosJson = JsonSerializer.Serialize(
                    movimientosPanel)
            };

            ViewBag.MensajeError =
                resultado.Error ?? consultaMovimientos.Error;

            return View(modelo);
        }

        // GESTIÓN DE PRODUCTOS

        [HttpGet]
        public async Task<IActionResult> GestionProductos(
            string busqueda = "",
            string categoria = "Todos",
            string ordenarPor = "Nombre",
            int pagina = 1,
            bool mostrarFormulario = false,
            int? editarId = null,
            int? detalleId = null)
        {
            return await CargarGestionAsync(
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

        private async Task<IActionResult> CargarGestionAsync(
            string busqueda,
            string categoria,
            string ordenarPor,
            int pagina,
            bool mostrarFormulario,
            int? editarId,
            int? detalleId,
            ProductoInventarioItem? formulario,
            string? mensajeError)
        {
            var resultado = await ConsultarProductosAsync();

            var todos = resultado.Productos
                .Select(ConvertirProducto)
                .ToList();

            var categorias = todos
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToArray();

            busqueda = (busqueda ?? "").Trim();

            categoria = string.IsNullOrWhiteSpace(categoria)
                ? "Todos"
                : categoria;

            ordenarPor = string.IsNullOrWhiteSpace(ordenarPor)
                ? "Nombre"
                : ordenarPor;

            IEnumerable<ProductoInventarioItem> consulta = todos;

            if (!categoria.Equals(
                "Todos",
                StringComparison.OrdinalIgnoreCase))
            {
                consulta = consulta.Where(p =>
                    p.Categoria.Equals(
                        categoria,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase) ||
                    p.Marca.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase) ||
                    p.Codigo.Contains(
                        busqueda,
                        StringComparison.OrdinalIgnoreCase));
            }

            consulta = ordenarPor switch
            {
                "NombreDesc" =>
                    consulta.OrderByDescending(p => p.Nombre),

                "PrecioAsc" =>
                    consulta.OrderBy(p => p.Precio),

                "PrecioDesc" =>
                    consulta.OrderByDescending(p => p.Precio),

                "StockAsc" =>
                    consulta.OrderBy(p => p.StockActual),

                "StockDesc" =>
                    consulta.OrderByDescending(p => p.StockActual),

                _ => consulta.OrderBy(p => p.Nombre)
            };

            var filtrados = consulta.ToList();
            const int porPagina = 7;

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    filtrados.Count / (double)porPagina));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaProductos = filtrados
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToList();

            ViewBag.CategoriasDisponibles = categorias;
            ViewBag.Busqueda = busqueda;
            ViewBag.Categoria = categoria;
            ViewBag.OrdenarPor = ordenarPor;

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalFiltrados = filtrados.Count;

            ViewBag.MostrarDesde = filtrados.Count == 0
                ? 0
                : (pagina - 1) * porPagina + 1;

            ViewBag.MostrarHasta = Math.Min(
                pagina * porPagina,
                filtrados.Count);

            ViewBag.TotalProductos = todos.Count;

            ViewBag.StockBajo = todos.Count(p =>
                p.StockActual > 0 &&
                p.StockActual < p.StockMinimo);

            ViewBag.SinStock = todos.Count(p =>
                p.StockActual <= 0);

            ViewBag.UltimaActualizacion =
                resultado.Productos.Count > 0
                    ? resultado.Productos.Max(
                        p => p.UltimaActualizacion)
                    : null;

            ViewBag.FormularioAbierto = mostrarFormulario;

            ViewBag.ProductoFormulario = formulario ??
                (editarId.HasValue
                    ? todos.FirstOrDefault(p =>
                        p.Id == editarId.Value)
                    : new ProductoInventarioItem
                    {
                        Categoria =
                            categorias.FirstOrDefault() ?? ""
                    });

            ViewBag.ProductoDetalle = detalleId.HasValue
                ? todos.FirstOrDefault(p =>
                    p.Id == detalleId.Value)
                : null;

            ViewBag.Mensaje = TempData["Mensaje"];

            ViewBag.MensajeError =
                mensajeError
                ?? TempData["Error"] as string
                ?? resultado.Error;

            if (editarId.HasValue &&
                ViewBag.ProductoFormulario == null)
            {
                ViewBag.FormularioAbierto = false;
                ViewBag.ProductoFormulario =
                    new ProductoInventarioItem();

                ViewBag.MensajeError = resultado.Error
                    ?? "No se encontró el producto.";
            }

            return View(
                "GestionProductos",
                paginaProductos);
        }

        // AGREGAR Y EDITAR

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarProducto(
            ProductoInventarioItem producto,
            string busquedaRegreso = "",
            string categoriaRegreso = "Todos",
            string ordenarPorRegreso = "Nombre",
            int paginaRegreso = 1)
        {
            if (!ModelState.IsValid)
            {
                return await CargarGestionAsync(
                    busquedaRegreso,
                    categoriaRegreso,
                    ordenarPorRegreso,
                    paginaRegreso,
                    true,
                    producto.Id > 0
                        ? producto.Id
                        : (int?)null,
                    null,
                    producto,
                    "Revisa los datos del formulario.");
            }

            int? idInventario =
                _config.GetValue<int?>(
                    "Inventario:IdInventario");

            if (producto.Id == 0 && !idInventario.HasValue)
            {
                // Si solo hay un inventario, lo identifica
                // automáticamente. Si hay varios, no elige
                // una sucursal incorrecta.
                var consulta = await ConsultarProductosAsync();

                var inventarios = consulta.Productos
                    .Select(p => p.IdInventario)
                    .Distinct()
                    .ToList();

                if (inventarios.Count == 1)
                {
                    idInventario = inventarios[0];
                }
            }

            var respuesta = await EjecutarRpcAsync(
                "inv_guardar_producto",
                new
                {
                    p_id_detalle = producto.Id,
                    p_nombre = producto.Nombre,
                    p_categoria = producto.Categoria,
                    p_marca = producto.Marca,
                    p_stock_actual = producto.StockActual,
                    p_stock_minimo = producto.StockMinimo,
                    p_precio = producto.Precio,
                    p_id_inventario = idInventario
                });

            if (!respuesta.Correcto)
            {
                return await CargarGestionAsync(
                    busquedaRegreso,
                    categoriaRegreso,
                    ordenarPorRegreso,
                    paginaRegreso,
                    true,
                    producto.Id > 0
                        ? producto.Id
                        : (int?)null,
                    null,
                    producto,
                    respuesta.Error);
            }

            TempData["Mensaje"] = producto.Id > 0
                ? "Producto actualizado en Supabase."
                : "Producto agregado a Supabase.";

            return RedirectToAction(
                nameof(GestionProductos));
        }

        // ELIMINAR SELECCIONADOS
        // Retira los detalles del inventario activo,
        // conservando los movimientos históricos.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductos(
            List<int> ids,
            string busquedaRegreso = "",
            string categoriaRegreso = "Todos",
            string ordenarPorRegreso = "Nombre",
            int paginaRegreso = 1)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["Error"] =
                    "Selecciona al menos un producto.";
            }
            else
            {
                var respuesta = await EjecutarRpcAsync(
                    "inv_archivar_varios",
                    new
                    {
                        p_ids = ids
                            .Where(id => id > 0)
                            .Distinct()
                            .ToArray()
                    });

                if (respuesta.Correcto)
                {
                    TempData["Mensaje"] =
                        "Productos retirados del inventario.";
                }
                else
                {
                    TempData["Error"] = respuesta.Error;
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
    }
}
