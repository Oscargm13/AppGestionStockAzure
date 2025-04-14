using System.Net.Http.Headers;
using System.Text;
using AppGestionStock.Models;
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
    }
}
