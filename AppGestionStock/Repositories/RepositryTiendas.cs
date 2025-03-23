using System.Globalization;
using AppGestionStock.Data;
using AppGestionStock.Models;
using Microsoft.Data.SqlClient;

namespace AppGestionStock.Repositories
{
    public class RepositryTiendas
    {
        SqlConnection cn;
        SqlCommand com;
        SqlDataReader reader;

        private AlmacenesContext context;
        public RepositryTiendas(AlmacenesContext context)
        {
            this.context = context;
            this.cn = new SqlConnection();
            this.com = new SqlCommand();
            this.com.Connection = this.cn;
        }
        public List<Tienda> GetTiendas()
        {
            var consulta = from datos in this.context.Tiendas select datos;
            return consulta.ToList();
        }

        public Tienda FindTienda(int idTienda)
        {
            var consulta = (from datos in this.context.Tiendas
                            where datos.IdTienda == idTienda select datos).FirstOrDefault();
            return consulta;
        }

        public void CrearTienda(int idTienda, string nombre, string direccion, string telefono, string email)
        {
            Tienda nuevaTienda = new Tienda
            {
                IdTienda = idTienda,
                Nombre = nombre,
                Direccion = direccion,
                Telefono = telefono,
                Email = email
            };

            this.context.Tiendas.Add(nuevaTienda);
            this.context.SaveChanges();
        }
    }
}
