using AgroForm.Model;
using AlbaServicios.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AgroForm.Business.Externos.DolarApi
{
    public class DolarApiService : IDolarApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://dolarapi.com/v1/dolares";
        private readonly ILogger<DolarApiService> _log;
        public DolarApiService(HttpClient httpClient, ILogger<DolarApiService> log)
        {
            _httpClient = httpClient;
            _log = log;
        }

        public async Task<OperationResult<List<DolarInfo>>> ObtenerCotizacionesAsync()
        {
            try
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
            catch (Exception e)
            {
                _log.LogError(e, "Error al obtener cotizaciones desde DolarApi.");
                return OperationResult<List<DolarInfo>>.Failure(e.ToString(), "HTTP_CLIENT_FAILED");
                throw;
            }

        }
    }
}
