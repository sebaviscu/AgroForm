using AgroForm.Model;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroForm.Business.Contracts;
using System.Security.Claims;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class SimulacionController : Controller
    {
        private readonly IUserContext _userContext;
        private readonly ICampaniaService _campaniaService;

        public SimulacionController(IUserContext userContext, ICampaniaService campaniaService)
        {
            _userContext = userContext;
            _campaniaService = campaniaService;
        }

        [HttpPost]
        public async Task<IActionResult> SimularLicencia([FromBody] int licenciaId)
        {
            try
            {
                if (!_userContext.IsSuperAdmin)
                {
                    return Json(new { success = false, message = "Acceso denegado. Solo los Super Admin pueden simular licencias." });
                }

                _userContext.SetSimulatedLicencia(licenciaId);
                
                // Guardar en sesión para persistir durante login
                HttpContext.Session.SetString("SimulacionLicenciaId", licenciaId.ToString());

                // Determinar campaña actual de la licencia simulada y persistirla también
                var campaniaResult = await _campaniaService.GetCurrentByLicencia(licenciaId);
                int? campaniaIdToSet = null;
                if (campaniaResult.Success && campaniaResult.Data != null)
                {
                    var campaniaId = campaniaResult.Data.Id;
                    campaniaIdToSet = campaniaId;
                    _userContext.SetSimulatedCampania(campaniaId);
                    HttpContext.Session.SetString("SimulacionCampaniaId", campaniaId.ToString());
                }

                // Guardar valores originales para restaurar al detener simulación
                var originalLicencia = User.FindFirst("Licencia")?.Value;
                var originalCampania = User.FindFirst("Campania")?.Value;
                if (!string.IsNullOrWhiteSpace(originalLicencia))
                    HttpContext.Session.SetString("SimulacionOriginalLicenciaId", originalLicencia);
                if (!string.IsNullOrWhiteSpace(originalCampania))
                    HttpContext.Session.SetString("SimulacionOriginalCampaniaId", originalCampania);

                // Actualizar claims para que la UI (selector campañas) refleje el contexto simulado
                await UpdateClaimAsync("Licencia", licenciaId.ToString());
                if (campaniaIdToSet.HasValue)
                {
                    await UpdateClaimAsync("Campania", campaniaIdToSet.Value.ToString());
                }

                return Json(new { 
                    success = true, 
                    message = $"Simulando licencia {licenciaId}. Ahora verás el sistema como si fueras un usuario de esa licencia.",
                    isSimulating = true,
                    licenciaId = licenciaId,
                    redirectUrl = Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al simular licencia: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetenerSimulacion()
        {
            try
            {
                if (!_userContext.IsSuperAdmin)
                {
                    return Json(new { success = false, message = "Acceso denegado." });
                }

                _userContext.ClearSimulation();

                // Restaurar claims originales si existen
                var originalLicencia = HttpContext.Session.GetString("SimulacionOriginalLicenciaId");
                var originalCampania = HttpContext.Session.GetString("SimulacionOriginalCampaniaId");
                if (!string.IsNullOrWhiteSpace(originalLicencia))
                    await UpdateClaimAsync("Licencia", originalLicencia);
                if (!string.IsNullOrWhiteSpace(originalCampania))
                    await UpdateClaimAsync("Campania", originalCampania);
                
                // Limpiar la sesión
                HttpContext.Session.Remove("SimulacionLicenciaId");
                HttpContext.Session.Remove("SimulacionCampaniaId");
                HttpContext.Session.Remove("SimulacionOriginalLicenciaId");
                HttpContext.Session.Remove("SimulacionOriginalCampaniaId");

                return Json(new { 
                    success = true, 
                    message = "Simulación detenida. Ahora tienes vista de Super Admin nuevamente.",
                    isSimulating = false,
                    redirectUrl = Url.Action("Index", "Administrador")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al detener simulación: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetEstadoSimulacion()
        {
            try
            {
                return Json(new { 
                    isSimulating = _userContext.IsSimulating,
                    isSuperAdmin = _userContext.IsSuperAdmin,
                    licenciaActual = _userContext.IdLicencia
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener estado: " + ex.Message });
            }
        }

        private async Task UpdateClaimAsync(string claimType, string newValue)
        {
            var user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("El usuario no está autenticado.");

            var claims = user.Claims.ToList();
            var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
            if (claimToRemove != null)
            {
                claims.Remove(claimToRemove);
            }
            claims.Add(new Claim(claimType, newValue));

            var claimsIdentity = new ClaimsIdentity(claims, "AgroFormAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var properties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = (HttpContext.Request.Cookies[".AspNetCore.Cookies"] != null)
            };

            await HttpContext.SignInAsync("AgroFormAuth", claimsPrincipal, properties);
            HttpContext.User = claimsPrincipal;
        }
    }
}
