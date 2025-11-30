using AgroForm.Business.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class AccessController : Controller
    {
        private readonly IUsuarioService _userService;
        private readonly ICampaniaService _campaniaService;
        private readonly IWebHostEnvironment _env;

        public AccessController(IUsuarioService userService, ICampaniaService campaniaService, IWebHostEnvironment env)
        {
            _userService = userService;
            _campaniaService = campaniaService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                if (claimuser.Identity.IsAuthenticated)
                {
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
            //if (_env.IsDevelopment())
            //{
            //    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            //    {
            //        var isValid = await _userService.ValidateUserAsync(email, password);
            //        if (isValid)
            //        {
            //            var user = await _userService.GetUserByEmailAsync(email);
            //            var campania = await _campaniaService.GetCurrent();
            //            var idCampania = campania.Data != null ? campania.Data.Id.ToString() : null;

            //            var claims = new List<Claim>
            //            {
            //                new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
            //                new Claim(ClaimTypes.Name, user.Nombre),
            //                new Claim("Licencia", user.IdLicencia.ToString()),
            //                new Claim("Campania", idCampania),
            //                new Claim(ClaimTypes.Role, user.Rol.ToString())
            //            };
        
            //            await CreateAuthenticationCookie(claims, rememberMe);
            //            return RedirectToAction("Index", "Home");
            //        }
            //    }

                var campaniaDev = await _campaniaService.GetCurrentByLicencia(1);

                var devClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "Desarrollador"),
                    new Claim("Licencia", "1"),
                    new Claim("Campania", campaniaDev.Data?.Id.ToString() ?? "1"),
                    new Claim(ClaimTypes.Role, "0"),
                    new Claim("Moneda", ((int)Monedas.DolarOficial).ToString()),
                };
        
                await CreateAuthenticationCookie(devClaims, true);
                
                return RedirectToAction("Index", "Home");
            //}
        
            //// Código de producción
            //if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            //{
            //    ViewBag.Error = "Email y contraseña son requeridos";
            //    return View();
            //}
        
            //var isValidProd = await _userService.ValidateUserAsync(email, password);
        
            //if (!isValidProd)
            //{
            //    ViewBag.Error = "Credenciales inválidas";
            //    return View();
            //}
        
            //var userProd = await _userService.GetUserByEmailAsync(email);
            //var campaniaProd = await _campaniaService.GetCurrent();
        
            //var prodClaims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.NameIdentifier, userProd!.Id.ToString()),
            //    new Claim(ClaimTypes.Name, userProd.Nombre),
            //    new Claim("Licencia", userProd.IdLicencia.ToString()),
            //    new Claim("Campania", campaniaProd.Data?.Id.ToString() ?? "1"),
            //    new Claim(ClaimTypes.Role, userProd.Rol.ToString())
            //};
        
            //await CreateAuthenticationCookie(prodClaims, rememberMe);
            //return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
