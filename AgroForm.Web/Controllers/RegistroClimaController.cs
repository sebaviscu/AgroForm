using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class RegistroClimaController : BaseController<RegistroClima, RegistroClimaVM, IRegistroClimaService>
    {
        private readonly ILoteService _loteService;
        public RegistroClimaController(ILogger<RegistroClimaController> logger, IMapper mapper, IRegistroClimaService service, ILoteService loteService)
            : base(logger, mapper, service)
        {
            _loteService = loteService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] RegistroClimaVM dto)
        {
            var entity = Map<RegistroClimaVM, RegistroClima>(dto);

            var lotes = await _loteService.GetByidCampoAsync(dto.IdCampo);
            if (!lotes.Success)
            {
                gResponse.Success = false;
                gResponse.Message = lotes.ErrorMessage;
                return NotFound(gResponse);
            }

            var registrosLluvia = new List<RegistroClima>();

            foreach (var item in lotes.Data)
            {
                var rl = new RegistroClima()
                {
                    IdLote = item.Id,
                    Observaciones = dto.Observaciones,
                    Milimetros = dto.Milimetros,
                    TipoClima = dto.TipoClima,
                    Fecha = dto.Fecha
                };
                registrosLluvia.Add(rl);
            }

            var result = await _service.CreateRangeAsync(registrosLluvia);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Message = "Registro creado correctamente";
            return Ok(gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetDatosLluvia(int meses = 6, int campoId = 0)
        {
            try
            {
                var registros = await _service.GetRegistroClimasAsync(meses, campoId);
                var fechaFin = TimeHelper.GetArgentinaTime();

                var todosLosMeses = new List<DateTime>();
                for (int i = 0; i < meses; i++)
                {
                    var mes = fechaFin.AddMonths(-i);
                    todosLosMeses.Add(new DateTime(mes.Year, mes.Month, 1));
                }
                todosLosMeses = todosLosMeses.OrderBy(m => m).ToList();

                // Agrupar datos por mes
                var datosAgrupados = registros
                    .GroupBy(rc => new {
                        Año = rc.Fecha.Year,
                        Mes = rc.Fecha.Month
                    })
                    .ToDictionary(
                        g => new DateTime(g.Key.Año, g.Key.Mes, 1),
                        g => new
                        {
                            TotalLluvia = g.Where(x => x.TipoClima == TipoClima.Lluvia).Sum(x => x.Milimetros),
                            CantidadGranizo = g.Count(x => x.TipoClima == TipoClima.Granizo)
                        }
                    );

                // Combinar con todos los meses
                var resultado = todosLosMeses.Select(mes =>
                {
                    var tieneDatos = datosAgrupados.TryGetValue(mes, out var datos);
                    return new
                    {
                        mes = mes.ToString("MMM yyyy"),
                        totalLluvia = tieneDatos ? Math.Round((decimal)datos.TotalLluvia, 1) : 0,
                        cantidadGranizo = tieneDatos ? datos.CantidadGranizo : 0
                    };
                }).ToList();

                return Json(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de lluvia");
                return Json(new { success = false, message = "Error al cargar los datos" });
            }
        }

    }
}
