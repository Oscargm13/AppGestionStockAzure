using System.Data;
using System.Xml.Linq;
using AppGestionStock.Data;
using AppGestionStock.Interfaces;
using AppGestionStock.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppGestionStock.Repositories
{
    public class RepositoryAlmacen : IRepositoyAlmacen
    {
        private AlmacenesContext context;
        public RepositoryAlmacen(AlmacenesContext context)
        {
            this.context = context;
        }
        //Productos
        #region
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
        #endregion
        //Clientes y proveedores
        #region
        public async Task<List<Cliente>> GetClientes()
        {
            var consulta = from datos in this.context.Clientes select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Proveedor>> GetProveedores()
        {
            var consulta = from datos in this.context.Proveedores select datos;
            return await consulta.ToListAsync();
        }

        public async Task<Cliente> FindCliente(int id)
        {
            return await context.Clientes.FindAsync(id);
        }

        public async Task CreateCliente(Cliente cliente)
        {
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCliente(Cliente cliente)
        {
            context.Clientes.Update(cliente);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCliente(int id)
        {
            var cliente = await context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                context.Clientes.Remove(cliente);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Proveedor> FindProveedor(int id)
        {
            return await context.Proveedores.FindAsync(id);
        }

        public async Task CreateProveedor(Proveedor proveedor)
        {
            context.Proveedores.Add(proveedor);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProveedor(Proveedor proveedor)
        {
            context.Proveedores.Update(proveedor);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProveedor(int id)
        {
            var proveedor = await context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                context.Proveedores.Remove(proveedor);
                await context.SaveChangesAsync();
            }
        }
        #endregion
        //Inventario
        #region
        public async Task<List<VistaInventarioDetalladoVenta>> GetMovimientos()
        {
            return await this.context.vistaInventarioDetalladoVenta
                .Include(i => i.Producto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Notificacion>> GetNotificaciones()
        {
            return await context.Notificaciones.ToListAsync();
        }

        public async Task<bool> ExisteNotificacion(int idProducto, int idTienda, AlmacenesContext context)
        {
            return await context.Notificaciones
                .AnyAsync(n => n.IdProducto == idProducto && n.IdTienda == idTienda);
        }

        public async Task CrearNotificacion(Notificacion notificacion, AlmacenesContext context)
        {
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
        }

        public async Task ProcesarVenta(Venta venta, List<DetallesVenta> detalles)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {
                        command.Transaction = transaction.GetDbTransaction(); // Usa la transacción de EF Core
                        command.CommandText = "ProcesarVentaStock";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de la venta
                        command.Parameters.Add(new SqlParameter("@FechaVenta", venta.FechaVenta));
                        command.Parameters.Add(new SqlParameter("@IdTienda", venta.IdTienda));
                        command.Parameters.Add(new SqlParameter("@IdUsuario", venta.IdUsuario));
                        command.Parameters.Add(new SqlParameter("@ImporteTotal", venta.ImporteTotal));
                        command.Parameters.Add(new SqlParameter("@IdCliente", venta.IdCliente));

                        // Crear XML para los detalles de venta
                        var detallesXml = new XElement("Detalles",
                            detalles.Select(d => new XElement("Detalle",
                                new XElement("IdProducto", d.IdProducto),
                                new XElement("Cantidad", d.Cantidad),
                                new XElement("PrecioUnidad", d.PrecioUnidad)
                            ))
                        );

                        // Parámetro XML para los detalles de venta
                        command.Parameters.Add(new SqlParameter("@DetallesVenta", detallesXml.ToString()));

                        // Ejecutar el comando
                        await command.ExecuteNonQueryAsync();
                        command.Parameters.Clear();
                    }

                    // Verificar stock bajo y crear notificaciones (usando EF Core)
                    foreach (var detalle in detalles)
                    {
                        var producto = await context.ProductosTienda
                            .FirstOrDefaultAsync(pt => pt.IdProducto == detalle.IdProducto && pt.IdTienda == venta.IdTienda);

                        if (producto != null && producto.Cantidad < 10) // Umbral de stock bajo
                        {
                            var notificacionExistente = await this.ExisteNotificacion(detalle.IdProducto, venta.IdTienda, context);
                            if (!notificacionExistente)
                            {
                                var notificacion = new Notificacion
                                {
                                    Mensaje = $"Aviso de stock bajo: En {venta.IdTienda} la cantidad de {detalle.IdProducto} es de {producto.Cantidad}.",
                                    Fecha = DateTime.Now,
                                    IdProducto = detalle.IdProducto,
                                    IdTienda = venta.IdTienda
                                };
                                await CrearNotificacion(notificacion, context);
                            }
                        }
                    }

                    await transaction.CommitAsync(); // Confirmar la transacción de EF Core
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(); // Revertir la transacción de EF Core
                    throw; // Re-lanza la excepción para que se maneje en la capa superior
                }
            }
        }

        public async Task ProcesarCompra(Compra compra, List<DetallesCompra> detalles)
        {
            using (var connection = context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "ProcesoCompraNotificaciones";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@FechaCompra", compra.FechaCompra));
                    command.Parameters.Add(new SqlParameter("@IdProveedor", compra.IdProveedor));
                    command.Parameters.Add(new SqlParameter("@IdTienda", compra.IdTienda));
                    command.Parameters.Add(new SqlParameter("@ImporteTotal", compra.ImporteTotal));
                    command.Parameters.Add(new SqlParameter("@IdUsuario", compra.IdUsuario));

                    var detallesXml = new XElement("Detalles",
                        detalles.Select(d => new XElement("Detalle",
                            new XElement("IdProducto", d.IdProducto),
                            new XElement("Cantidad", d.Cantidad),
                            new XElement("PrecioUnidad", d.PrecioUnidad)
                        ))
                    );

                    command.Parameters.Add(new SqlParameter("@DetallesCompra", detallesXml.ToString()));

                    await command.ExecuteNonQueryAsync();
                    command.Parameters.Clear();
                }
                await connection.CloseAsync();
            }
        }

        public async Task<decimal> GetIngresosMes(int mes, int year)
        {
            decimal ingresos = 0;

            // Utilizar el DbContext inyectado
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "IngresosMes";
                command.CommandType = CommandType.StoredProcedure;

                // Parámetros de entrada
                command.Parameters.Add(new SqlParameter("@mes", mes));
                command.Parameters.Add(new SqlParameter("@año", year));

                // Parámetro de salida
                SqlParameter ingresosParameter = new SqlParameter("@ingresos", SqlDbType.Decimal);
                ingresosParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(ingresosParameter);

                // Ejecutar el comando
                await context.Database.OpenConnectionAsync(); // Abrir la conexión del DbContext
                await command.ExecuteNonQueryAsync();

                // Obtener el valor del parámetro de salida
                ingresos = (decimal)ingresosParameter.Value;
                command.Parameters.Clear();
            }

            return ingresos;
        }

        public async Task<DetallesVenta> GetDetallesVenta(int idDetallesVenta)
        {
            var consulta = (from datos in this.context.DetallesVenta
                            where datos.IdProducto == idDetallesVenta
                            select datos).FirstOrDefault();
            return consulta;
        }

        public async Task DeleteNotificacion(int idNotificacion)
        {
            var notificacion = await context.Notificaciones.FindAsync(idNotificacion);
        }

        public async Task<List<Venta>> GetVentas()
        {
            return await context.Ventas.ToListAsync();
        }

        public async Task<List<Compra>> GetCompras()
        {
            return await context.Compras.ToListAsync();
        }
        #endregion
        //Usuario
        #region
        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var consulta = from datos in this.context.Usuarios select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Rol>> GetRoles()
        {
            var consulta = from datos in this.context.Roles select datos;
            return await consulta.ToListAsync();
        }

        public async Task PostUsuario(string nombre, string email, string pass, int idRole)
        {
            try
            {
                // Crear un nuevo objeto Usuario
                var usuario = new Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Password = pass,
                    IdRol = idRole
                };

                // Agregar el usuario a la base de datos
                await this.context.Usuarios.AddAsync(usuario);

                // Guardar los cambios
                await this.context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejar errores (por ejemplo, registrar el error)
                Console.WriteLine($"Error al crear usuario: {ex.Message}");
                throw; // Re-lanzar la excepción para que el llamador pueda manejarla
            }
        }

        public async Task<Usuario> CompararUsuario(string nombreUsuario, string password)
        {
            List<Usuario> usuarios = await this.GetUsuariosAsync();
            foreach (Usuario usuario in usuarios)
            {
                if (usuario.Nombre == nombreUsuario && usuario.Password == password)
                {
                    return usuario;
                }
            }
            return null;
        }

        public async Task<Usuario> findUsuario(int idUsuario)
        {
            try
            {
                var usuario = await this.context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar usuario: {ex.Message}");
                return null;
            }
        }
        #endregion
        //Tienda
        #region
        public List<Tienda> GetTiendas()
        {
            var consulta = from datos in this.context.Tiendas select datos;
            return consulta.ToList();
        }

        public Tienda FindTienda(int idTienda)
        {
            var consulta = (from datos in this.context.Tiendas
                            where datos.IdTienda == idTienda
                            select datos).FirstOrDefault();
            return consulta;
        }

        public void CrearTienda(int idTienda, string nombre, string direccion, string telefono, string email)
        {
            Tienda nuevaTienda = new Tienda
            {
                IdTienda = idTienda,
                Nombre = nombre,
                Direccion = direccion,
                Telefono = telefono,
                Email = email
            };

            this.context.Tiendas.Add(nuevaTienda);
            this.context.SaveChanges();
        }
        #endregion
    }
}
