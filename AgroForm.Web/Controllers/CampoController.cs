using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CampoController : BaseController<Campo, ICampoService>
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
        public async Task<IActionResult> ObtenerCampos()
        {
            try
            {
                var campos = await _service.GetAllAsync();
                if (!campos.Success)
                    return Json(new { success = false, message = campos.ErrorMessage });

                var data = campos.Data.Select(c => new {
                    id = c.Id,
                    nombre = c.Nombre,
                    ubicacion = c.Ubicacion,
                    superficieHectareas = c.SuperficieHectareas
                });

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener campos");
                return Json(new { success = false, message = "Error al obtener campos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerLotesPorCampo(int campoId)
        {
            try
            {
                var lotes = await _loteService.GetByCampoIdAsync(campoId);
                if (!lotes.Success)
                    return Json(new { success = false, message = lotes.ErrorMessage });

                var data = lotes.Data.Select(l => new {
                    id = l.Id,
                    nombre = l.Nombre,
                    campoNombre = l.Campo?.Nombre ?? "Sin campo",
                    superficie = l.SuperficieHectareas
                });

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes por campo {CampoId}", campoId);
                return Json(new { success = false, message = "Error al obtener lotes" });
            }
        }
    }
}
