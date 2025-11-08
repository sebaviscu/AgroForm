using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class VariedadController : BaseController<Variedad, VariedadVM, IVariedadService>
    {
        public VariedadController(ILogger<VariedadController> logger, IMapper mapper, IVariedadService service)
        : base(logger, mapper, service)
        {
        }


        [HttpGet]
        public async Task<IActionResult> GetByCultivo(int idCultivo)
        {
            try
            {
                var entities = await _service.GetByCultivo(idCultivo);
                if (!entities.Success)
                    return Json(new { success = false, message = entities.ErrorMessage });

                gResponse.Success = true;
                gResponse.ListObject = Map<List<Variedad>, List<VariedadVM>>(entities.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Variedads por tipo {idCultivo}", idCultivo);
                return Json(new { success = false, message = "Error al obtener Variedads por tipo" });
            }
        }

    }
}

