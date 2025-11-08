using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CampoController : BaseController<Campo, CampoVM, ICampoService>
    {
        private readonly IActividadService _actividadService;

        public CampoController(ILogger<CampoController> logger, IMapper mapper, ICampoService service, IActividadService actividadService)
            : base(logger, mapper, service)
        {
            _actividadService = actividadService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetCamposConLotesYActividades()
        {
            try
            {

                var result = await _service.GetAllWithDetailsAsync();
                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                var campoVM = Map<List<Campo>, List<CampoVM>>(result.Data);

                foreach (var campo in campoVM)
                {

                    foreach (var lote in campo.Lotes)
                    {
                        var resultLabores = await _actividadService.GetLaboresByAsync(IdLote: lote.Id);
                        if (!resultLabores.Success)
                        {
                            gResponse.Success = false;
                            gResponse.Message = resultLabores.ErrorMessage;
                            return BadRequest(gResponse);
                        }
                        lote.Actividades = resultLabores.Data;
                    }
                }

                gResponse.Success = true;
                gResponse.ListObject = campoVM;
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex,"Error al cargar campos y lotes con actividades", "GetCamposConLotesYActividades");
            }
        }
    }
}
