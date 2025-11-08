using AgroForm.Business.Contracts;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CatalogoController : BaseController<Catalogo, CatalogoVM, ICatalogoService>
    {
        public CatalogoController(ILogger<CatalogoController> logger, IMapper mapper, ICatalogoService service)
        : base(logger, mapper, service)
        {
        }


        [HttpGet]
        public async Task<IActionResult> GetByTipo(TipoCatalogo tipo)
        {
            try
            {
                var entities = await _service.GetByType(tipo);
                if (!entities.Success)
                    return Json(new { success = false, message = entities.ErrorMessage });

                gResponse.Success = true;
                gResponse.ListObject = Map<List<Catalogo>, List<CatalogoVM>>(entities.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catalogos por tipo {tipo}", tipo);
                return Json(new { success = false, message = "Error al obtener catalogos por tipo" });
            }
        }

    }
}
