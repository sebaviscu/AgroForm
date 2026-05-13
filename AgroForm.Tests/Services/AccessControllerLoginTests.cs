using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AgroForm.Data;
using AgroForm.Model.Configuracion;

namespace AgroForm.Tests.Services
{
    public class AccessControllerLoginTests
    {
        private readonly Mock<ILogger<AccessController>> _mockLogger;
        private readonly Mock<IUsuarioService> _mockUserService;
        private readonly Mock<ICampaniaService> _mockCampaniaService;
        private readonly Mock<ILicenciaService> _mockLicenciaService;
        private readonly AccessController _controller;

        public AccessControllerLoginTests()
        {
            _mockLogger = new Mock<ILogger<AccessController>>();
            _mockUserService = new Mock<IUsuarioService>();
            _mockCampaniaService = new Mock<ICampaniaService>();
            _mockLicenciaService = new Mock<ILicenciaService>();

            var mockWebHostEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            _controller = new AccessController(
                _mockLogger.Object,
                _mockUserService.Object,
                _mockCampaniaService.Object,
                _mockLicenciaService.Object,
                mockWebHostEnv.Object
            );
        }

        [Fact]
        public async Task Login_UsuarioConLicenciaInactiva_RetornaError()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1,
                Email = email,
                Nombre = "Test User",
                IdLicencia = 1,
                SuperAdmin = false
            };

            var licenciaInactiva = new Licencia
            {
                Id = 1,
                Activo = false
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            _mockLicenciaService.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(OperationResult<Licencia>.SuccessResult(licenciaInactiva));

            // Act
            var result = await _controller.Login(email, password, false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("Error"));
            Assert.Equal("Su licencia está deshabilitada. Contacte al administrador.", viewResult.ViewData["Error"]);
        }

        [Fact]
        public async Task Login_SuperAdminConLicenciaInactiva_PermiteAcceso()
        {
            // Arrange
            var email = "admin@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1,
                Email = email,
                Nombre = "Admin User",
                IdLicencia = 1,
                SuperAdmin = true
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            _mockCampaniaService.Setup(x => x.GetCurrentByLicencia(1))
                .ReturnsAsync(OperationResult<Campania>.Failure("No hay campaña"));

            // Act
            var result = await _controller.Login(email, password, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Administrador", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_UsuarioConLicenciaActiva_PermiteAcceso()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1,
                Email = email,
                Nombre = "Test User",
                IdLicencia = 1,
                SuperAdmin = false
            };

            var licenciaActiva = new Licencia
            {
                Id = 1,
                Activo = true
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            _mockLicenciaService.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(OperationResult<Licencia>.SuccessResult(licenciaActiva));

            _mockCampaniaService.Setup(x => x.GetCurrentByLicencia(1))
                .ReturnsAsync(OperationResult<Campania>.Failure("No hay campaña"));

            // Act
            var result = await _controller.Login(email, password, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_UsuarioSinLicencia_RetornaError()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new Usuario
            {
                Id = 1,
                Email = email,
                Nombre = "Test User",
                IdLicencia = null,
                SuperAdmin = false
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(email, password, false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("Error"));
            Assert.Equal("Usuario sin licencia asignada. Contacte al administrador.", viewResult.ViewData["Error"]);
        }
    }
}
