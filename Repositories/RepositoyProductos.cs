using AppGestionStock.Data;
using AppGestionStock.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace AppGestionStock.Repositories
{
    public class RepositoyProductos
    {
        private AlmacenesContext context;
        public RepositoyProductos(AlmacenesContext context)
        {
            this.context = context;
        }

        public List<Producto> GetProductos()
        {
            var consulta = from datos in this.context.Productos select datos;
            return consulta.ToList();
        }

        public async Task<List<Producto>> GetProductosProveedor(int proveedorId)
        {
            var consulta = from producto in this.context.Productos
                           join productoProveedor in this.context.ProductosProveedores
                               on producto.IdProducto equals productoProveedor.IdProducto
                           where productoProveedor.IdProveedor == proveedorId
                           select producto;

            return await consulta.ToListAsync();
        }

        public List<VistaProductoTienda> GetAllVistaProductosTienda()
        {
            var consulta = from datos in this.context.VistaProductosTienda
                           select datos;
            return consulta.ToList();
        }

        public List<VistaProductoTienda> GetVistaProductosTienda(int idTienda)
        {
            var consulta = from datos in this.context.VistaProductosTienda
                           where datos.IdTienda == idTienda
                           select datos;
            return consulta.ToList();
        }

        public async Task<List<VistaProductoTienda>> GetVistaProductosTiendaConStockBajo()
        {
            return await context.VistaProductosTienda
                .Where(vp => vp.StockTienda < 15)
                .ToListAsync();
        }

        public List<ProductosTienda> GetProductosTiendaGerente(int idGerente)
        {
            var consulta = from pt in this.context.ProductosTienda
                           join t in this.context.Tiendas on pt.IdTienda equals t.IdTienda
                           join mt in this.context.ManagerTiendas on t.IdTienda equals mt.IdTienda
                           where mt.IdUsuario == idGerente
                           select pt;

            return consulta.ToList();
        }

        public List<VistaProductosGerente> GetProductosGerente(int idUsuarioGerente)
        {
            var consulta = from datos in this.context.VistaProductosGerente
                           join managers in this.context.ManagerTiendas
                           on datos.IdTienda equals managers.IdTienda
                           where managers.IdUsuario == idUsuarioGerente
                           select datos;
            return consulta.ToList();
        }

        public int GetTotalStockGerente(int idUsuarioGerente)
        {
            var consulta = (from datos in this.context.VistaProductosGerente
                            join managers in this.context.ManagerTiendas
                            on datos.IdTienda equals managers.IdTienda
                            where managers.IdUsuario == idUsuarioGerente
                            select datos.StockTienda).Sum();

            return consulta;
        }

        public VistaProductoTienda FindProductoTienda(int idProducto, int idTienda)
        {
            var consulta = (from datos in this.context.VistaProductosTienda
                            where datos.IdProducto == idProducto && datos.IdTienda == idTienda
                            select datos).FirstOrDefault();
            return consulta;
        }

        public async Task<Producto> FindProductoAsync(int idProducto)
        {
            var consulta = (from datos in this.context.Productos
                            where datos.IdProducto == idProducto
                            select datos);
            return await consulta.FirstOrDefaultAsync();
        }

        public List<VistaProductosGerente> FindProductoManager(int idProducto, int idUsuarioGerente)
        {
            var consulta = from datos in this.context.VistaProductosGerente
                           join managers in this.context.ManagerTiendas
                           on datos.IdTienda equals managers.IdTienda
                           where managers.IdUsuario == idUsuarioGerente && datos.IdProducto == idProducto
                           select datos;
            return consulta.ToList();
        }

        public List<Producto> findProductosCategoria(int idCategoria)
        {
            var consulta = from productos in this.context.Productos
                           where productos.IdCategoria == idCategoria
                           select productos;
            return consulta.ToList();
        }

        public void CrearProducto(string nombreProducto, decimal precio, decimal coste, string nombreCategoria, int? idCategoriaPadre, string imagen)
        {
            // 1. Crear o encontrar la categoría
            Categoria categoria = this.context.Categorias.FirstOrDefault(c => c.Nombre == nombreCategoria);

            if (categoria == null)
            {
                categoria = new Categoria
                {
                    Nombre = nombreCategoria,
                    IdCategoriaPadre = idCategoriaPadre
                };

                this.context.Categorias.Add(categoria);
                this.context.SaveChanges();
            }

            // 2. Crear el producto
            Producto nuevoProducto = new Producto
            {
                Nombre = nombreProducto,
                Precio = precio,
                Coste = coste,
                IdCategoria = categoria.IdCategoria,
                Imagen = imagen
            };

            this.context.Productos.Add(nuevoProducto);
            this.context.SaveChanges();
        }

        public async Task UpdateProductoAsync(int idProducto, string nombreProducto, decimal precio, decimal coste, int idCategoria, string imagen)
        {
            try
            {
                Producto productoExistente = await this.context.Productos.FindAsync(idProducto);

                if (productoExistente != null)
                {
                    productoExistente.Nombre = nombreProducto;
                    productoExistente.Precio = precio;
                    productoExistente.Coste = coste;
                    productoExistente.IdCategoria = idCategoria;
                    productoExistente.Imagen = imagen;

                    this.context.Productos.Update(productoExistente);
                    await this.context.SaveChangesAsync();
                }
                else
                {
                    // Manejar el caso en que el producto no existe
                    throw new Exception($"No se encontró el producto con ID {idProducto}");
                }
            }
            catch (Exception ex)
            {
                // Manejar el error (por ejemplo, registrarlo, lanzar una excepción personalizada)
                Console.WriteLine($"Error al actualizar el producto: {ex.Message}");
                throw; // Re-lanzar la excepción para que el controlador pueda manejarla
            }
        }

        public async Task EliminarProducto(int idProducto)
        {
            Producto productoAEliminar = await this.context.Productos.FindAsync(idProducto);
            
            this.context.Productos.Remove(productoAEliminar);
            await this.context.SaveChangesAsync();
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var consulta = from datos in this.context.Categorias select datos;
            return await consulta.ToListAsync();
        }

        public Producto GetProductoPorId(int productoId)
        {
            return context.Productos.FirstOrDefault(p => p.IdProducto == productoId);
        }
    }
}
