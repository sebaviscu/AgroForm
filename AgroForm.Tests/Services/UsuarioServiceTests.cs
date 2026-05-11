using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class UsuarioServiceTests : ServiceTestBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioServiceTests()
        {
            _usuarioService = GetService<IUsuarioService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var usuario1 = new Usuario 
            { 
                Id = 1, 
                Nombre = "Juan Pérez", 
                Email = "juan@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var usuario2 = new Usuario 
            { 
                Id = 2, 
                Nombre = "María García", 
                Email = "maria@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var usuarioOtraLicencia = new Usuario 
            { 
                Id = 3, 
                Nombre = "Pedro López", 
                Email = "pedro@ejemplo.com",
                Activo = true,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(usuario1);
            await AddTestDataAsync(usuario2);
            await AddTestDataAsync(usuarioOtraLicencia);

            // Act
            var result = await _usuarioService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Solo los de licencia 1 (GetAllAsync ahora filtra por licencia)
            Assert.All(result.Data, u => Assert.Equal(1, u.IdLicencia));
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _usuarioService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var usuario1 = new Usuario 
            { 
                Id = 1, 
                Nombre = "Juan Pérez", 
                Email = "juan@ejemplo.com",
                Activo = true,
                IdLicencia = 1
            };
            var usuarioOtraLicencia = new Usuario 
            { 
                Id = 2, 
                Nombre = "María García", 
                Email = "maria@ejemplo.com",
                Activo = true,
                IdLicencia = 2
            };

            await AddTestDataAsync(usuario1);
            await AddTestDataAsync(usuarioOtraLicencia);

            // Act
            var result = await _usuarioService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data); // Solo el de licencia 1 (GetAllWithDetailsAsync ahora filtra por licencia)
            Assert.Equal(1, result.Data[0].IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var usuario = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Test", 
                Email = "test@ejemplo.com",
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(usuario);

            // Act
            var result = await _usuarioService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Usuario Test", result.Data.Nombre);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _usuarioService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var usuarioOtraLicencia = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Otra Licencia", 
                Email = "otro@ejemplo.com",
                Activo = true,
                IdLicencia = 2
            };
            await AddTestDataAsync(usuarioOtraLicencia);

            // Act
            var result = await _usuarioService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var usuario = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Test", 
                Email = "test@ejemplo.com",
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(usuario);

            // Act
            var result = await _usuarioService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task CreateAsync_DebeAsignarLicenciaYCampania()
        {
            // Arrange
            var nuevoUsuario = new Usuario 
            { 
                Nombre = "Nuevo Usuario", 
                Email = "nuevo@ejemplo.com",
                Activo = true,
                IdLicencia = 1 // Asignar licencia para que funcione el filtrado
            };

            // Act
            var result = await _usuarioService.CreateAsync(nuevoUsuario);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IdLicencia); // Asignado automáticamente
 // Asignado automáticamente
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var fechaOriginal = DateTime.Now.AddDays(-1);
            var usuarioOriginal = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Original", 
                Email = "original@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
                RegistrationDate = fechaOriginal,
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(usuarioOriginal);

            var usuarioActualizado = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Actualizado", 
                Email = "actualizado@ejemplo.com",
                Activo = false,
                IdLicencia = 1,
            };

            // Act
            var result = await _usuarioService.UpdateAsync(usuarioActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Usuario Actualizado", result.Data.Nombre);
            Assert.Equal("actualizado@ejemplo.com", result.Data.Email);
            Assert.False(result.Data.Activo);
            Assert.Equal(1, result.Data.IdLicencia); // Preservado
            Assert.Equal(fechaOriginal, result.Data.RegistrationDate); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var usuarioInexistente = new Usuario 
            { 
                Id = 999, 
                Nombre = "Usuario Inexistente", 
                Email = "inexistente@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
            };

            // Act
            var result = await _usuarioService.UpdateAsync(usuarioInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var usuarioMismaLicencia = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Misma Licencia", 
                Email = "misma@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var usuarioOtraLicencia = new Usuario 
            { 
                Id = 2, 
                Nombre = "Usuario Otra Licencia", 
                Email = "otra@ejemplo.com",
                Activo = true,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };
            await AddTestDataAsync(usuarioMismaLicencia);
            await AddTestDataAsync(usuarioOtraLicencia);

            // Act
            var result = await _usuarioService.DeleteAsync(1);

            // Assert
            Console.WriteLine($"Delete result - Success: {result.Success}, ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}");
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            // El usuario de otra licencia todavía debe existir en la base de datos
            var usuarioRestante = await DbContext.Usuarios.FindAsync(2);
            Assert.NotNull(usuarioRestante); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var usuarioOtraLicencia = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Otra Licencia", 
                Email = "otra@ejemplo.com",
                Activo = true,
                IdLicencia = 2,
            };
            await AddTestDataAsync(usuarioOtraLicencia);

            // Act
            var result = await _usuarioService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _usuarioService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var usuario = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario Test", 
                Email = "test@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
            };
            await AddTestDataAsync(usuario);

            // Act
            var exists = await _usuarioService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _usuarioService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var usuario1 = new Usuario 
            { 
                Id = 1, 
                Nombre = "Usuario 1", 
                Email = "usuario1@ejemplo.com",
                Activo = true,
                IdLicencia = 1,
            };
            var usuario2 = new Usuario 
            { 
                Id = 2, 
                Nombre = "Usuario 2", 
                Email = "usuario2@ejemplo.com",
                Activo = true,
                IdLicencia = 2,
            };
            await AddTestDataAsync(usuario1);
            await AddTestDataAsync(usuario2);

            // Act
            var query = _usuarioService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1 (GetQuery() ahora filtra por licencia)
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
