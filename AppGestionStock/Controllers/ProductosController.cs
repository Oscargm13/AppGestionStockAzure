using AppGestionStock.Models;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppGestionStock.Controllers
{
    public class ProductosController : Controller
    {
        private ServiceAlmacenes service;
        public ProductosController(ServiceAlmacenes service)
        {
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            List<Producto> productos = await this.service.GetProductosAsync();
            return View(productos);
        }
        [HttpPost]
        public async Task<IActionResult> Index(int idTienda)
        {
            List<VistaProductoTienda> productos = await this.service.GetVistaProductosTiendaAsync(idTienda);
            return View(productos);
        }

        public async Task<IActionResult> ProductosTienda()
        {
            List<VistaProductoTienda> productos = await this.service.GetAllVistaProductosTiendaAsync();
            List<Tienda> tiendas = await this.service.GetTiendasAsync();

            // Agregar la opción "Todas las Tiendas"
            tiendas.Insert(0, new Tienda { IdTienda = 0, Nombre = "Todas las Tiendas" });

            ViewData["Tiendas"] = new SelectList(tiendas, "IdTienda", "Nombre");

            return View(productos);
        }

        [HttpPost]
        public async Task<IActionResult> ProductosTienda(int idTienda)
        {
            List<VistaProductoTienda> productos;

            if (idTienda == 0)
            {
                productos = await this.service.GetAllVistaProductosTiendaAsync();
            }
            else
            {
                productos = await this.service.GetVistaProductosTiendaAsync(idTienda);
            }

            return PartialView("_ProductosTiendaPartial", productos);
        }

        public IActionResult ProductosManager()
        {
            List<VistaProductosGerente> productos = new List<VistaProductosGerente>();
            return View(productos);
        }

        [HttpPost]
        public async Task<IActionResult> ProductosManager(int idUsuario)
        {
            List<VistaProductosGerente> productos = await this.service.GetProductosGerenteAsync(idUsuario);
            return View(productos);
        }

        public async Task<IActionResult> CrearProducto()
        {
            List<Categoria> categorias = await this.service.GetCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto(int? idCategoria, string nombre, decimal precio, decimal coste,
            string? nombreCategoria, int? idCategoriaPadre, string imagen)
        {
            if (!idCategoria.HasValue && string.IsNullOrWhiteSpace(nombreCategoria))
            {
                ModelState.AddModelError("", "Debe seleccionar una categoría existente o introducir una nueva.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["CATEGORIAS"] = await this.service.GetCategoriasAsync();
                return View(new Producto
                {
                    Nombre = nombre,
                    Precio = precio,
                    Coste = coste,
                    Imagen = imagen
                });
            }

            var producto = new Producto
            {
                IdCategoria = idCategoria ?? 0,
                Nombre = nombre,
                Precio = precio,
                Coste = coste,
                Imagen = imagen
            };

            await this.service.CreateProductoAsync(producto, nombreCategoria, idCategoriaPadre);

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> UpdateProducto(int idProducto)
        {
            Producto producto = await this.service.FindProductoAsync(idProducto);
            return View(producto);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProducto(int idProducto, string nombre, decimal precio, decimal coste,
            int idCategoria, string imagen)
        {
            Producto producto = new Producto
            {
                Nombre = nombre,
                Precio = precio,
                Coste = coste,
                IdCategoria = idCategoria,
                Imagen = imagen
            };

            await this.service.UpdateProductoAsync(idProducto, producto);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EliminarProducto(int idProducto)
        {
            await this.service.DeleteProductoAsync(idProducto);
            return RedirectToAction("Index");
        }
    }
}
