using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CultivoController : BaseController<Cultivo, CultivoVM, ICultivoService>
    {
        public CultivoController(ILogger<CultivoController> logger, ICultivoService service)
            : base(logger, service)
        {
        }

        /// <summary>
        /// Gets all visible crops for the current license.
        /// Returns global crops + own crops, excluding hidden ones.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVisible()
        {
            try
            {
                var result = await _service.GetVisibleByLicenseAsync();
                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                gResponse.Success = true;
                gResponse.ListObject = Map<List<Cultivo>, List<CultivoVM>>(result.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener cultivos visibles", "GetVisible");
            }
        }

        /// <summary>
        /// Sets visibility of a global crop for the current license.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetVisibility(int idCultivo, bool visible)
        {
            try
            {
                var result = await _service.SetVisibilityAsync(idCultivo, visible);
                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                gResponse.Success = true;
                gResponse.Message = "Visibilidad actualizada correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al establecer visibilidad del cultivo", "SetVisibility", idCultivo, ("Visible", (object)visible));
            }
        }
    }
}
