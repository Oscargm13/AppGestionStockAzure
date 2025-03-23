using System.Data;
using System.Xml.Linq;
using AppGestionStock.Data;
using AppGestionStock.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

#region
/*
CREATE PROCEDURE ProcesarCompra
    @FechaCompra DATETIME,
    @IdProveedor INT,
    @IdTienda INT,
    @IdUsuario INT,
    @ImporteTotal DECIMAL(18, 2),
    @DetallesCompra XML
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Insertar la compra
        INSERT INTO Compras (FechaCompra, IdProveedor, IdTienda, IdUsuario, ImporteTotal)
        VALUES (@FechaCompra, @IdProveedor, @IdTienda, @IdUsuario, @ImporteTotal);

        DECLARE @IdCompra INT = SCOPE_IDENTITY();

        -- 2. Insertar los detalles de compra
        INSERT INTO DetallesCompra (IdCompra, IdProducto, Cantidad, PrecioUnidad)
        SELECT @IdCompra,
               Detalles.value('(IdProducto)[1]', 'INT'),
               Detalles.value('(Cantidad)[1]', 'INT'),
               Detalles.value('(PrecioUnidad)[1]', 'DECIMAL(18, 2)')
        FROM @DetallesCompra.nodes('/Detalles/Detalle') AS Detalles(Detalles);

        -- 3. Actualizar el inventario (entrada en vez de salida)
        INSERT INTO Inventario (IdProducto, FechaMovimiento, TipoMovimiento, Cantidad, IdMovimiento)
        SELECT Detalles.value('(IdProducto)[1]', 'INT'),
               @FechaCompra,
               'Entrada',
               Detalles.value('(Cantidad)[1]', 'INT'),
               @IdCompra
        FROM @DetallesCompra.nodes('/Detalles/Detalle') AS Detalles(Detalles);

        -- 4. Actualizar el stock en ProductosTienda (incrementando en vez de decrementando)
        UPDATE ProductosTienda
        SET Cantidad = pt.Cantidad + Detalles.value('(Cantidad)[1]', 'INT')
        FROM ProductosTienda pt
        INNER JOIN @DetallesCompra.nodes('/Detalles/Detalle') AS Detalles(Detalles)
            ON pt.IdProducto = Detalles.value('(IdProducto)[1]', 'INT')
        WHERE pt.IdTienda = @IdTienda;

        -- 5. Manejar casos donde el producto no existe en la tienda todavía
        INSERT INTO ProductosTienda (IdProducto, IdTienda, Cantidad)
        SELECT 
            Detalles.value('(IdProducto)[1]', 'INT'),
            @IdTienda,
            Detalles.value('(Cantidad)[1]', 'INT')
        FROM @DetallesCompra.nodes('/Detalles/Detalle') AS Detalles(Detalles)
        WHERE NOT EXISTS (
            SELECT 1 FROM ProductosTienda 
            WHERE IdProducto = Detalles.value('(IdProducto)[1]', 'INT')
            AND IdTienda = @IdTienda
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Guardar información del error para diagnóstico
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH;
END;
 */
#endregion

namespace AppGestionStock.Repositories
{
    public class RepositoryInventario
    {
        private AlmacenesContext context;

        public RepositoryInventario(AlmacenesContext context)
        {
            this.context = context;
        }

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
    }
}
