using AppGestionStock.Models;
using Microsoft.EntityFrameworkCore;

namespace AppGestionStock.Data
{
    public class AlmacenesContext: DbContext
    {
        public AlmacenesContext(DbContextOptions<AlmacenesContext> options): base(options) { }
        //MODELS PRODUCTOS
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductosTienda> ProductosTienda { get; set; }
        public DbSet<VistaProductoTienda> VistaProductosTienda { get; set; }
        public DbSet<VistaProductosGerente> VistaProductosGerente { get; set; }
        public DbSet<ManagerTienda> ManagerTiendas { get; set; }
        public DbSet<ProductoProveedor> ProductosProveedores { get; set; }

        //MODELS CLIENTES
        public DbSet<Cliente> Clientes { get; set; }

        //MODELS TIENDA
        public DbSet<Tienda> Tiendas { get; set; }

        //MODELS CATEGORIAS
        public DbSet<Categoria> Categorias { get; set; }

        //MODELS ROLES
        public DbSet<Rol> Roles { get; set; }

        //MODELS USUARIOS
        public DbSet<Usuario> Usuarios { get; set; }

        //INVENTARIO
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<DetallesVenta> DetallesVenta { get; set; }
        public DbSet<VistaInventarioDetalladoVenta> vistaInventarioDetalladoVenta {get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Compra> Compras { get; set; }

        //PROVEEDORES
        public DbSet<Proveedor> Proveedores { get; set; }

        //NOTIFICACIONES
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VistaInventarioDetalladoVenta>()
                .HasOne(i => i.Producto)
                .WithMany()
                .HasForeignKey(i => i.IdProducto);
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Tienda)
                .WithMany()
                .HasForeignKey(v => v.IdTienda);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.IdUsuario);
        }
    }
}
