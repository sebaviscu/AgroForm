using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Scaffolding;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class ActividadController : BaseController<Actividad, ActividadVM, IActividadService>
    {
        private readonly ICampoService _campoService;
        private readonly ILoteService _loteService;

        public ActividadController(ILogger<ActividadController> logger, IMapper mapper, IActividadService service, ICampoService campoService, ILoteService loteService)
        : base(logger, mapper, service)
        {
            _campoService = campoService;
            _loteService = loteService;
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var campos = await _campoService.GetAllAsync();
                var actividades = await _service.GetAllAsync();

                if (!actividades.Success)
                {
                    return BadRequest(actividades.ErrorMessage);
                }

                if (!campos.Success)
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
        public async Task<IActionResult> FiltrarPorCampo(int idCampo = 0)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                List<Actividad> actividades;

                if (idCampo == 0)
                {
                    var result = await _service.GetAllAsync();
                    if (!result.Success)
                    {
                        return Json(new { success = false, message = "Error al obtener actividades" });
                    }
                    actividades = result.Data;
                }
                else
                {
                    var lotesResult = await _loteService.GetByidCampoAsync(idCampo);
                    if (!lotesResult.Success)
                    {
                        return Json(new { success = false, message = "Error al obtener lotes del campo" });
                    }

                    var lotesIds = lotesResult.Data.Select(l => l.Id).ToList();
                    actividades = await _service.GetByidCampoAsync(lotesIds);
                }

                var actividadesVM = Map<List<Actividad>, List<ActividadVM>>(actividades);

                return Json(new { success = true, data = actividadesVM });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar actividades por campo {idCampo}", idCampo);
                return Json(new { success = false, message = "Error al filtrar actividades" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CrearRapida([FromBody] ActividadRapidaVM model)
        {
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var actividades = new List<Actividad>();
                foreach (var item in model.LotesIds)
                {
                    var actividad = new Actividad
                    {
                        Fecha = model.Fecha,
                        IdTipoActividad = model.TipoidActividad,
                        Observacion = model.Observacion ?? string.Empty,
                        IdLote = item,
                        IdUsuario = user.IdUsuario,
                        Cantidad = model.Cantidad,
                        IdInsumo = model.idInsumo
                    };

                    actividades.Add(actividad);
                }

                if (model.idInsumo.HasValue)
                {
                    // actualizar precio
                }

                // Crear actividad
                var resultActividad = await _service.CreateRangeAsync(actividades);
                if (!resultActividad.Success)
                {
                    return Json(new { success = false, message = resultActividad.ErrorMessage });
                }

                return Json(new { success = true, message = "Actividad creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear actividad rápida");
                return Json(new { success = false, message = "Error al crear actividad" });
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetRecent()
        {
            var result = await _service.GetRecentAsync();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = Map<List<Actividad>, List<ActividadVM>>(result.Data);
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }
    }
}