using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgroForm.Web.Controllers
{
    public class AccessController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICampaniaService _campaniaService;
        public AccessController(IAuthService authService, ICampaniaService campaniaService)
        {
            _authService = authService;
            _campaniaService = campaniaService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email y contraseña son requeridos";
                return View();
            }

            //var isValid = await _authService.ValidateUserAsync(email, password);

            //if (!isValid)
            //{
            //    ViewBag.Error = "Credenciales inválidas";
            //    return View();
            //}

            var campania = await _campaniaService.GetCurrent();

            //var user = await _authService.GetUserByEmailAsync(email);

            //var claims = new List<Claim>
            //    {
            //        new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
            //        new Claim(ClaimTypes.Name, user.Nombre),
            //        new Claim("Licencia", user.IdLicencia.ToString()),
            //        new Claim(ClaimTypes.Role, user.Rol.ToString())
            //    };


            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "seba"),
                    new Claim("Licencia", "1"),
                    new Claim("Campania", campania.Data != null ? campania.Data.Id.ToString() : "1"),
                    new Claim(ClaimTypes.Role, "0")
                };

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


            return RedirectToAction("Index", "Home");
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