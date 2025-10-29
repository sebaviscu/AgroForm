using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class ActividadController : BaseController<Actividad>
    {
        private readonly IActividadService _actividadService;
        private readonly ICampoService _campoService;
        private readonly ILoteService _loteService;
        public ActividadController(ILogger<ActividadController> logger, IMapper mapper, IActividadService service, ICampoService campoService, ILoteService loteService)
        : base(logger, mapper)
        {
            _actividadService = service;
            _campoService = campoService;
            _loteService = loteService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var campos = await _campoService.GetAllAsync();
                var actividades = await _actividadService.GetAllAsync();

                if(!actividades.Success)
                {
                    return BadRequest(actividades.ErrorMessage);
                }

                if(!campos.Success)
                {
                    return BadRequest(campos.ErrorMessage);
                }

                var vm = new ActividadesIndexVM
                {
                    Campos = campos.Data.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre
                    }).ToList(),
                    Actividades = Map<List<Actividad>, List<ActividadVM>>(actividades.Data)
                };

                // Agregar opción "TODOS"
                vm.Campos.Insert(0, new SelectListItem { Value = "0", Text = "TODOS", Selected = true });

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cargar actividades");
            }
        }

        [HttpPost]
        public async Task<IActionResult> FiltrarPorCampo(int campoId = 0)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                List<Actividad> actividades;

                if (campoId == 0)
                {
                    var result = await _actividadService.GetAllAsync();
                    if(result.Success)
                    {
                        actividades = result.Data;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Error al obtener actividades" });
                    }
                }
                else
                {
                    var lotes = await _loteService.GetByCampoIdAsync(campoId);
                    actividades = await _actividadService.GetByCampoIdAsync(lotes.Select(_=>_.Id).ToList());
                }

                var actividadesVM = Map<List<Actividad>, List<ActividadVM>>(actividades);

                return Json(new { success = true, data = actividadesVM });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al filtrar actividades");
            }
        }
    }
}
