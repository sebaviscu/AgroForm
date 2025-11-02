using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class LoteController : BaseController<Lote, LoteVM, ILoteService>
    {
        public LoteController(ILogger<LoteController> logger, IMapper mapper, ILoteService service)
            : base(logger, mapper, service)
        {
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetByCampos(int idCampo)
        {
            try
            {
                var lotes = await _service.GetByidCampoAsync(idCampo);
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
                _logger.LogError(ex, "Error al obtener lotes por campo {idCampo}", idCampo);
                return Json(new { success = false, message = "Error al obtener lotes" });
            }
        }
    }
}
