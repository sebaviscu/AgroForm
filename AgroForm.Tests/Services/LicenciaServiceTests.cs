using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class LicenciaServiceTests : ServiceTestBase
    {
        private readonly ILicenciaService _licenciaService;

        public LicenciaServiceTests()
        {
            _licenciaService = GetService<ILicenciaService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodasLasLicencias()
        {
            // Arrange
            var licencia1 = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Premium", 
                NombreContacto = "Contacto Premium",
                Activo = true
            };
            var licencia2 = new Licencia 
            { 
                Id = 2, 
                RazonSocial = "Empresa Básica", 
                Activo = true
            };
            var licencia3 = new Licencia 
            { 
                Id = 3, 
                RazonSocial = "Empresa Test", 
                NombreContacto = "Contacto Test",
                Activo = false
            };

            await AddTestDataAsync(licencia1);
            await AddTestDataAsync(licencia2);
            await AddTestDataAsync(licencia3);

            // Act
            var result = await _licenciaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todas las licencias (sin filtro por licencia)
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _licenciaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarTodasLasLicencias()
        {
            // Arrange
            var licencia1 = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Premium", 
                NombreContacto = "Contacto Premium",
                Activo = true
            };
            var licencia2 = new Licencia 
            { 
                Id = 2, 
                RazonSocial = "Empresa Básica", 
                NombreContacto = "Contacto Básico",
                Activo = false
            };

            await AddTestDataAsync(licencia1);
            await AddTestDataAsync(licencia2);

            // Act
            var result = await _licenciaService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todas las licencias (sin filtro por licencia)
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var licencia = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Test", 
                NombreContacto = "Contacto Test",
                Activo = true
            };
            await AddTestDataAsync(licencia);

            // Act
            var result = await _licenciaService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Empresa Test", result.Data.RazonSocial);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _licenciaService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró la licencia", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var licencia = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Test", 
                NombreContacto = "Contacto Test",
                Activo = true
            };
            await AddTestDataAsync(licencia);

            // Act
            var result = await _licenciaService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearNuevaLicencia()
        {
            // Arrange
            var nuevaLicencia = new Licencia 
            { 
                RazonSocial = "Nueva Empresa", 
                NombreContacto = "Nuevo Contacto",
                Activo = true
            };

            // Act
            var result = await _licenciaService.CreateAsync(nuevaLicencia);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarLicenciaExistente()
        {
            // Arrange
            var licenciaOriginal = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Original", 
                NombreContacto = "Contacto Original",
                Activo = true,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(licenciaOriginal);

            var licenciaActualizada = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Actualizada", 
                NombreContacto = "Contacto Actualizado",
                Activo = false
            };

            // Act
            var result = await _licenciaService.UpdateAsync(licenciaActualizada);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Empresa Actualizada", result.Data.RazonSocial);
            Assert.Equal("Contacto Actualizado", result.Data.NombreContacto);
            Assert.False(result.Data.Activo);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var licenciaInexistente = new Licencia 
            { 
                Id = 999, 
                RazonSocial = "Empresa Inexistente", 
                NombreContacto = "Contacto Inexistente",
                Activo = true
            };

            // Act
            var result = await _licenciaService.UpdateAsync(licenciaInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarLicenciaExistente()
        {
            // Arrange
            var licencia = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa a Eliminar", 
                NombreContacto = "Contacto a Eliminar",
                Activo = true
            };
            await AddTestDataAsync(licencia);

            // Act
            var result = await _licenciaService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _licenciaService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var licencia = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa Test", 
                NombreContacto = "Contacto Test",
                Activo = true
            };
            await AddTestDataAsync(licencia);

            // Act
            var exists = await _licenciaService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _licenciaService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryable()
        {
            // Arrange
            var licencia1 = new Licencia 
            { 
                Id = 1, 
                RazonSocial = "Empresa 1", 
                NombreContacto = "Contacto 1",
                Activo = true
            };
            var licencia2 = new Licencia 
            { 
                Id = 2, 
                RazonSocial = "Empresa 2", 
                NombreContacto = "Contacto 2",
                Activo = true
            };
            await AddTestDataAsync(licencia1);
            await AddTestDataAsync(licencia2);

            // Act
            var query = _licenciaService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todas las licencias (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todas las licencias (sin filtro por licencia)
        }
    }
}
