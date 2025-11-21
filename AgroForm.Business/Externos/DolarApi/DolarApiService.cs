using AgroForm.Model;
using AlbaServicios.Services;
using System.Net.Http.Json;

namespace AgroForm.Business.Externos.DolarApi
{
    public class DolarApiService  : IDolarApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://dolarapi.com/v1/dolares";

        public DolarApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OperationResult<List<DolarInfo>>> ObtenerCotizacionesAsync()
        {
            var response = await _httpClient.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();

            var dolares = await response.Content.ReadFromJsonAsync<List<DolarInfo>>();

            if (dolares == null)
            {
                return OperationResult<List<DolarInfo>>.Failure("Erro en la llamasa al api de DolarApi.", "HTTP_CLIENT_FAILED");
            }

            return OperationResult<List<DolarInfo>>.SuccessResult(dolares);
        }
    }
}
