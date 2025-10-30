using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
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
    public class ActividadController : BaseController<Actividad, IActividadService>
    {
        private readonly ICampoService _campoService;
        private readonly ILoteService _loteService;
        private readonly IMovimientoInsumoService _movimientoInsumoService;
        public ActividadController(ILogger<ActividadController> logger, IMapper mapper, IActividadService service, ICampoService campoService, ILoteService loteService, IMovimientoInsumoService movimientoInsumoService)
        : base(logger, mapper, service)
        {
            _campoService = campoService;
            _loteService = loteService;
            _movimientoInsumoService = movimientoInsumoService;
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
        public async Task<IActionResult> FiltrarPorCampo(int campoId = 0)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                List<Actividad> actividades;

                if (campoId == 0)
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
                    var lotesResult = await _loteService.GetByCampoIdAsync(campoId);
                    if (!lotesResult.Success)
                    {
                        return Json(new { success = false, message = "Error al obtener lotes del campo" });
                    }

                    var lotesIds = lotesResult.Data.Select(l => l.Id).ToList();
                    actividades = await _service.GetByCampoIdAsync(lotesIds);
                }

                var actividadesVM = Map<List<Actividad>, List<ActividadVM>>(actividades);

                return Json(new { success = true, data = actividadesVM });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar actividades por campo {CampoId}", campoId);
                return Json(new { success = false, message = "Error al filtrar actividades" });
            }
        }

        // Endpoint para ver detalles de actividad
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = await _service.GetByIdWithDetailsAsync(id);
                if (!result.Success)
                {
                    return Json(new { success = false, message = "Actividad no encontrada" });
                }

                var actividadVM = Map<Actividad, ActividadVM>(result.Data);
                return Json(new { success = true, data = actividadVM });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de actividad {Id}", id);
                return Json(new { success = false, message = "Error al cargar detalles" });
            }
        }

        // Endpoint para cargar formulario de edición
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = await _service.GetByIdWithDetailsAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = "Actividad no encontrada";
                    return RedirectToAction("Index");
                }

                var actividadVM = Map<Actividad, ActividadVM>(result.Data);

                // Cargar datos para dropdowns si es necesario
                await CargarDatosParaFormulario();

                return View(actividadVM);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cargar formulario de edición");
            }
        }

        // Endpoint para actualizar actividad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ActividadVM model)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    await CargarDatosParaFormulario();
                    return View(model);
                }

                var actividad = Map<ActividadVM, Actividad>(model);
                var result = await _service.UpdateAsync(actividad);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    await CargarDatosParaFormulario();
                    return View(model);
                }

                TempData["Success"] = "Actividad actualizada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar actividad");
            }
        }

        // Endpoint para eliminar actividad
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = await _service.DeleteAsync(id);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }

                return Json(new { success = true, message = "Actividad eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar actividad {Id}", id);
                return Json(new { success = false, message = "Error al eliminar actividad" });
            }
        }

        // Método auxiliar para cargar datos de formulario
        private async Task CargarDatosParaFormulario()
        {
            var camposResult = await _campoService.GetAllAsync();
            if (camposResult.Success)
            {
                ViewBag.Campos = camposResult.Data.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToList();
            }

            // Aquí puedes cargar otros datos necesarios para el formulario
            // como tipos de actividad, responsables, etc.
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

                // Mapear a entidad Actividad
                var actividad = new Actividad
                {
                    Fecha = model.Fecha,
                    TipoActividadId = model.TipoActividadId,
                    Observacion = model.Observacion ?? string.Empty,
                    Descripcion = "Actividad rápida", // O obtener del tipo de actividad
                    LoteId = 1, // O el lote por defecto
                    UsuarioId = user.IdUsuario, // Método para obtener usuario actual
                    RegistrationDate = DateTime.Now,
                    RegistrationUser = CurrentUser
                };

                // Crear actividad
                var resultActividad = await _service.CreateAsync(actividad);
                if (!resultActividad.Success)
                {
                    return Json(new { success = false, message = resultActividad.ErrorMessage });
                }

                // Crear movimientos de insumo si existen
                if (model.Insumo != null)
                {
                        var movimiento = new MovimientoInsumo
                        {
                            ActividadId = actividad.Id,
                            InsumoId = model.Insumo.Id,
                            Cantidad = model.Cantidad.GetValueOrDefault(),
                            CostoUnitario = 0, // O obtener costo actual
                            EsEntrada = false, // Asumimos salida
                            MonedaId = 1, // Moneda por defecto
                            UsuarioId = user.IdUsuario,
                            RegistrationDate = DateTime.Now,
                            RegistrationUser = CurrentUser
                        };

                        var resultMovimiento = await _movimientoInsumoService.CreateAsync(movimiento);
                        if (!resultMovimiento.Success)
                        {
                            // Log error pero continuar con otros insumos
                            _logger.LogWarning("Error al crear movimiento de insumo: {Error}", resultMovimiento.ErrorMessage);
                        }
                    
                }

                return Json(new { success = true, message = "Actividad creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear actividad rápida");
                return Json(new { success = false, message = "Error al crear actividad" });
            }
        }
    }
}