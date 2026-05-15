using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Data;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace AgroForm.Tests.Services
{
    public class AccessControllerLoginTests
    {
        private readonly Mock<ILogger<AccessController>> _mockLogger;
        private readonly Mock<IUsuarioService> _mockUserService;
        private readonly Mock<ICampaniaService> _mockCampaniaService;
        private readonly Mock<ILicenciaService> _mockLicenciaService;
        private readonly Mock<IUserContext> _mockUserContext;

        private readonly AccessController _controller;

        public AccessControllerLoginTests()
        {
            _mockLogger = new Mock<ILogger<AccessController>>();
            _mockUserService = new Mock<IUsuarioService>();
            _mockCampaniaService = new Mock<ICampaniaService>();
            _mockLicenciaService = new Mock<ILicenciaService>();
            _mockUserContext = new Mock<IUserContext>();

            var mockWebHostEnv = new Mock<IWebHostEnvironment>();

            _controller = new AccessController(
                _mockLogger.Object,
                _mockUserService.Object,
                _mockCampaniaService.Object,
                _mockLicenciaService.Object,
                _mockUserContext.Object,
                mockWebHostEnv.Object
            );
        }

        private void SetupHttpContextForLogin()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton<IAuthenticationService>(authServiceMock.Object);
            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory, Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionaryFactory>();
            
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var sessionMock = new Mock<ISession>();
            byte[] value;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out value)).Returns(false);
            httpContext.Session = sessionMock.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            var urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            _controller.Url = urlHelperMock.Object;
        }

        [Fact]
        public async Task Login_UsuarioConLicenciaInactiva_RetornaError()
        {
            SetupHttpContextForLogin();
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1, Email = email, Nombre = "Test User", IdLicencia = 1, SuperAdmin = false
            };
            var licenciaInactiva = new Licencia { Id = 1, Activo = false };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _mockLicenciaService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(OperationResult<Licencia>.SuccessResult(licenciaInactiva));

            var result = await _controller.Login(email, password, false);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("Error"));
            Assert.Equal("Su licencia está deshabilitada. Contacte al administrador.", viewResult.ViewData["Error"]);
        }

        [Fact]
        public async Task Login_SuperAdminConLicenciaInactiva_PermiteAcceso()
        {
            SetupHttpContextForLogin();
            var email = "admin@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1, Email = email, Nombre = "Admin User", IdLicencia = 1, SuperAdmin = true
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _mockCampaniaService.Setup(x => x.GetCurrentByLicencia(1)).ReturnsAsync(OperationResult<Campania>.Failure("No hay campaña"));

            var result = await _controller.Login(email, password, false);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Administrador", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_UsuarioConLicenciaActiva_PermiteAcceso()
        {
            SetupHttpContextForLogin();
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1, Email = email, Nombre = "Test User", IdLicencia = 1, SuperAdmin = false
            };
            var licenciaActiva = new Licencia { Id = 1, Activo = true };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _mockLicenciaService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(OperationResult<Licencia>.SuccessResult(licenciaActiva));
            _mockCampaniaService.Setup(x => x.GetCurrentByLicencia(1)).ReturnsAsync(OperationResult<Campania>.Failure("No hay campaña"));

            var result = await _controller.Login(email, password, false);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_UsuarioSinLicencia_RetornaError()
        {
            SetupHttpContextForLogin();
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1, Email = email, Nombre = "Test User", IdLicencia = null, SuperAdmin = false
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);

            var result = await _controller.Login(email, password, false);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("Error"));
            Assert.Equal("Usuario sin licencia asignada. Contacte al administrador.", viewResult.ViewData["Error"]);
        }
    }
}
