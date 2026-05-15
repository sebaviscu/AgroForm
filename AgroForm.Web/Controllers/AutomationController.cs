using AgroForm.Business.Contracts;
using AgroForm.Business.Externos.DolarApi;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [ApiController]
    [Route("api/automation")]
    public class AutomationController : ControllerBase
    {
        private readonly ILogger<AutomationController> _logger;

        private readonly IDolarApiService _dolarService;
        private readonly IMonedaService _monedaService;

        // API KEY para proteger endpoints
        private readonly IConfiguration _configuration;

        public AutomationController(
            ILogger<AutomationController> logger,
            IConfiguration configuration,
            IDolarApiService dolarApiService,
            IMonedaService monedaService)
        {
            _logger = logger;
            _configuration = configuration;

            _dolarService = dolarApiService;
            _monedaService = monedaService;
        }

        /// <summary>
        /// Valida API KEY enviada desde Activepieces
        /// </summary>
        private bool IsAuthorized(string? apiKey)
        {
            var validApiKey = _configuration["Automation:ApiKey"];

            return !string.IsNullOrWhiteSpace(apiKey)
                   && apiKey == validApiKey;
        }

        /// <summary>
        /// Endpoint base para validar conexión
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                ok = true,
                service = "automation",
                date = TimeHelper.GetArgentinaTime()
            });
        }

        /// <summary>
        /// Actualiza cotizaciones del dólar
        /// </summary>
        [HttpPost("dolar/actualizar")]
        public async Task<IActionResult> ActualizarDolar(
            [FromHeader(Name = "x-api-key")] string? apiKey)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized();

            try
            {
                _logger.LogInformation("Iniciando actualización de dólar - {Date}", TimeHelper.GetArgentinaTime());

                var result = await _dolarService.ObtenerCotizacionesAsync();

                if (!result.Success)
                {
                    _logger.LogError("Error al obtener cotizaciones de DolarApi: " + result.ErrorMessage);
                }

                await _monedaService.ActualizarMonedasCotizacionAsync(result.Data);

                var monedaActyual = await _monedaService.ObtenerTipoCambioActualAsync();

                return Ok(new
                {
                    ok = true,
                    message = "Cotizaciones actualizadas",
                    date = TimeHelper.GetArgentinaTime()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cotizaciones");

                return StatusCode(500, new
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Genera resumen semanal
        /// </summary>
        [HttpPost("resumen/semanal")]
        public async Task<IActionResult> GenerarResumenSemanal(
            [FromHeader(Name = "x-api-key")] string? apiKey)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized();

            try
            {
                _logger.LogInformation("Generando resumen semanal - {Date}", TimeHelper.GetArgentinaTime());

                //await _resumenService.GenerarResumenSemanal();

                return Ok(new
                {
                    ok = true,
                    message = "Resumen semanal generado",
                    date = TimeHelper.GetArgentinaTime()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando resumen semanal");

                return StatusCode(500, new
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Genera resumen semanal
        /// </summary>
        [HttpPost("lluvia/consultar")]
        public async Task<IActionResult> ConsultarLluvia(
            [FromHeader(Name = "x-api-key")] string? apiKey)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized();

            try
            {
                _logger.LogInformation("Consultar lluvia - {Date}", TimeHelper.GetArgentinaTime());



                return Ok(new
                {
                    ok = true,
                    message = "Lluvia consultada",
                    date = TimeHelper.GetArgentinaTime()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar lluvia");

                return StatusCode(500, new
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }


    }
}
