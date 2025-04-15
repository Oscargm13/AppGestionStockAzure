using AppGestionStock.Models;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class ProveedoresController : Controller
    {
        private ServiceAlmacenes service;

        public ProveedoresController(ServiceAlmacenes service)
        {
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            List<Proveedor> proveedores = await this.service.GetProveedoresAsync();
            return View(proveedores);
        }

        public IActionResult ProveedorCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProveedorCreate(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                await this.service.CreateProveedorAsync(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public async Task<IActionResult> ProveedorEdit(int id)
        {
            Proveedor proveedor = await this.service.FindProveedorAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return View(proveedor);
        }

        [HttpPost]
        public async Task<IActionResult> ProveedorEdit(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                await this.service.UpdateProveedorAsync(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public async Task<IActionResult> ProveedorDelete(int id)
        {
            await this.service.DeleteProveedorAsync(id);
            return RedirectToAction("Index");
        }
    }
}