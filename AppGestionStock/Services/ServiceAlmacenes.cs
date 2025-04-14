using System.Net.Http.Headers;
using System.Text;
using AppGestionStock.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppGestionStock.Services
{
    public class ServiceAlmacenes
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor contextAccessor;

        public ServiceAlmacenes(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiAlmacenes");
            this.contextAccessor = contextAccessor;
        }

        // ========================
        // MÉTODOS GENERALES HTTP
        // ========================
        private async Task<T> SendRequestAsync<T>(string request, HttpMethod method, object data = null)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                HttpRequestMessage httpRequest = new HttpRequestMessage(method, request);

                if (data != null)
                {
                    string json = JsonConvert.SerializeObject(data);
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                if (typeof(T) == typeof(string))
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    return (T)Convert.ChangeType(stringData, typeof(T));
                }
                else if (response.Content.Headers.ContentLength == 0)
                {
                    return default(T);
                }
                else
                {
                    return await response.Content.ReadAsAsync<T>();
                }
            }
        }

        private async Task<T> GetAsync<T>(string request) =>
            await this.SendRequestAsync<T>(request, HttpMethod.Get);

        private async Task PostAsync<T>(string request, T data) =>
            await this.SendRequestAsync<string>(request, HttpMethod.Post, data);

        private async Task PutAsync<T>(string request, T data) =>
            await this.SendRequestAsync<string>(request, HttpMethod.Put, data);

        private async Task DeleteAsync(string request) =>
            await this.SendRequestAsync<string>(request, HttpMethod.Delete);

        // ========================
        // AUTENTICACIÓN
        // ========================
        public async Task<string> GetTokenAsync(string email, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Auth/Login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                LoginModel model = new LoginModel
                {
                    userName = email,
                    password = pass
                };

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                return null;
            }
        }

        // ========================
        // CLIENTES
        // ========================
        public async Task<List<Cliente>> GetClientesAsync() =>
            await this.GetAsync<List<Cliente>>("api/clientes");

        public async Task<Cliente> FindClienteAsync(int id) =>
            await this.GetAsync<Cliente>($"api/clientes/{id}");

        public async Task CreateClienteAsync(Cliente cliente) =>
            await this.PostAsync("api/clientes", cliente);

        public async Task UpdateClienteAsync(Cliente cliente) =>
            await this.PutAsync("api/clientes", cliente);

        public async Task DeleteClienteAsync(int id) =>
            await this.DeleteAsync($"api/clientes/{id}");

        // ========================
        // PROVEEDORES
        // ========================
        public async Task<List<Proveedor>> GetProveedoresAsync() =>
            await this.GetAsync<List<Proveedor>>("api/proveedores");

        public async Task<Proveedor> FindProveedorAsync(int id) =>
            await this.GetAsync<Proveedor>($"api/proveedores/{id}");

        public async Task CreateProveedorAsync(Proveedor proveedor) =>
            await this.PostAsync("api/proveedores", proveedor);

        public async Task UpdateProveedorAsync(Proveedor proveedor) =>
            await this.PutAsync("api/proveedores", proveedor);

        public async Task DeleteProveedorAsync(int id) =>
            await this.DeleteAsync($"api/proveedores/{id}");

        // ========================
        // TIENDAS
        // ========================

        public async Task<List<Tienda>> GetTiendasAsync()
        {
            return await this.GetAsync<List<Tienda>>("api/Tiendas");
        }

        public async Task<Tienda> FindTiendaAsync(int id)
        {
            return await this.GetAsync<Tienda>($"api/tiendas/{id}");
        }

        public async Task CreateTiendaAsync(Tienda tienda)
        {
            await this.PostAsync("api/tiendas", tienda);
        }

        public async Task UpdateTiendaAsync(Tienda tienda)
        {
            await this.PutAsync($"api/tiendas/{tienda.IdTienda}", tienda);
        }

        public async Task DeleteTiendaAsync(int id)
        {
            await this.DeleteAsync($"api/tiendas/{id}");
        }

        // ========================
        // PRODUCTOS
        // ========================

        public async Task<List<Producto>> GetProductosAsync()
        {
            return await this.GetAsync<List<Producto>>("api/productos");
        }

        public async Task<Producto> FindProductoAsync(int id)
        {
            return await this.GetAsync<Producto>($"api/productos/{id}");
        }

        public async Task<List<Producto>> GetProductosProveedorAsync(int proveedorId)
        {
            return await this.GetAsync<List<Producto>>($"api/productos/proveedor/{proveedorId}");
        }

        public async Task<List<VistaProductoTienda>> GetAllVistaProductosTiendaAsync()
        {
            return await this.GetAsync<List<VistaProductoTienda>>("api/productos/tienda");
        }

        public async Task<List<VistaProductoTienda>> GetVistaProductosTiendaAsync(int idTienda)
        {
            return await this.GetAsync<List<VistaProductoTienda>>($"api/productos/tienda/{idTienda}");
        }

        public async Task<List<VistaProductoTienda>> GetVistaProductosTiendaConStockBajoAsync()
        {
            return await this.GetAsync<List<VistaProductoTienda>>("api/productos/tienda/stockbajo");
        }

        public async Task<List<ProductosTienda>> GetProductosTiendaGerenteAsync(int idGerente)
        {
            return await this.GetAsync<List<ProductosTienda>>($"api/productos/gerente/{idGerente}");
        }

        public async Task<List<VistaProductosGerente>> GetProductosGerenteAsync(int idUsuarioGerente)
        {
            return await this.GetAsync<List<VistaProductosGerente>>($"api/productos/gerente/productos/{idUsuarioGerente}");
        }

        public async Task<int> GetTotalStockGerenteAsync(int idUsuarioGerente)
        {
            return await this.GetAsync<int>($"api/productos/gerente/stock/{idUsuarioGerente}");
        }

        public async Task<VistaProductoTienda> GetProductoTiendaAsync(int idTienda, int idProducto)
        {
            return await this.GetAsync<VistaProductoTienda>($"api/productos/tienda/{idTienda}/producto/{idProducto}");
        }

        public async Task CreateProductoAsync(Producto producto, string? nombreCategoria, int? idCategoriaPadre)
        {
            var query = new StringBuilder("api/productos");

            // Agregamos los query params si están definidos
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(nombreCategoria))
            {
                queryParams.Add($"nombreCategoria={Uri.EscapeDataString(nombreCategoria)}");
            }
            if (idCategoriaPadre.HasValue)
            {
                queryParams.Add($"idCategoriaPadre={idCategoriaPadre.Value}");
            }

            if (queryParams.Any())
            {
                query.Append("?").Append(string.Join("&", queryParams));
            }

            await this.PostAsync(query.ToString(), producto);
        }

        public async Task UpdateProductoAsync(int idProducto, Producto producto)
        {
            await this.PutAsync($"api/productos/{idProducto}", producto);
        }

        public async Task DeleteProductoAsync(int idProducto)
        {
            await this.DeleteAsync($"api/productos/{idProducto}");
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await this.GetAsync<List<Categoria>>("api/productos/categorias");
        }

        public async Task<Producto> GetProductoPorIdAsync(int productoId)
        {
            return await this.GetAsync<Producto>($"api/productos/id/{productoId}");
        }

        // ========================
        // USUARIOS
        // ========================

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await this.GetAsync<List<Usuario>>("api/usuario");
        }

        public async Task<Usuario> FindUsuarioAsync(int id)
        {
            return await this.GetAsync<Usuario>($"api/usuario/{id}");
        }

        public async Task<List<Rol>> GetRolesAsync()
        {
            return await this.GetAsync<List<Rol>>("api/usuario/roles");
        }

        public async Task LoginUsuarioAsync(string nombreUsuario, string password)
        {
            var values = new Dictionary<string, string>
    {
        { "nombreUsuario", nombreUsuario },
        { "password", password }
    };

            var content = new FormUrlEncodedContent(values);

            await this.PostAsync("api/usuario/login", content);

            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(this.UrlApi);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(this.header);

            //    HttpResponseMessage response = await client.PostAsync("api/usuario/login", content);

            //    if (!response.IsSuccessStatusCode)
            //    {
            //        string error = await response.Content.ReadAsStringAsync();
            //        throw new Exception($"Login fallido: {response.StatusCode} - {error}");
            //    }

            //    return await response.Content.ReadFromJsonAsync<Usuario>();
            //}
        }


        public async Task CreateUsuarioAsync(string nombre, string email, string password, int idRol, string imagen, string nombreEmpresa)
        {
            var values = new Dictionary<string, string>
    {
        { "nombre", nombre },
        { "email", email },
        { "password", password },
        { "idRol", idRol.ToString() },
        { "imagen", imagen },
        { "nombreEmpresa", nombreEmpresa }
    };

            var content = new FormUrlEncodedContent(values);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                HttpResponseMessage response = await client.PostAsync("api/usuario", content);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear usuario: {response.StatusCode} - {error}");
                }
            }
        }

        public async Task DeleteUsuarioAsync(int id)
        {
            await this.DeleteAsync($"api/usuario/{id}");
        }

        // ========================
        // Inventario
        // ========================

        public class PagedResult<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

            public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
            {
                Items = items;
                TotalCount = totalCount;
                PageNumber = pageNumber;
                PageSize = pageSize;
            }
        }

        public async Task<List<VistaInventarioDetalladoVenta>> GetMovimientosAsync()
        {
            return await this.GetAsync<List<VistaInventarioDetalladoVenta>>("api/inventario/movimientos");
        }

        public async Task<PagedResult<VistaInventarioDetalladoVenta>> GetMovimientosPaginadosAsync(int pageNumber, int pageSize)
        {
            string request = $"api/inventario/movimientos/paginados?pageNumber={pageNumber}&pageSize={pageSize}";
            return await this.GetAsync<PagedResult<VistaInventarioDetalladoVenta>>(request);
        }

        public async Task<List<Notificacion>> GetNotificacionesAsync()
        {
            return await this.GetAsync<List<Notificacion>>("api/inventario/notificaciones");
        }

        public async Task<bool> ExisteNotificacionAsync(int idProducto, int idTienda)
        {
            return await this.GetAsync<bool>($"api/inventario/notificaciones/existe/{idProducto}/{idTienda}");
        }

        public async Task CreateNotificacionAsync(Notificacion notificacion)
        {
            // Como el endpoint espera [FromForm], usamos FormUrlEncodedContent
            var values = new Dictionary<string, string>
            {
                { "mensaje", notificacion.Mensaje },
                { "fecha", notificacion.Fecha.ToString("o") },
                { "idProducto", notificacion.IdProducto.ToString() },
                { "idTienda", notificacion.IdTienda.ToString() }
            };

            var content = new FormUrlEncodedContent(values);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                HttpResponseMessage response = await client.PostAsync("api/inventario/notificaciones", content);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear notificación: {response.StatusCode} - {error}");
                }
            }
        }

        public async Task DeleteNotificacionAsync(int idNotificacion)
        {
            await this.DeleteAsync($"api/inventario/notificaciones/{idNotificacion}");
        }

        public async Task<int> CreateVentaAsync(DateTime fechaVenta, int idTienda, int idUsuario, decimal importeTotal, int idCliente)
        {
            var values = new Dictionary<string, string>
            {
                { "fechaVenta", fechaVenta.ToString("o") },
                { "idTienda", idTienda.ToString() },
                { "idUsuario", idUsuario.ToString() },
                { "importeTotal", importeTotal.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "idCliente", idCliente.ToString() }
            };

            var content = new FormUrlEncodedContent(values);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                HttpResponseMessage response = await client.PostAsync("api/inventario/ventas", content);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear venta: {response.StatusCode} - {error}");
                }

                return int.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task ProcesarVentaAsync(VentaConDetallesDto ventaDto)
        {
            await this.PostAsync("api/inventario/procesarventa", ventaDto);
        }

        public async Task<List<Venta>> GetVentasAsync()
        {
            return await this.GetAsync<List<Venta>>("api/inventario/ventas");
        }

        public async Task<Venta> GetVentaAsync(int id)
        {
            return await this.GetAsync<Venta>($"api/inventario/{id}");
        }

        public async Task AgregarDetalleVentaAsync(int idVenta, DetallesVenta detalle)
        {
            await this.PostAsync($"api/inventario/ventas/{idVenta}/detalles", detalle);
        }

        public async Task ProcesarCompraAsync(CompraConDetallesDto compraDto)
        {
            await this.PostAsync("api/inventario/procesarcompra", compraDto);
        }

        public async Task<List<Compra>> GetComprasAsync()
        {
            return await this.GetAsync<List<Compra>>("api/inventario/compras");
        }

        public async Task<decimal> GetIngresosMesAsync(int mes, int year)
        {
            return await this.GetAsync<decimal>($"api/inventario/ingresos/{mes}/{year}");
        }

        public async Task<DetallesVenta> GetDetallesVentaAsync(int idDetallesVenta)
        {
            return await this.GetAsync<DetallesVenta>($"api/inventario/detallesventa/{idDetallesVenta}");
        }

        public class CompraConDetallesDto
        {
            public DateTime FechaCompra { get; set; }
            public int IdProveedor { get; set; }
            public int IdTienda { get; set; }
            public decimal ImporteTotal { get; set; }
            public int IdUsuario { get; set; }
            public List<DetalleCompraDto> Detalles { get; set; }
        }

        public class DetalleCompraDto
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnidad { get; set; }
        }
    }
}
