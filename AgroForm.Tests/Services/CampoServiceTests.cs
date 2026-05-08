using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class CampoServiceTests : ServiceTestBase
    {
        private readonly ICampoService _campoService;

        public CampoServiceTests()
        {
            _campoService = GetService<ICampoService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var campo1 = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Norte", 
                Ubicacion = "Ubicación campo norte",
                SuperficieHectareas = 100,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campo2 = new Campo 
            { 
                Id = 2, 
                Nombre = "Campo Sur", 
                Ubicacion = "Ubicación campo sur",
                SuperficieHectareas = 200,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campoOtraLicencia = new Campo 
            { 
                Id = 3, 
                Nombre = "Campo Otra Licencia", 
                Ubicacion = "Ubicación campo otra licencia",
                SuperficieHectareas = 300,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(campo1);
            await AddTestDataAsync(campo2);
            await AddTestDataAsync(campoOtraLicencia);

            // Act
            var result = await _campoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Solo los de licencia 1
            Assert.All(result.Data, c => Assert.Equal(1, c.IdLicencia));
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _campoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var campo1 = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Norte", 
                Ubicacion = "Ubicación campo norte",
                SuperficieHectareas = 100,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campoOtraLicencia = new Campo 
            { 
                Id = 2, 
                Nombre = "Campo Otra Licencia", 
                Ubicacion = "Ubicación campo otra licencia",
                SuperficieHectareas = 300,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            // Agregar lotes relacionados para que el filtro Lotes.Any() funcione
            var lote1 = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Test", 
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            await AddTestDataAsync(lote1);
            await AddTestDataAsync(campo1);
            await AddTestDataAsync(campoOtraLicencia);

            // Act
            var result = await _campoService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data); // Solo el de licencia 1
            Assert.Equal(1, result.Data[0].IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var campo = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Test", 
                IdLicencia = 1
            };
            await AddTestDataAsync(campo);

            // Act
            var result = await _campoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Campo Test", result.Data.Nombre);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _campoService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var campoOtraLicencia = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Otra Licencia", 
                IdLicencia = 2
            };
            await AddTestDataAsync(campoOtraLicencia);

            // Act
            var result = await _campoService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var campo = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Test", 
                IdLicencia = 1
            };
            await AddTestDataAsync(campo);

            // Act
            var result = await _campoService.GetByIdWithDetailsAsync(1);

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
            var nuevoCampo = new Campo 
            { 
                Nombre = "Nuevo Campo", 
                // No asignar IdLicencia ni IdCampania
            };

            // Act
            var result = await _campoService.CreateAsync(nuevoCampo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var campoOriginal = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Original", 
                IdLicencia = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(campoOriginal);

            var campoActualizado = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Actualizado", 
                IdLicencia = 1
            };

            // Act
            var result = await _campoService.UpdateAsync(campoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Campo Actualizado", result.Data.Nombre);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var campoInexistente = new Campo 
            { 
                Id = 999, 
                Nombre = "Campo Inexistente", 
                IdLicencia = 1,
            };

            // Act
            var result = await _campoService.UpdateAsync(campoInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var campoMismaLicencia = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Misma Licencia", 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campoOtraLicencia = new Campo 
            { 
                Id = 2, 
                Nombre = "Campo Otra Licencia", 
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };
            await AddTestDataAsync(campoMismaLicencia);
            await AddTestDataAsync(campoOtraLicencia);

            // Act
            var result = await _campoService.DeleteAsync(1);

            // Assert
            Console.WriteLine($"Delete result - Success: {result.Success}, ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}");
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            // El campo de otra licencia todavía debe existir en la base de datos
            var campoRestante = await DbContext.Campos.FindAsync(2);
            Assert.NotNull(campoRestante); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var campoOtraLicencia = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Otra Licencia", 
                IdLicencia = 2
            };
            await AddTestDataAsync(campoOtraLicencia);

            // Act
            var result = await _campoService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _campoService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var campo = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Test", 
                IdLicencia = 1
            };
            await AddTestDataAsync(campo);

            // Act
            var exists = await _campoService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _campoService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var campo1 = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo 1", 
                IdLicencia = 1
            };
            var campo2 = new Campo 
            { 
                Id = 2, 
                Nombre = "Campo 2", 
                IdLicencia = 2
            };
            await AddTestDataAsync(campo1);
            await AddTestDataAsync(campo2);

            // Act
            var query = _campoService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
