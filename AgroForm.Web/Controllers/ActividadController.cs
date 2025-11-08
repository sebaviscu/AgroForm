using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AgroForm.Web.Utilities;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Scaffolding;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class ActividadController : Controller
    {
        private readonly ICampoService _campoService;
        private readonly ILoteService _loteService;
        protected readonly ILogger<ActividadController> _logger;
        protected readonly IMapper _mapper;
        protected readonly IActividadService _service;
        protected string CurrentUser => HttpContext?.User?.Identity?.Name ?? "Anonimo";

        public ActividadController(ILogger<ActividadController> logger, IMapper mapper, IActividadService service)
        {
            _logger = logger;
            _mapper = mapper;
            _service = service;
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var campos = await _campoService.GetAllAsync();
                var idsLotes = campos.Data.SelectMany(c => c.Lotes).Select(l => l.Id).ToList();

                var actividades = await _service.GetLaboresByAsync(IdsLotes: idsLotes);

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
                    Actividades = actividades.Data
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
                return HandleException(ex, "Error al cargar actividades", "Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> FiltrarPorCampo(int idCampo = 0)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                //List<Actividad> actividades;

                //if (idCampo == 0)
                //{
                //    var result = await _service.GetAllAsync();
                //    if (!result.Success)
                //    {
                //        return Json(new { success = false, message = "Error al obtener actividades" });
                //    }
                //    actividades = result.Data;
                //}
                //else
                //{
                //    var lotesResult = await _loteService.GetByidCampoAsync(idCampo);
                //    if (!lotesResult.Success)
                //    {
                //        return Json(new { success = false, message = "Error al obtener lotes del campo" });
                //    }

                //    var lotesIds = lotesResult.Data.Select(l => l.Id).ToList();
                //    actividades = await _service.GetByidCampoAsync(lotesIds);
                //}

                //var actividadesVM = Map<List<Actividad>, List<ActividadVM>>(actividades);

                //return Json(new { success = true, data = actividadesVM });

                return default;
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
            var gResponse = new GenericResponse<int>();
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var actividades = new List<ILabor>();

                foreach (var loteId in model.LotesIds)
                {
                    ILabor actividad = null;

                    switch (model.TipoActividad?.ToLower())
                    {
                        case "siembra":
                            actividad = new Siembra
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                SuperficieHa = model.DatosEspecificos?.SuperficieHa ?? 0,
                                DensidadSemillaKgHa = model.DatosEspecificos?.DensidadSemillaKgHa ?? 0,
                                Costo = model.DatosEspecificos?.Costo ?? model.Costo,
                                IdCultivo = model.DatosEspecificos?.IdCultivo ?? 0,
                                IdVariedad = model.DatosEspecificos?.IdVariedad,
                                IdMetodoSiembra = model.DatosEspecificos?.IdMetodoSiembra ?? 0
                            };
                            break;

                        case "riego":
                            actividad = new Riego
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                HorasRiego = model.DatosEspecificos?.HorasRiego ?? 0,
                                VolumenAguaM3 = model.DatosEspecificos?.VolumenAguaM3 ?? 0,
                                IdMetodoRiego = model.DatosEspecificos?.IdMetodoRiego ?? 0,
                                IdFuenteAgua = model.DatosEspecificos?.IdFuenteAgua,
                                Costo = model.DatosEspecificos?.Costo
                            };
                            break;

                        case "fertilizado":
                            actividad = new Fertilizacion
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                CantidadKgHa = model.DatosEspecificos?.CantidadKgHa ?? 0,
                                DosisKgHa = model.DatosEspecificos?.DosisKgHa ?? 0,
                                Costo = model.DatosEspecificos?.Costo ?? model.Costo,
                                IdNutriente = model.DatosEspecificos?.IdNutriente ?? 0,
                                IdTipoFertilizante = model.DatosEspecificos?.IdTipoFertilizante ?? 0,
                                IdMetodoAplicacion = model.DatosEspecificos?.IdMetodoAplicacion ?? 0
                            };
                            break;

                        case "pulverizacion":
                            actividad = new Pulverizacion
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                VolumenLitrosHa = model.DatosEspecificos?.VolumenLitrosHa ?? 0,
                                Dosis = model.DatosEspecificos?.Dosis ?? 0,
                                CondicionesClimaticas = model.DatosEspecificos?.CondicionesClimaticas ?? string.Empty,
                                IdProductoAgroquimico = model.DatosEspecificos?.IdProductoAgroquimico ?? 0,
                                Costo = model.DatosEspecificos?.Costo
                            };
                            break;

                        case "monitoreo":
                            actividad = new Monitoreo
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                IdTipoMonitoreo = model.DatosEspecificos?.IdTipoMonitoreo ?? 0,
                                IdEstadoFenologico = model.DatosEspecificos?.IdEstadoFenologico
                            };
                            break;

                        case "analisissuelo":
                            actividad = new AnalisisSuelo
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                ProfundidadCm = model.DatosEspecificos?.ProfundidadCm,
                                PH = model.DatosEspecificos?.PH,
                                MateriaOrganica = model.DatosEspecificos?.MateriaOrganica,
                                Nitrogeno = model.DatosEspecificos?.Nitrogeno,
                                Fosforo = model.DatosEspecificos?.Fosforo,
                                Potasio = model.DatosEspecificos?.Potasio,
                                ConductividadElectrica = model.DatosEspecificos?.ConductividadElectrica,
                                CIC = model.DatosEspecificos?.CIC,
                                Textura = model.DatosEspecificos?.Textura ?? string.Empty,
                                IdLaboratorio = model.DatosEspecificos?.IdLaboratorio
                            };
                            break;

                        case "cosecha":
                            actividad = new Cosecha
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario,
                                RendimientoTonHa = model.DatosEspecificos?.RendimientoTonHa ?? 0,
                                HumedadGrano = model.DatosEspecificos?.HumedadGrano ?? 0,
                                SuperficieCosechadaHa = model.DatosEspecificos?.SuperficieCosechadaHa ?? 0,
                                IdCultivo = model.DatosEspecificos?.IdCultivo ?? 0
                            };
                            break;

                        case "otras labores":
                            actividad = new OtraLabor
                            {
                                Fecha = model.Fecha,
                                IdTipoActividad = model.TipoidActividad,
                                Observacion = model.Observacion ?? string.Empty,
                                IdLote = loteId,
                                IdUsuario = user.IdUsuario
                            };
                            break;
                    }

                    actividad.IdCampania = user.IdCampaña;
                    actividad.RegistrationDate = TimeHelper.GetArgentinaTime();
                    actividad.RegistrationUser = user.UserName;
                    actividad.IdLicencia = user.IdLicencia;

                    actividades.Add(actividad);
                }

                await _service.SaveActividadAsync(actividades);

                // Actualizar stock de insumo si se usó
                if (model.idInsumo.HasValue && model.Cantidad.HasValue)
                {
                    // Aquí llamarías a tu servicio para actualizar el stock
                    // await _insumoService.ActualizarStock(model.idInsumo.Value, -model.Cantidad.Value);
                }

                // Crear actividades
                //var resultActividad = await _service.CreateRangeAsync(actividades);
                //if (!resultActividad.Success)
                //{
                //    return Json(new { success = false, message = resultActividad.ErrorMessage });
                //}

                gResponse.Success = true;
                gResponse.Message = "Actividad creada correctamente";

                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear labor rápida");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetRecent()
        {
            var user = ValidarAutorizacion();

            var gResponse = new GenericResponse<LaborDTO>();

            var result = await _service.GetLaboresByAsync(user.IdCampaña);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = result.Data;
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }


        private UserAuth ValidarAutorizacion(Roles[]? rolesPermitidos = null)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("El usuario no esta autenticado");

            var claimUser = HttpContext.User;

            var userName = UtilidadService.GetClaimValue<string>(claimUser, ClaimTypes.Name) ?? string.Empty;

            var userAuth = new UserAuth
            {
                UserName = userName,
                IdLicencia = UtilidadService.GetClaimValue<int>(claimUser, "Licencia"),
                IdCampaña = UtilidadService.GetClaimValue<int>(claimUser, "Campania"),
                IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, ClaimTypes.Role)
            };

            if (rolesPermitidos != null)
            {
                userAuth.Result = rolesPermitidos.Contains((Roles)userAuth.IdRol);

                if (!userAuth.Result)
                {
                    var controller = ControllerContext.ActionDescriptor.ControllerName;
                    var action = ControllerContext.ActionDescriptor.ActionName;
                    var fullUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

                    _logger.LogError($"Acceso denegado: Usuario {userName} intento acceder a {controller}/{action} ({fullUrl})");

                    throw new AccessViolationException($" * * * * {userName} USUARIO CON PERMISOS INSUFICIENTES * * * * ");
                }
            }

            return userAuth;
        }

    }
}