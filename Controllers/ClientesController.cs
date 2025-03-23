using AppGestionStock.Models;
using AppGestionStock.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class ClientesController : Controller
    {
        private RepositoryClientes repo;

        public ClientesController(RepositoryClientes repo)
        {
            this.repo = repo;
        }
        //CLIENTES

        public async Task<IActionResult> Clientes()
        {
            List<Cliente> clientes = await this.repo.GetClientes();
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
                await this.repo.CreateCliente(cliente);
                return RedirectToAction("Clientes");
            }
            return View(cliente);
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            Cliente cliente = await this.repo.FindCliente(id);
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
                await this.repo.UpdateCliente(cliente);
                return RedirectToAction("Clientes");
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await this.repo.DeleteCliente(id);
            return RedirectToAction("Clientes");
        }
    }
}
