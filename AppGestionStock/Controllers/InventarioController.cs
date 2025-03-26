using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using Microsoft.AspNetCore.Mvc;

#region
/*
 CREATE PROCEDURE IngresosMes
    @mes INT,
    @año INT,
    @ingresos DECIMAL(18, 2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ventas DECIMAL(18, 2);
    DECLARE @compras DECIMAL(18, 2);

    -- Obtener ingresos por ventas
    SELECT @ventas = ISNULL(SUM(ImporteTotal), 0)
    FROM Ventas
    WHERE MONTH(FechaVenta) = @mes AND YEAR(FechaVenta) = @año;

    -- Obtener gastos por compras
    SELECT @compras = ISNULL(SUM(ImporteTotal), 0)
    FROM Compras
    WHERE MONTH(FechaCompra) = @mes AND YEAR(FechaCompra) = @año;

    -- Calcular ingresos netos
    SET @ingresos = @ventas - @compras;
END;

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

namespace AppGestionStock.Controllers
{
    public class InventarioController : Controller
    {
        private RepositoryAlmacen repo;
        public InventarioController(RepositoryAlmacen repo)
        {
            this.repo = repo;
        }
        public async Task<IActionResult> Index()
        {
            //List<Inventario> inventario = await this.repo.GetMovimientos();
            return View();
        }

        public async Task<IActionResult> Venta()
        {
            List<Tienda> tiendas = this.repo.GetTiendas();
            ViewData["TIENDAS"] = tiendas;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Venta(Venta venta, string siguientePaso,
            List<int> idProducto, List<int> cantidad, List<decimal> precioUnidad)
        {
            if (!string.IsNullOrEmpty(siguientePaso) && siguientePaso == "true")
            {
                // Obtener los productos de la tienda seleccionada
                List<VistaProductoTienda> productos = this.repo.GetVistaProductosTienda(venta.IdTienda);
                ViewData["PRODUCTOS"] = productos;
                ViewData["TIENDAS"] = this.repo.GetTiendas(); // Asegura que las tiendas se envíen de nuevo

                // Pasar los datos de la primera parte del formulario a la vista
                return View("Venta", venta);
            }

            decimal importe = 0;
            if (cantidad != null && precioUnidad != null && idProducto != null &&
                cantidad.Count == precioUnidad.Count && cantidad.Count == idProducto.Count &&
                cantidad.Count > 0)
            {
                for (int i = 0; i < cantidad.Count; i++)
                {
                    importe += precioUnidad[i] * cantidad[i];
                }
            }
            else
            {
                return BadRequest("Las listas de cantidad, precioUnidad o idProducto son inválidas.");
            }

            // 1. Crear el objeto Venta
            venta.IdUsuario = HttpContext.Session.GetObject<Usuario>("USUARIO").IdUsuario;
            venta.ImporteTotal = importe;

            // 2. Crear la lista de DetallesVenta
            var detallesVenta = new List<DetallesVenta>();
            for (int i = 0; i < idProducto.Count; i++)
            {
                detallesVenta.Add(new DetallesVenta
                {
                    IdProducto = idProducto[i],
                    Cantidad = cantidad[i],
                    PrecioUnidad = precioUnidad[i]
                });
            }

            // 3. Llamar al repositorio para procesar la venta
            await this.repo.ProcesarVenta(venta, detallesVenta);
            ViewData["MensajeExito"] = "Venta registrada con exito";


            // 4. Retornar una respuesta exitosa
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Importante para seguridad CSRF
        public async Task<IActionResult> DeleteNotificacion([FromBody] DeleteNotificacionRequest request)
        {
            if (request == null || request.IdNotificacion <= 0)
            {
                return Json(new { success = false, error = "ID de notificación inválido." });
            }

            try
            {
                await repo.DeleteNotificacion(request.IdNotificacion); // Usa el ID del objeto request
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Compra()
        {
            List<Proveedor> proveedores = await repo.GetProveedores();
            List<Tienda> tiendas = repo.GetTiendas();

            ViewData["PROVEEDORES"] = proveedores;
            ViewData["TIENDAS"] = tiendas;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Compra(DateTime fechaCompra, int idProveedor, int idTienda,
            List<int> idProducto, List<int> cantidad, List<decimal> precioUnidad)
        {
            try
            {
                decimal importe = 0;
                if (cantidad != null && precioUnidad != null && idProducto != null &&
                    cantidad.Count == precioUnidad.Count && cantidad.Count == idProducto.Count &&
                    cantidad.Count > 0)
                {
                    for (int i = 0; i < cantidad.Count; i++)
                    {
                        importe += precioUnidad[i] * cantidad[i];
                    }
                }
                else
                {
                    return BadRequest("Las listas de cantidad, precioUnidad o idProducto son inválidas.");
                }

                // 1. Crear el objeto Compra
                var compra = new Compra
                {
                    FechaCompra = fechaCompra,
                    IdProveedor = idProveedor,
                    IdTienda = idTienda,
                    IdUsuario = HttpContext.Session.GetObject<Usuario>("USUARIO").IdUsuario, // Obtener el usuario de la sesión
                    ImporteTotal = importe
                };

                // 2. Crear la lista de DetallesCompra
                var detallesCompra = new List<DetallesCompra>();
                for (int i = 0; i < idProducto.Count; i++)
                {
                    detallesCompra.Add(new DetallesCompra
                    {
                        IdProducto = idProducto[i],
                        Cantidad = cantidad[i],
                        PrecioUnidad = precioUnidad[i]
                    });
                }


                // 3. Llamar al repositorio para procesar la compra
                await repo.ProcesarCompra(compra, detallesCompra);
                ViewData["MensajeExito"] = "Compra registrada con éxito";

                // 4. Retornar una respuesta exitosa
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // 5. Manejar errores
                return BadRequest($"Error al procesar la compra: {ex.Message}");
            }
        }
        public IActionResult GetProductosProveedor(int proveedorId)
        {
            var productosTask = repo.GetProductosProveedor(proveedorId);
            var productos = productosTask.Result;

            return Json(productos.Select(p => new { p.IdProducto, p.Nombre }));
        }

        public IActionResult GetCostoProducto(int productoId)
        {
            Producto producto = repo.GetProductoPorId(productoId);
            if (producto != null)
            {
                return Json(producto.Coste);
            }
            return Json(0);
        }
    }
}
