using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CampaniaController : BaseController<Campania, CampaniaVM, ICampaniaService>
    {
        public CampaniaController(ILogger<CampaniaController> logger, IMapper mapper, ICampaniaService service)
            : base(logger, mapper, service)
        {
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var campanias = await _service.GetAllWithDetailsAsync();

                if (!campanias.Success)
                {
                    return BadRequest(campanias.ErrorMessage);
                }

                var vm = new CampaniasIndexVM
                {
                    Campanias = Map<List<Campania>, List<CampaniaVM>>(campanias.Data),
                    Estados = new List<SelectListItem>
                {
                    new SelectListItem { Value = "EnCurso", Text = "En Curso" },
                    new SelectListItem { Value = "Finalizada", Text = "Finalizada" },
                    new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
                }
                };

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
            catch (Exception ex)
            {
                return HandleException(ex,"Error al iniciar la pagina", "Index");
            }
        }
    }
}
