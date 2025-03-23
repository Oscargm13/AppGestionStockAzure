using AppGestionStock.Data;
using AppGestionStock.Models;
using Microsoft.EntityFrameworkCore;

namespace AppGestionStock.Repositories
{
    public class RepositoryUsuario
    {
        private AlmacenesContext context;
        public RepositoryUsuario(AlmacenesContext context)
        {
            this.context = context;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var consulta = from datos in this.context.Usuarios select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Rol>> GetRoles()
        {
            var consulta = from datos in this.context.Roles select datos;
            return await consulta.ToListAsync();
        }

        public async Task PostUsuario(string nombre, string email, string pass, int idRole)
        {
            try
            {
                // Crear un nuevo objeto Usuario
                var usuario = new Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Password = pass,
                    IdRol = idRole
                };

                // Agregar el usuario a la base de datos
                await this.context.Usuarios.AddAsync(usuario);

                // Guardar los cambios
                await this.context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejar errores (por ejemplo, registrar el error)
                Console.WriteLine($"Error al crear usuario: {ex.Message}");
                throw; // Re-lanzar la excepción para que el llamador pueda manejarla
            }
        }

        public async Task<Usuario> CompararUsuario(string nombreUsuario, string password)
        {
            List<Usuario> usuarios = await this.GetUsuariosAsync();
            foreach(Usuario usuario in usuarios)
            {
                if(usuario.Nombre == nombreUsuario && usuario.Password == password)
                {
                    return usuario;
                }
            }
            return null;
        }

        public async Task<Usuario> findUsuario(int idUsuario)
        {
            try
            {
                var usuario = await this.context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar usuario: {ex.Message}");
                return null;
            }
        }
    }
}
