using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class GastoController : BaseController<Gasto, GastoVM, IGastoService>
    {
        private readonly IMonedaService _monedaService;
        private readonly IActividadService _actividadService;

        public GastoController(ILogger<GastoController> logger, IMapper mapper, IGastoService service, IMonedaService monedaService, IActividadService actividadService)
            : base(logger, mapper, service)
        {
            _monedaService = monedaService;
            _actividadService = actividadService;
        }

        public async Task<IActionResult> Index()
        {
            ValidarAutorizacion(new[] { Roles.Administrador });
            return View();
        }

        public override async Task<IActionResult> Create([FromBody] GastoVM dto)
        {
            var user = ValidarAutorizacion(new[] { Roles.Administrador });

            var tipoCambioUSD = await _monedaService.ObtenerTipoCambioActualAsync();

            dto.CostoARS = UtilidadService.CalcularCostoARS(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.CostoUSD = UtilidadService.CalcularCostoUSD(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.IdMoneda = dto.EsDolar ? (int)Monedas.Dolar : (int)Monedas.Peso;
            dto.CampaniaId = user.IdCampaña;

            return await base.Create(dto);
        }

        public override async Task<IActionResult> Update([FromBody] GastoVM dto)
        {
            var user = ValidarAutorizacion(new[] { Roles.Administrador });

            var tipoCambioUSD = await _monedaService.ObtenerTipoCambioActualAsync();

            dto.CostoARS = UtilidadService.CalcularCostoARS(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.CostoUSD = UtilidadService.CalcularCostoUSD(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.IdMoneda = dto.EsDolar ? (int)Monedas.Dolar : (int)Monedas.Peso;
            dto.CampaniaId = user.IdCampaña;

            return await base.Update(dto);
        }


        [HttpGet]
        public async Task<IActionResult> GetGatosIndex()
        {
            var gResponse = new GenericResponse<GastoDto>();
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                var acividadesResult = await _actividadService.GetLaboresByAsync(user.IdCampaña);
                if (!acividadesResult.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = acividadesResult.ErrorMessage;
                    return BadRequest(gResponse);
                }

                var gastoDto = _mapper.Map<List<GastoDto>>(acividadesResult.Data);

                var gastosResult = await _service.GetAllByCamapniaAsync();
                if (!gastosResult.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = gastosResult.ErrorMessage;
                    return BadRequest(gResponse);
                }

                var gastos = _mapper.Map<List<GastoDto>>(gastosResult.Data);

                gastoDto.AddRange(gastos);

                gResponse.Success = true;
                gResponse.ListObject = gastoDto.Where(_ => _.Costo > 0).OrderBy(_ => _.Fecha).ToList();
                gResponse.Message = "Lista de gastos correctamente";

                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gastos");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }
        }
    }
}
