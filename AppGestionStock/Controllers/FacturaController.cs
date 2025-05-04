using AppGestionStock.DTOs;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class FacturaController : Controller
    {
        private readonly FacturaRepository repo;
        private readonly ServiceAlmacenes service;

        public FacturaController(ServiceAlmacenes service)
        {
            var keyVaultUrl = "https://keyvaultalmacenes.vault.azure.net/";
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

            KeyVaultSecret secret = secretClient.GetSecret("intelligenceskey");
            string apiKey = secret.Value;

            var endpoint = "https://docreaderogm.cognitiveservices.azure.com/";

            this.repo = new FacturaRepository(endpoint, apiKey);
            this.service = service;
        }


        [HttpGet]
        public IActionResult Subir()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Subir(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Mensaje = "Archivo inválido.";
                return View();
            }

            using var stream = file.OpenReadStream();
            var factura = await this.repo.AnalizarFacturaAsync(stream);

            return View("Resultado", factura);
        }
        [HttpPost]
        public async Task<IActionResult> Procesar(FacturaReconocida model)
        {
            if (model == null || model.Productos == null || !model.Productos.Any())
            {
                TempData["Mensaje"] = "No se puede procesar la factura. Datos incompletos.";
                return RedirectToAction("Subir");
            }

            // Obtener IDs necesarios desde el servicio (puedes ajustar estos métodos)
            //int idProveedor = await service.ObtenerIdProveedorPorNombreAsync(model.Proveedor);
            //int idTienda = await service.ObtenerIdTiendaActivaDelUsuarioAsync();
            //int idUsuario = await service.ObtenerIdUsuarioActualAsync();

            var dto = new CompraConDetallesDto
            {
                FechaCompra = DateTime.Parse(model.Fecha),
                IdProveedor = 1,
                IdTienda = 1,
                IdUsuario = 3,
                ImporteTotal = model.ImporteTotal ?? 0,
                Detalles = new List<DetalleCompraDto>()
            };

            foreach (var p in model.Productos)
            {
                int idProducto = await service.FindIdProductoNombre(p.Descripcion);

                dto.Detalles.Add(new DetalleCompraDto
                {
                    IdProducto = idProducto,
                    Cantidad = p.Cantidad,
                    PrecioUnidad = p.PrecioUnitario
                });
            }

            await service.ProcesarCompraAsync(dto);

            TempData["Mensaje"] = "Factura guardada correctamente.";
            return RedirectToAction("Index", "Home");
        }

    }
}
