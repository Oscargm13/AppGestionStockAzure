using System.Diagnostics;
using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ServiceAlmacenes service;

        public HomeController(ILogger<HomeController> logger, ServiceAlmacenes service)
        {
            _logger = logger;
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            List<Tienda> tiendas = await this.service.GetTiendasAsync();
            ViewData["TIENDAS"] = tiendas;
            //Calculo de clientes
            List<Cliente> clientes = await this.service.GetClientesAsync();
            int numeroClientes = clientes.Count();
            ViewData["NUMEROCLIENTES"] = numeroClientes;


            // Cálculo del stock total
            var usuario = HttpContext.Session.GetObject<Usuario>("USUARIO");
            int stockTotalGerente = await this.service.GetTotalStockGerenteAsync(usuario.IdUsuario);
            ViewData["STOCKTOTAL"] = stockTotalGerente;

            // Cálculo de los ingresos mensuales
            int mesActual = DateTime.Now.Month;
            int añoActual = DateTime.Now.Year;
            decimal ingresosMensuales = await service.GetIngresosMesAsync(mesActual, añoActual);
            ViewData["INGRESOSMENSUALES"] = ingresosMensuales;
            ViewData["MESACTUAL"] = mesActual;
            ViewData["AÑOACTUAL"] = añoActual;

            // Obtener la lista de movimientos de inventario y almacenarlos en la sesión
            List<VistaInventarioDetalladoVenta> inventario = await this.service.GetMovimientosAsync();
            HttpContext.Session.SetObject("INVENTARIO", inventario);

            // Obtener notificaciones en caso de haberlas
            List<Notificacion> notificaciones = await service.GetNotificacionesAsync();
            //ViewData["NOTIFICACIONES"] = notificaciones;
            HttpContext.Session.SetObject("NOTIFICACIONES", notificaciones);

            // Obtener ventas y compras
            List<Venta> ventas = await this.service.GetVentasAsync();
            List<Compra> compras = await this.service.GetComprasAsync();

            // Agrupar ventas por mes
            var ventasMensuales = ventas
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(v => v.ImporteTotal) })
                .OrderBy(g => g.Mes)
                .ToList();

            // Agrupar compras por mes
            var comprasMensuales = compras
                .GroupBy(c => c.FechaCompra.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(c => c.ImporteTotal) })
                .OrderBy(g => g.Mes)
                .ToList();

            // Obtener listas de meses, montos de ventas y compras
            List<int> meses = ventasMensuales.Select(v => v.Mes).ToList();
            List<decimal> montosVentas = ventasMensuales.Select(v => v.Total).ToList();
            List<decimal> montosCompras = comprasMensuales.Select(c => c.Total).ToList();

            ViewData["MESES"] = meses;
            ViewData["MONTOS_VENTAS"] = montosVentas;
            ViewData["MONTOS_COMPRAS"] = montosCompras;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
