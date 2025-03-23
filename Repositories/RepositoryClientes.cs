using AppGestionStock.Data;
using AppGestionStock.Models;
using Microsoft.EntityFrameworkCore;

namespace AppGestionStock.Repositories
{
    public class RepositoryClientes
    {
        private AlmacenesContext context;
        public RepositoryClientes(AlmacenesContext context)
        {
            this.context = context;
        }

        public async Task<List<Cliente>> GetClientes()
        {
            var consulta = from datos in this.context.Clientes select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Proveedor>> GetProveedores()
        {
            var consulta = from datos in this.context.Proveedores select datos;
            return await consulta.ToListAsync();
        }

        public async Task<Cliente> FindCliente(int id)
        {
            return await context.Clientes.FindAsync(id);
        }

        public async Task CreateCliente(Cliente cliente)
        {
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCliente(Cliente cliente)
        {
            context.Clientes.Update(cliente);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCliente(int id)
        {
            var cliente = await context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                context.Clientes.Remove(cliente);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Proveedor> FindProveedor(int id)
        {
            return await context.Proveedores.FindAsync(id);
        }

        public async Task CreateProveedor(Proveedor proveedor)
        {
            context.Proveedores.Add(proveedor);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProveedor(Proveedor proveedor)
        {
            context.Proveedores.Update(proveedor);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProveedor(int id)
        {
            var proveedor = await context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                context.Proveedores.Remove(proveedor);
                await context.SaveChangesAsync();
            }
        }
    }
}
