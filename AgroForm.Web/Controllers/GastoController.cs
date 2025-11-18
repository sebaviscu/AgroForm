using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class GastoController : BaseController<Gasto, GastoVM, IGastoService>
    {
        private readonly IMonedaService _monedaService;

        public GastoController(ILogger<GastoController> logger, IMapper mapper, IGastoService service, IMonedaService monedaService)
            : base(logger, mapper, service)
        {
            _monedaService = monedaService;
        }

        public IActionResult Index()
        {
            ValidarAutorizacion(new[] { Roles.Administrador });

            return View();
        }

        public override async Task<IActionResult> Create([FromBody] GastoVM dto)
        {
            var tipoCambioUSD = await  _monedaService.ObtenerTipoCambioActualAsync();

            dto.CostoARS = UtilidadService.CalcularCostoARS(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.CostoUSD = UtilidadService.CalcularCostoUSD(dto.Costo, dto.EsDolar, tipoCambioUSD.TipoCambioReferencia);
            dto.IdMoneda = dto.EsDolar ? (int)Monedas.Dolar : (int)Monedas.Peso;

            return await base.Create(dto);
        }
    }
}
