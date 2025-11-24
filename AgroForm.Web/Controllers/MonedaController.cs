using AgroForm.Business.Contracts;
using AgroForm.Business.Externos.DolarApi;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class MonedaController : BaseController<Moneda, MonedaVM, IMonedaService>
    {
        private readonly IDolarApiService _dolarApiService;

        public MonedaController(ILogger<MonedaController> logger, IMapper mapper, IMonedaService service, IDolarApiService dolarApiService)
            : base(logger, mapper, service)
        {
            _dolarApiService = dolarApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual async Task<IActionResult> ActualizarCotizacion()
        {
            var result = await _dolarApiService.ObtenerCotizacionesAsync();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                _logger.LogError("Error al obtener cotizaciones de DolarApi: " + result.ErrorMessage);
                return NotFound(gResponse);
            }

            await _service.ActualizarMonedasCotizacionAsync(result.Data);

            var monedaActyual = await _service.ObtenerTipoCambioActualAsync();

            gResponse.Success = true;
            gResponse.Object = Map<Moneda, MonedaVM>(monedaActyual);
            gResponse.Message = "Registro encontrado";
            return Ok(gResponse);
        }
    }
}
