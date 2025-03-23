using AppGestionStock.Models;
using AppGestionStock.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class ProveedoresController : Controller
    {
        private RepositoryClientes repo;

        public ProveedoresController(RepositoryClientes repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<Proveedor> proveedores = await this.repo.GetProveedores();
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
                await this.repo.CreateProveedor(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public async Task<IActionResult> ProveedorEdit(int id)
        {
            Proveedor proveedor = await this.repo.FindProveedor(id);
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
                await this.repo.UpdateProveedor(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public async Task<IActionResult> ProveedorDelete(int id)
        {
            await this.repo.DeleteProveedor(id);
            return RedirectToAction("Index");
        }
    }
}