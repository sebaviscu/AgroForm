using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using AgroForm.Model.Configuracion;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class AccessController : Controller
    {
        private readonly ILogger<AccessController> _logger;
        private readonly IUsuarioService _userService;
        private readonly ICampaniaService _campaniaService;
        private readonly ILicenciaService _licenciaService;
        private readonly IUserContext _userContext;
        private readonly IWebHostEnvironment _env;

        public AccessController(ILogger<AccessController> logger, IUsuarioService userService, ICampaniaService campaniaService, ILicenciaService licenciaService, IUserContext userContext, IWebHostEnvironment env)
        {
            _logger = logger;
            _userService = userService;
            _campaniaService = campaniaService;
            _licenciaService = licenciaService;
            _userContext = userContext;
            _env = env;
        }

        protected async Task<UserAuth> ValidarAutorizacion(Roles[]? rolesPermitidos = null)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("El usuario no esta autenticado");

            var claimUser = HttpContext.User;

            var userName = UtilidadService.GetClaimValue<string>(claimUser, ClaimTypes.Name) ?? string.Empty;

            var userAuth = new UserAuth
            {
                UserName = userName,
                IdLicencia = UtilidadService.GetClaimValue<int?>(claimUser, "Licencia"),
                IdCampaña = UtilidadService.GetClaimValue<int?>(claimUser, "Campania"),
                IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, ClaimTypes.Role),
                Moneda = UtilidadService.GetClaimValue<Monedas>(claimUser, "Moneda")
            };

            // Validar licencia (excepto para SuperAdmin)
            if (userAuth.IdRol != Roles.SuperAdmin)
            {
                if (!userAuth.IdLicencia.HasValue)
                {
                    _logger.LogError($"Usuario {userName} sin licencia asignada");
                    throw new UnauthorizedAccessException("Usuario sin licencia asignada. Contacte al administrador.");
                }

                var licenciaResult = await _licenciaService.GetByIdAsync(userAuth.IdLicencia.Value);

                if (!licenciaResult.Success || licenciaResult.Data == null || !licenciaResult.Data.Activo)
                {
                    _logger.LogError($"Licencia {userAuth.IdLicencia} del usuario {userName} esta deshabilitada o no existe");
                    throw new UnauthorizedAccessException("Su licencia está deshabilitada. Contacte al administrador.");
                }

                // Validar que la licencia de prueba no haya expirado
                if (licenciaResult.Data.EsPrueba && licenciaResult.Data.FechaFinPrueba.HasValue)
                {
                    if (licenciaResult.Data.FechaFinPrueba.Value.Date <= TimeHelper.GetArgentinaTime().Date)
                    {
                        _logger.LogError($"Licencia de prueba {userAuth.IdLicencia} del usuario {userName} ha expirado el {licenciaResult.Data.FechaFinPrueba.Value:yyyy-MM-dd}");
                        throw new UnauthorizedAccessException("Su licencia de prueba ha expirado. Contacte al administrador.");
                    }
                }
            }

            if (rolesPermitidos != null)
            {
                // SuperAdmin siempre tiene acceso (incluso cuando "simula" una licencia)
                userAuth.Result = userAuth.IdRol == Roles.SuperAdmin || rolesPermitidos.Contains((Roles)userAuth.IdRol);

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

            var controllerName = GetType().Name;
            var logTemplate = $" - - - - - - ❌ - - - - - - Usuario: {HttpContext.User?.Identity?.Name}. [Controller: {controllerName}]  Endpint: {endpint}";

            _logger.LogError(ex, logTemplate, logParams);

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                if (claimuser.Identity.IsAuthenticated)
                {
                    var userAuth = await ValidarAutorizacion();
                    
                    if (userAuth.IdRol == Roles.SuperAdmin)
                    {
                        return RedirectToAction("Index", "Administrador");
                    }

                    return RedirectToAction("Index", "Home");
                }
                return View();

            }
            catch (Exception ex)
            {
                ViewBag["Error"] = $"Error: {ex.ToString()}.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            //if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            //{
            //    ViewBag.Error = "Email y contraseña son requeridos";
            //    return View();
            //}

            //var isValid = await _userService.ValidateUserAsync(email, password);

            //if (!isValid)
            //{
            //    ViewBag.Error = "Credenciales inválidas";
            //    return View();
            //}

            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                ViewBag.Error = "Usuario no encontrado";
                return View();
            }

            // Validar que la licencia esté activa (excepto para SuperAdmin)
            if (!user.SuperAdmin)
            {
                if (!user.IdLicencia.HasValue)
                {
                    ViewBag.Error = "Usuario sin licencia asignada. Contacte al administrador.";
                    return View();
                }

                var licenciaResult = await _licenciaService.GetByIdAsync(user.IdLicencia.Value);
                if (!licenciaResult.Success || !licenciaResult.Data.Activo)
                {
                    ViewBag.Error = "Su licencia está deshabilitada. Contacte al administrador.";
                    return View();
                }

                // Validar que la licencia de prueba no haya expirado
                if (licenciaResult.Data.EsPrueba && licenciaResult.Data.FechaFinPrueba.HasValue)
                {
                    if (licenciaResult.Data.FechaFinPrueba.Value.Date <= TimeHelper.GetArgentinaTime().Date)
                    {
                        ViewBag.Error = "Su licencia de prueba ha expirado. Contacte al administrador.";
                        return View();
                    }
                }
            }
            
            // Buscar campaña actual
            var campaniaResult = await _campaniaService.GetCurrentByLicencia(user.IdLicencia);
            var idCampania = campaniaResult.Data?.Id.ToString() ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim("Licencia", user.IdLicencia.ToString()),
                new Claim("Campania", idCampania),
                new Claim("Moneda", user.IdMonedaReferencia.HasValue ? ((int)Monedas.DolarOficial).ToString() : ((int)Monedas.Peso).ToString()),
                new Claim("IdMonedaReferencia", user.IdMonedaReferencia?.ToString() ?? ""),
                new Claim(ClaimTypes.Role, ((int)user.Rol).ToString())
            };

            await CreateAuthenticationCookie(claims, rememberMe);

            if (user.SuperAdmin)
            {
                var simulacionActiva = HttpContext.Session.GetString("SimulacionLicenciaId");
                if (!string.IsNullOrEmpty(simulacionActiva))
                {
                    return RedirectToAction("Index", "Home");
                }
                
                return RedirectToAction("Index", "Administrador");
            }

            return RedirectToAction("Index", "Home");
        }
        
        private async Task CreateAuthenticationCookie(List<Claim> claims, bool rememberMe)
        {
            var claimsIdentity = new ClaimsIdentity(claims, "AgroFormAuth");
        
            await HttpContext.SignInAsync(
                "AgroFormAuth",
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(1)
                }
            );
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AgroFormAuth");
            return RedirectToAction("Login", "Access");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            try
            {
                await ValidarAutorizacion();
                
                var result = await _userService.GetByIdAsync(id);
                
                if (result.Success)
                {
                    var usuario = result.Data;
                    var userProfile = new UserProfileVM
                    {
                        Id = usuario.Id,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        PhoneNumber = usuario.PhoneNumber
                    };
                    
                    return Json(new { success = true, data = userProfile });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener el perfil del usuario");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserProfile([FromBody] dynamic perfilData)
        {
            try
            {
                await ValidarAutorizacion();
                
                int id = perfilData.id;
                string nombre = perfilData.nombre;
                string email = perfilData.email;
                string phoneNumber = perfilData.phoneNumber;

                var result = await _userService.UpdateUserProfileAsync(id, nombre, email, phoneNumber);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Perfil actualizado correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar el perfil del usuario");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTrialLicenseInfo()
        {
            try
            {
                var userAuth = await ValidarAutorizacion();

                // SuperAdmin no tiene licencia de prueba
                if (userAuth.IdRol == Roles.SuperAdmin)
                {
                    return Json(new { success = true, esPrueba = false });
                }

                if (!userAuth.IdLicencia.HasValue)
                {
                    return Json(new { success = true, esPrueba = false });
                }

                var licenciaResult = await _licenciaService.GetByIdAsync(userAuth.IdLicencia.Value);
                if (!licenciaResult.Success || licenciaResult.Data == null)
                {
                    return Json(new { success = true, esPrueba = false });
                }

                var licencia = licenciaResult.Data;
                var today = DateTime.Now.Date;
                var result = new
                {
                    success = true,
                    esPrueba = licencia.EsPrueba,
                    fechaFinPrueba = licencia.FechaFinPrueba?.ToString("yyyy-MM-dd"),
                    expirada = licencia.EsPrueba && licencia.FechaFinPrueba.HasValue
                        ? licencia.FechaFinPrueba.Value.Date < today
                        : false,
                    diasRestantes = licencia.EsPrueba && licencia.FechaFinPrueba.HasValue
                        ? (int)(licencia.FechaFinPrueba.Value.Date - today).TotalDays
                        : 0
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener información de licencia de prueba");
            }
        }
    }
}
