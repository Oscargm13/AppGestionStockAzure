using System.Net.Http.Headers;
using System.Text;
using AppGestionStock.Models;
using Newtonsoft.Json;

namespace AppGestionStock.Services
{
    public class ServiceAlmacenes
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;

        public ServiceAlmacenes(IConfiguration configuration)
        {
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiAlmacenes");
        }

        private async Task<T> CallApiAsync<T>(string request, HttpMethod method, object data = null)
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

                if (response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(string))
                    {
                        string stringData = await response.Content.ReadAsStringAsync();
                        return (T)Convert.ChangeType(stringData, typeof(T));
                    }
                    else
                    {
                        T result = await response.Content.ReadAsAsync<T>();
                        if (result == null)
                        {
                            throw new Exception("Error: La API devolvió datos inválidos o nulos.");
                        }

                        return result;
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en la API: {response.StatusCode} - {errorContent}");

                }
            }
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            string request = "api/clientes";
            return await CallApiAsync<List<Cliente>>(request, HttpMethod.Get);
        }

        public async Task<Cliente> FindClienteAsync(int id)
        {
            string request = $"api/clientes/{id}";
            return await CallApiAsync<Cliente>(request, HttpMethod.Get);
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            string request = "api/clientes";
            return await CallApiAsync<Cliente>(request, HttpMethod.Post, cliente);
        }

        public async Task<Cliente> UpdateClienteAsync(Cliente cliente)
        {
            string request = $"api/clientes";
            return await CallApiAsync<Cliente>(request, HttpMethod.Put, cliente);
        }

        public async Task<string> DeleteClienteAsync(int id)
        {
            string request = $"api/clientes/{id}";
            return await CallApiAsync<string>(request, HttpMethod.Delete);
        }
    }
}
