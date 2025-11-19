using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var user = ValidarAutorizacion(new[] { Roles.Administrador });

            var acividadesResult = await _actividadService.GetLaboresByAsync(user.IdCampaña);
            if (!acividadesResult.Success)
            {
                return BadRequest(acividadesResult.ErrorMessage);
            }

            var gastoDto = _mapper.Map<List<GastoDto>>(acividadesResult.Data);

            var gastosResult = await _service.GetAllByCamapniaAsync();
            if (!gastosResult.Success)
            {
                return BadRequest(gastosResult.ErrorMessage);
            }

            var gastos = _mapper.Map<List<GastoDto>>(gastosResult.Data);

            gastoDto.AddRange(gastos);

            var vm = new GastosIndexVM()
            {
                Gastos = gastoDto.Where(_=>_.Costo>0).OrderBy(_ => _.Fecha).ToList()
            };

            return View(vm);
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
    }
}
