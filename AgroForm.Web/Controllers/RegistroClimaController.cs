using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using Mapster;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class RegistroClimaController : BaseController<RegistroClima, RegistroClimaVM, IRegistroClimaService>
    {
        private readonly ILoteService _loteService;
        private readonly ICampoService _campoService;
        public RegistroClimaController(ILogger<RegistroClimaController> logger, IRegistroClimaService service, ILoteService loteService, ICampoService campoService)
            : base(logger, service)
        {
            _loteService = loteService;
            _campoService = campoService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                var campos = await _campoService.GetAllWithDetailsAsync();

                var climas = await _service.GetByCampaniaAsync(user.IdCampaña);

                if (!climas.Success)
                {
                    return BadRequest(climas.ErrorMessage);
                }

                if (!campos.Success)
                {
                    return BadRequest(campos.ErrorMessage);
                }

                var vm = new RegistroClimaIndexVM
                {
                    Campos = campos.Data.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre
                    }).ToList(),
                    Climas = Map<List<RegistroClima>, List<RegistroClimaVM>>(climas.Data)
                };

                vm.Campos.Insert(0, new SelectListItem { Value = "", Text = "TODOS", Selected = true });

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
            //catch (Exception ex)
            //{
            //    return HandleException(ex, "Error al cargar actividades", "Index");
            //}
        }


        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] RegistroClimaVM dto)
        {
            try
            {
                var entity = Map<RegistroClimaVM, RegistroClima>(dto);

                var registroLluvia = new RegistroClima()
                {
                    Observaciones = dto.Observaciones,
                    Milimetros = dto.Milimetros,
                    TipoClima = dto.TipoClima,
                    Fecha = dto.Fecha,
                    IdCampo = dto.IdCampo,
                };

                var result = await _service.CreateAsync(entity);

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
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear campo", "Create", dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDatosLluvia(int meses = 6, int campoId = 0)
        {
            try
            {
                var result = await _service.GetRegistroClimasAsync(meses, campoId);
                var resultHistorico = await _service.GetRegistroClimasHistoricoAsync(meses, campoId);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return NotFound(gResponse);
                }

                var registros = result.Data;
                var registrosHistoricos = resultHistorico.Data ?? new List<RegistroClima>();
                var fechaFin = TimeHelper.GetArgentinaTime();

                var todosLosMeses = new List<DateTime>();
                for (int i = 0; i < meses; i++)
                {
                    var mes = fechaFin.AddMonths(-i);
                    todosLosMeses.Add(new DateTime(mes.Year, mes.Month, 1));
                }
                todosLosMeses = todosLosMeses.OrderBy(m => m).ToList();

                // Agrupar datos actuales por mes
                var datosAgrupados = registros
                    .GroupBy(rc => new
                    {
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

                // Agrupar datos históricos
                var datosHistoricosAgrupados = registrosHistoricos
                    .GroupBy(rc => new
                    {
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
                    var tieneDatosHistoricos = datosHistoricosAgrupados.TryGetValue(mes, out var datosHistoricos);
                    return new
                    {
                        mes = mes.ToString("MMM yyyy"),
                        totalLluvia = tieneDatos ? Math.Round((decimal)datos.TotalLluvia, 1) : 0,
                        cantidadGranizo = tieneDatos ? datos.CantidadGranizo : 0,
                        totalLluviaHistorico = tieneDatosHistoricos ? Math.Round((decimal)datosHistoricos.TotalLluvia, 1) : 0,
                        cantidadGranizoHistorico = tieneDatosHistoricos ? datosHistoricos.CantidadGranizo : 0
                    };
                }).ToList();

                // Calcular totales para comparación histórica
                var totalLluviaActual = registros.Where(x => x.TipoClima == TipoClima.Lluvia).Sum(x => x.Milimetros);
                var totalLluviaHistorico = registrosHistoricos.Where(x => x.TipoClima == TipoClima.Lluvia).Sum(x => x.Milimetros);
                
                decimal variacionPorcentaje;
                bool tieneDatosHistoricos = registrosHistoricos.Any(x => x.TipoClima == TipoClima.Lluvia);
                
                if (tieneDatosHistoricos && totalLluviaHistorico > 0) {
                    variacionPorcentaje = Math.Round(((totalLluviaActual - totalLluviaHistorico) / totalLluviaHistorico) * 100, 1);
                } else {
                    variacionPorcentaje = 0; // No hay datos históricos o es cero
                }

                return Json(new { 
                    success = true, 
                    data = resultado,
                    comparacionHistorica = new {
                        totalLluviaActual = Math.Round((decimal)totalLluviaActual, 1),
                        totalLluviaHistorico = Math.Round((decimal)totalLluviaHistorico, 1),
                        variacionPorcentaje = variacionPorcentaje,
                        tieneDatosHistoricos = tieneDatosHistoricos
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener datos de lluvia", "GetDatosLluvia");
            }
        }

    }
}
