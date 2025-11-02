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
        private readonly ILoteService _loteService;
        public CampoController(ILogger<CampoController> logger, IMapper mapper, ICampoService service, ILoteService loteService)
            : base(logger, mapper, service)
        {
            _loteService = loteService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetCamposConLotesYActividades()
        {
            var result = await _service.GetCamposConLotesYActividades();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = Map<List<Campo>, List<CampoVM>>(result.Data);
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }
    }
}
