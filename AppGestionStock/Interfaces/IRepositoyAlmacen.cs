using AppGestionStock.Data;
using System.Data;
using System.Xml.Linq;
using AppGestionStock.Models;
using Microsoft.Data.SqlClient;

namespace AppGestionStock.Interfaces
{
    public interface IRepositoyAlmacen
    {
        //Clientes
        #region
        Task<List<Cliente>> GetClientes();
        Task<List<Proveedor>> GetProveedores();
        Task<Cliente> FindCliente(int id);
        Task CreateCliente(Cliente cliente);
        Task UpdateCliente(Cliente cliente);
        Task DeleteCliente(int id);
        Task<Proveedor> FindProveedor(int id);
        Task CreateProveedor(Proveedor proveedor);
        Task UpdateProveedor(Proveedor proveedor);
        Task DeleteProveedor(int id);
        #endregion
        //Productos
        #region
        List<Producto> GetProductos();
        Task<List<Producto>> GetProductosProveedor(int proveedorId);
        List<VistaProductoTienda> GetAllVistaProductosTienda();
        List<VistaProductoTienda> GetVistaProductosTienda(int idTienda);
        Task<List<VistaProductoTienda>> GetVistaProductosTiendaConStockBajo();
        List<ProductosTienda> GetProductosTiendaGerente(int idGerente);
        List<VistaProductosGerente> GetProductosGerente(int idUsuarioGerente);
        int GetTotalStockGerente(int idUsuarioGerente);
        VistaProductoTienda FindProductoTienda(int idProducto, int idTienda);
        Task<Producto> FindProductoAsync(int idProducto);
        List<VistaProductosGerente> FindProductoManager(int idProducto, int idUsuarioGerente);
        List<Producto> findProductosCategoria(int idCategoria);
        void CrearProducto(string nombreProducto, decimal precio, decimal coste, string nombreCategoria, int? idCategoriaPadre, string imagen);
        Task UpdateProductoAsync(int idProducto, string nombreProducto, decimal precio, decimal coste, int idCategoria, string imagen);
        Task EliminarProducto(int idProducto);
        Task<List<Categoria>> GetCategoriasAsync();
        Producto GetProductoPorId(int productoId);
        #endregion
        //Inventario
        #region
        Task<List<VistaInventarioDetalladoVenta>> GetMovimientos();
        Task<List<Notificacion>> GetNotificaciones();
        Task<bool> ExisteNotificacion(int idProducto, int idTienda, AlmacenesContext context);
        Task CrearNotificacion(Notificacion notificacion, AlmacenesContext context);
        Task ProcesarVenta(Venta venta, List<DetallesVenta> detalles);
        Task ProcesarCompra(Compra compra, List<DetallesCompra> detalles);
        Task<decimal> GetIngresosMes(int mes, int year);
        Task<DetallesVenta> GetDetallesVenta(int idDetallesVenta);
        Task DeleteNotificacion(int idNotificacion);
        Task<List<Venta>> GetVentas();
        Task<List<Compra>> GetCompras();
        #endregion
        //Usuario
        #region
        Task<List<Usuario>> GetUsuariosAsync();
        Task<List<Rol>> GetRoles();
        Task PostUsuario(string nombre, string email, string pass, int idRole);
        Task<Usuario> CompararUsuario(string nombreUsuario, string password);
        Task<Usuario> findUsuario(int idUsuario);
        #endregion
        //Tiendas
        #region
        List<Tienda> GetTiendas();
        Tienda FindTienda(int idTienda);
        void CrearTienda(int idTienda, string nombre, string direccion, string telefono, string email);
        #endregion
    }
}
