using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Route("[controller]/[action]")]
    public abstract class BaseController<TEntity, TDto, TService> : Controller
    where TEntity : EntityBase
    where TService : IServiceBase<TEntity>
    {
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;
        protected readonly TService _service;
        protected readonly GenericResponse<TDto> gResponse = new GenericResponse<TDto>();
        protected string CurrentUser => HttpContext?.User?.Identity?.Name ?? "Anonimo";

        public BaseController(ILogger logger, IMapper mapper, TService service)
        {
            _logger = logger;
            _mapper = mapper;
            _service = service;
        }

        protected TDest Map<TSource, TDest>(TSource source)
        {
            return _mapper.Map<TDest>(source);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllDataTable()
        {
            var result = await _service.GetAllAsync();
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            var dataVM = Map<List<TEntity>, List<TDto>>(result.Data);

            return Json(new
            {
                success = true,
                data = dataVM
            });
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = Map<List<TEntity>, List<TDto>>(result.Data);
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllWithDetailsAsync()
        {
            var result = await _service.GetAllWithDetailsAsync();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = Map<List<TEntity>, List<TDto>>(result.Data);
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }


        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return NotFound(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<TEntity, TDto>(result.Data);
            gResponse.Message = "Registro encontrado";
            return Ok(gResponse);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TDto dto)
        {
            var entity = Map<TDto, TEntity>(dto);
            var result = await _service.CreateAsync(entity);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<TEntity, TDto>(result.Data);
            gResponse.Message = "Registro creado correctamente";
            return Ok(gResponse);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Update([FromBody] TDto dto)
        {
            var entity = Map<TDto, TEntity>(dto);
            var result = await _service.UpdateAsync(entity);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<TEntity, TDto>(result.Data);
            gResponse.Message = "Registro actualizado correctamente";
            return Ok(gResponse);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Message = "Registro eliminado correctamente";
            return Ok(gResponse);
        }



        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new RedirectToActionResult("Login", "Access", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        protected UserAuth ValidarAutorizacion(Roles[]? rolesPermitidos = null)
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
        

        protected IActionResult HandleException(
            Exception? ex,
            string errorMessage,
            string endpint = "",
            object model = null,
            params (string Key, object Value)[] additionalData)
        {
            if (ex is UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }

            var gResponse = new GenericResponse<object>
            {
                Success = false,
                Message = ex == null ? errorMessage : $"{errorMessage}\n {ex.InnerException?.Message ?? ex.Message}"
            };


            var logParams = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "ExceptionMessage", ex?.Message ?? "null" },
                { "ExceptionType", ex?.GetType().FullName ?? "null" },
                { "StackTrace", ex?.StackTrace ?? "null" },
                { "Source", ex?.Source ?? "null" },
                { "HResult", ex?.HResult.ToString() ?? "null" },
                { "TargetSite", ex?.TargetSite?.ToString() ?? "null" },
                { "RequestPath", HttpContext?.Request?.Path.ToString() ?? "null" }
            };

            if (model != null)
            {
                try
                {
                    logParams["ModelRequest"] = JsonConvert.SerializeObject(model);
                }
                catch (Exception e)
                {
                    logParams["ModelRequest"] = $"Error al serializar model: {e.Message}";
                }
            }

            if (additionalData != null)
            {
                foreach (var data in additionalData)
                {
                    logParams[data.Key] = JsonConvert.SerializeObject(data.Value);
                }
            }

            var controllerName = GetType().Name;
            var logTemplate = $" - - - - - - ❌ - - - - - - Usuario: {CurrentUser}. [Controller: {controllerName}]  Endpint: {endpint}";

            foreach (var key in logParams.Keys.Where(k => k != "ErrorMessage"))
            {
                logTemplate += $", {key}: {{{key}}}";
            }

            logTemplate += " - - - - - - ❌ - - - - - - ";

            _logger.LogError(ex, logTemplate, logParams);

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }

        public async Task UpdateClaimAsync(string claimType, string? newValue = null)
        {
            var user = HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                var claims = user.Claims.ToList();

                var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
                if (claimToRemove != null)
                {
                    claims.Remove(claimToRemove);
                }

                if (newValue != null)
                {
                    claims.Add(new Claim(claimType, newValue));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "AgroFormAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Actualiza la cookie
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = (HttpContext.Request.Cookies[".AspNetCore.Cookies"] != null)
                };
                await HttpContext.SignInAsync("AgroFormAuth", claimsPrincipal, properties);

                // Actualiza el HttpContext.User
                HttpContext.User = claimsPrincipal;
            }
            else
                throw new UnauthorizedAccessException("El usuario no está autenticado.");

        }
    }
}
