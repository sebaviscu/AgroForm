using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CatalogoController : BaseController<Catalogo, CatalogoVM, ICatalogoService>
    {
        public CatalogoController(ILogger<CatalogoController> logger, ICatalogoService service)
        : base(logger, service)
        {
        }


        [HttpGet]
        public async Task<IActionResult> GetByTipo(TipoCatalogoEnum tipo)
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
                return HandleException(ex, "Error al obtener catalogos por tipo", "GetByTipo", tipo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActive()
        {
            try
            {
                var entities = await _service.GetAllActive();
                if (!entities.Success)
                    return Json(new { success = false, message = entities.ErrorMessage });

                gResponse.Success = true;
                gResponse.ListObject = Map<List<Catalogo>, List<CatalogoVM>>(entities.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener catalogos activos", "GetAllActive");
            }
        }

        /// <summary>
        /// Gets all visible catalog items for the current license.
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
                gResponse.ListObject = Map<List<Catalogo>, List<CatalogoVM>>(result.Data);
                gResponse.Message = "Datos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener catálogos visibles", "GetVisible");
            }
        }

        /// <summary>
        /// Sets visibility of a global catalog item for the current license.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetVisibility(int idCatalogo, bool visible)
        {
            try
            {
                var result = await _service.SetVisibilityAsync(idCatalogo, visible);
                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage });

                gResponse.Success = true;
                gResponse.Message = "Visibilidad actualizada correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al establecer visibilidad del catálogo", "SetVisibility", idCatalogo, ("Visible", (object)visible));
            }
        }
    }
}
