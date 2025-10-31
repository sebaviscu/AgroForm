using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public abstract class BaseController<TEntity, TService> : Controller
        where TEntity : EntityBase
        where TService : IServiceBase<TEntity>
    {
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;
        protected readonly TService _service;
        protected readonly GenericResponse<TEntity> gResponse = new GenericResponse<TEntity>();
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

            var userName = GetClaimValue<string>(claimUser, ClaimTypes.Name) ?? string.Empty;

            var userAuth = new UserAuth
            {
                UserName = userName,
                IdLicencia = GetClaimValue<int>(claimUser, "Licencia"),
                IdUsuario = GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                IdRol = GetClaimValue<Roles>(claimUser, ClaimTypes.Role)
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

        protected T GetClaimValue<T>(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim == null)
                return default;

            try
            {
                if (typeof(T).IsEnum)
                {
                    if (Enum.TryParse(typeof(T), claim.Value, out var result))
                    {
                        return (T)result;
                    }
                    return default;
                }

                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        protected IActionResult HandleException(
            Exception? ex,
            string errorMessage,

            object model = null,
            params (string Key, object Value)[] additionalData)
        {
            if (ex is UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }

            var gResponse = new GenericResponse<object>
            {
                State = false,
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
            var logTemplate = $" - - - - - - ❌ - - - - - - [Controller: {controllerName}] Usuario: {CurrentUser}. {{ErrorMessage}}";

            foreach (var key in logParams.Keys.Where(k => k != "ErrorMessage"))
            {
                logTemplate += $", {key}: {{{key}}}";
            }

            logTemplate += " - - - - - - ❌ - - - - - - ";

            _logger.LogError(ex, logTemplate, logParams);

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }
    }
}
