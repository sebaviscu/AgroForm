using AgroForm.Business.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Services
{
    public class DolarApiService : IDolarApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://dolarapi.com/v1/dolares";

        public DolarApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DolarInfo>> ObtenerDolaresAsync()
        {
            var response = await _httpClient.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();

            var dolares = await response.Content.ReadFromJsonAsync<List<DolarInfo>>();
            return dolares ?? new List<DolarInfo>();
        }
    }

    public class DolarInfo
    {
        public string Moneda { get; set; }
        public string Casa { get; set; }
        public string Nombre { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
