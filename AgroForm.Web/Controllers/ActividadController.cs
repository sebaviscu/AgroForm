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
        private readonly IMonedaService _monedaService;
        private readonly ICampaniaService _campaniaService;
        protected string CurrentUser => HttpContext?.User?.Identity?.Name ?? "Anonimo";

        public ActividadController(ILogger<ActividadController> logger, IMapper mapper, IActividadService service, ICampoService campoService, ILoteService loteService, IMonedaService monedaService, ICampaniaService campaniaService)
        {
            _logger = logger;
            _mapper = mapper;
            _service = service;
            _campoService = campoService;
            _loteService = loteService;
            _monedaService = monedaService;
            _campaniaService = campaniaService;
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
                    Labores = actividades.Data
                };

                // Agregar opción "TODOS"
                vm.Campos.Insert(0, new SelectListItem { Value = "", Text = "TODOS", Selected = true });

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditarLabor([FromBody] ActividadRapidaVM model)
        {
            var gResponse = new GenericResponse<int>();
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    gResponse.Success = false;
                    gResponse.Message = $"Ah ocurrido un error: {string.Join(", ", errors)}";
                    return BadRequest(gResponse);
                }

                var tipoCambioUSD = await _monedaService.ObtenerTipoCambioActualAsync();

                var actividad = ArmarLabor(model, user, tipoCambioUSD.TipoCambioReferencia);

                await _service.UpdateActividadAsync(actividad);

                gResponse.Success = true;
                gResponse.Message = "Labor editada correctamente";

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

        private static ILabor ArmarLabor(ActividadRapidaVM model, UserAuth user, decimal tipoCambioUSD)
        {
            ILabor actividad = null;

            switch ((TipoActividadEnum)model.TipoidActividad)
            {
                case TipoActividadEnum.Siembra:
                    actividad = new Siembra
                    {
                        SuperficieHa = model.DatosEspecificos?.SuperficieHa,
                        DensidadSemillaKgHa = model.DatosEspecificos?.DensidadSemillaKgHa,
                        IdVariedad = model.DatosEspecificos?.IdVariedad,
                        IdMetodoSiembra = model.DatosEspecificos?.IdMetodoSiembra,
                        IdCultivo = model.DatosEspecificos?.IdCultivo ?? throw new Exception("Cultivo es requerido para la siembra")
                    };
                    break;

                case TipoActividadEnum.Riego:
                    actividad = new Riego
                    {
                        HorasRiego = model.DatosEspecificos?.HorasRiego,
                        VolumenAguaM3 = model.DatosEspecificos?.VolumenAguaM3,
                        IdMetodoRiego = model.DatosEspecificos?.IdMetodoRiego,
                        IdFuenteAgua = model.DatosEspecificos?.IdFuenteAgua
                    };
                    break;

                case TipoActividadEnum.Fertilizado:
                    actividad = new Fertilizacion
                    {
                        CantidadKgHa = model.DatosEspecificos?.CantidadKgHa,
                        DosisKgHa = model.DatosEspecificos?.DosisKgHa,
                        IdNutriente = model.DatosEspecificos?.IdNutriente,
                        IdTipoFertilizante = model.DatosEspecificos?.IdTipoFertilizante,
                        IdMetodoAplicacion = model.DatosEspecificos?.IdMetodoAplicacion,
                    };
                    break;

                case TipoActividadEnum.Pulverizacion:
                    actividad = new Pulverizacion
                    {
                        VolumenLitrosHa = model.DatosEspecificos?.VolumenLitrosHa,
                        Dosis = model.DatosEspecificos?.Dosis,
                        CondicionesClimaticas = model.DatosEspecificos?.CondicionesClimaticas ?? string.Empty,
                        IdProductoAgroquimico = model.DatosEspecificos?.IdProductoAgroquimico
                    };
                    break;

                case TipoActividadEnum.Monitoreo:
                    actividad = new Monitoreo
                    {
                        IdTipoMonitoreo = model.DatosEspecificos?.IdTipoMonitoreo,
                        IdEstadoFenologico = model.DatosEspecificos?.IdEstadoFenologico,
                        IdMonitoreo = model.DatosEspecificos.IdMonitoreo.Value
                    };
                    break;

                case TipoActividadEnum.AnalisisSuelo:
                    actividad = new AnalisisSuelo
                    {
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

                case TipoActividadEnum.Cosecha:
                    actividad = new Cosecha
                    {
                        RendimientoTonHa = model.DatosEspecificos?.RendimientoTonHa,
                        HumedadGrano = model.DatosEspecificos?.HumedadGrano,
                        SuperficieCosechadaHa = model.DatosEspecificos?.SuperficieCosechadaHa,
                        IdCultivo = model.DatosEspecificos?.IdCultivo ?? throw new Exception("Cultivo es requerido para la cosecha")
                    };
                    break;

                case TipoActividadEnum.OtrasLabores:
                    actividad = new OtraLabor
                    {

                    };
                    break;
            }

            var esDolar = model.DatosEspecificos?.EsDolar == true;
            //actividad.IdUsuario = user.IdUsuario,;
            if (model.IdLabor.HasValue)
                actividad.Id = model.IdLabor.GetValueOrDefault();

            actividad.IdCampania = user.IdCampaña;
            actividad.RegistrationDate = TimeHelper.GetArgentinaTime();
            actividad.RegistrationUser = user.UserName;
            actividad.IdLicencia = user.IdLicencia;
            actividad.Costo = model.DatosEspecificos?.Costo;
            actividad.IdMoneda = esDolar ? (int)Monedas.Dolar : (int)Monedas.Peso;
            actividad.Fecha = model.Fecha;
            actividad.IdTipoActividad = model.TipoidActividad;
            actividad.Observacion = model.Observacion ?? string.Empty;
            actividad.IdLote = model.idLote ?? throw new Exception("No se envió Id del lote.");
            actividad.CostoARS = UtilidadService.CalcularCostoARS(actividad.Costo, esDolar, tipoCambioUSD);
            actividad.CostoUSD = UtilidadService.CalcularCostoUSD(actividad.Costo, esDolar, tipoCambioUSD);

            return actividad;
        }

        [HttpPost]
        public async Task<IActionResult> CrearLabor([FromBody] ActividadRapidaVM model)
        {
            var gResponse = new GenericResponse<int>();
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    gResponse.Success = false;
                    gResponse.Message = $"Ah ocurrido un error: {string.Join(", ", errors)}";
                    return BadRequest(gResponse);
                }

                var tipoCambioUSD = await _monedaService.ObtenerTipoCambioActualAsync();
                var actividades = new List<ILabor>();

                // TODO
                //if(model.LotesIds.Count > 1)
                //{
                //    var costo = model.DatosEspecificos?.Costo;
                //    var lotes = await _loteService.GetByIds(model.LotesIds);
                //    if(lotes.Success)
                //    {
                //        var sumaSup = lotes.Data.Sum(l => l.SuperficieHectareas);
                //    }
                //}

                foreach (var loteId in model.LotesIds)
                {
                    model.idLote = loteId;
                    var actividad = ArmarLabor(model, user, tipoCambioUSD.TipoCambioReferencia);
                    actividades.Add(actividad);
                }

                var campania = await _campaniaService.GetCurrent();
                if (campania.Success && campania.Data.EstadosCampania == EstadosCamapaña.Iniciada)
                {
                    campania.Data.EstadosCampania = EstadosCamapaña.EnCurso;
                    await _campaniaService.UpdateAsync(campania.Data);
                }

                var response = await _service.SaveActividadAsync(actividades);

                gResponse.Success = response.Success;
                gResponse.Message = response.ErrorMessage;

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

        [HttpGet]
        public async Task<IActionResult> GetBy(int id, int idTipoActividad)
        {
            var gResponse = new GenericResponse<object>();

            try
            {
                var entity = await _service.GetLaboresByAsync(id, (TipoActividadEnum)idTipoActividad);

                gResponse.Success = true;
                gResponse.Object = entity;
                gResponse.Message = "Labor recuperado correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recuperar labor");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id, int idTipoActividad)
        {
            var gResponse = new GenericResponse<int>();

            try
            {
                await _service.DeteleActividadAsync(id, (TipoActividadEnum)idTipoActividad);

                gResponse.Success = true;
                gResponse.Message = "Labor eliminada correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al borrar labor");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

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