using AppGestionStock.Models;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class ClientesController : Controller
    {
        //private RepositoryAlmacen repo;
        private ServiceAlmacenes service;

        public ClientesController(RepositoryAlmacen repo, ServiceAlmacenes service)
        {
            //this.repo = repo;
            this.service = service;
        }
        //CLIENTES

        public async Task<IActionResult> Clientes()
        {
            List<Cliente> clientes = await this.service.GetClientesAsync();
            return View(clientes);
        }

        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await this.service.CreateClienteAsync(cliente);
                return RedirectToAction("Clientes");
            }
            return View(cliente);
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            Cliente cliente = await this.service.FindClienteAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await this.service.UpdateClienteAsync(cliente);
                return RedirectToAction("Clientes");
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await this.service.DeleteClienteAsync(id);
            return RedirectToAction("Clientes");
        }
    }
}
