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
        List<Cliente> GetClientes();
        Cliente FindCliente(int id);
        void CreateCliente(Cliente cliente);
        void UpdateCliente(Cliente cliente);
        void DeleteCliente(int id);
        #endregion 
        //Proveedores
        #region
        List<Proveedor> GetProveedores();
        Proveedor FindProveedor(int id);
        void CreateProveedor(Proveedor proveedor);
        void UpdateProveedor(Proveedor proveedor);
        void DeleteProveedor(int id);
        #endregion
        //Productos
        #region
        List<Producto> GetProductos();
        List<Producto> GetProductosProveedor(int proveedorId);
        List<VistaProductoTienda> GetAllVistaProductosTienda();
        List<VistaProductoTienda> GetVistaProductosTienda(int idTienda);
        List<VistaProductoTienda> GetVistaProductosTiendaConStockBajo();
        List<ProductosTienda> GetProductosTiendaGerente(int idGerente);
        List<VistaProductosGerente> GetProductosGerente(int idUsuarioGerente);
        int GetTotalStockGerente(int idUsuarioGerente);
        VistaProductoTienda FindProductoTienda(int idProducto, int idTienda);
        Producto FindProductoAsync(int idProducto);
        List<VistaProductosGerente> FindProductoManager(int idProducto, int idUsuarioGerente);
        List<Producto> findProductosCategoria(int idCategoria);
        void CrearProducto(string nombreProducto, decimal precio, decimal coste, string nombreCategoria, int? idCategoriaPadre, string imagen);
        void UpdateProductoAsync(int idProducto, string nombreProducto, decimal precio, decimal coste, int idCategoria, string imagen);
        void EliminarProducto(int idProducto);
        List<Categoria> GetCategoriasAsync();
        Producto GetProductoPorId(int productoId);
        #endregion
        //Inventario
        #region
        List<VistaInventarioDetalladoVenta> GetMovimientos();
        List<Notificacion> GetNotificaciones();
        bool ExisteNotificacion(int idProducto, int idTienda, AlmacenesContext context);
        void CrearNotificacion(Notificacion notificacion, AlmacenesContext context);
        void ProcesarVenta(Venta venta, List<DetallesVenta> detalles);
        void ProcesarCompra(Compra compra, List<DetallesCompra> detalles);
        decimal GetIngresosMes(int mes, int year);
        DetallesVenta GetDetallesVenta(int idDetallesVenta);
        void DeleteNotificacion(int idNotificacion);
        List<Venta> GetVentas();
        List<Compra> GetCompras();
        #endregion
        //Usuario
        #region
        List<Usuario> GetUsuariosAsync();
        List<Rol> GetRoles();
        void PostUsuario(string nombre, string email, string pass, int idRole);
        Usuario CompararUsuario(string nombreUsuario, string password);
        Usuario findUsuario(int idUsuario);
        #endregion
        //Tiendas
        #region
        List<Tienda> GetTiendas();
        Tienda FindTienda(int idTienda);
        void CrearTienda(int idTienda, string nombre, string direccion, string telefono, string email);
        #endregion
    }
}
