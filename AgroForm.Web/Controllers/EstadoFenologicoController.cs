using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class EstadoFenologicoController : BaseController<EstadoFenologico, EstadoFenologicoVM, IEstadoFenologicoService>
    {
        public EstadoFenologicoController(ILogger<EstadoFenologicoController> logger, IMapper mapper, IEstadoFenologicoService service)
            : base(logger, mapper, service)
        {
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByCultivo(int id)
        {
            try
            {
                var resultLabores = await _service.GetFenologicosByCultivoAsync(id);
                if (!resultLabores.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = resultLabores.ErrorMessage;
                    return BadRequest(gResponse);
                }    

                gResponse.Success = true;
                gResponse.ListObject = Map<List<EstadoFenologico>, List<EstadoFenologicoVM>>(resultLabores.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener Fenologicos por idCultivo", "GetFenologicosByCultivoAsync", id);
            }
        }
    }
}
