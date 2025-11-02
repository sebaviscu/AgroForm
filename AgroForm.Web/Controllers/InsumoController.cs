using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class InsumoController : BaseController<Insumo, InsumoVM, IInsumoService>
    {
        public InsumoController(ILogger<InsumoController> logger, IMapper mapper, IInsumoService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetByTipoInsumo(int idTipoInsumo)
        {
            try
            {
                var result = await _service.GetByTipoInsumo(idTipoInsumo);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return NotFound(gResponse);
                }

                gResponse.Success = true;
                gResponse.ListObject = Map<List<Insumo>, List<InsumoVM>>(result.Data);
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Insumos por Tipo de Insumo {idTipoInsumo}", idTipoInsumo);
                return Json(new { success = false, message = "Error al obtener Insumos" });
            }
        }
    }
}
