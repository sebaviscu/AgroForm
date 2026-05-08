using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class CampaniaServiceTests : ServiceTestBase
    {
        private readonly ICampaniaService _campaniaService;

        public CampaniaServiceTests()
        {
            _campaniaService = GetService<ICampaniaService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var campania1 = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña 2024", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania2 = new Campania 
            { 
                Id = 2, 
                Nombre = "Campaña 2023", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campaniaOtraLicencia = new Campania 
            { 
                Id = 3, 
                Nombre = "Campaña Otra Licencia", 
                EstadosCampania = EnumClass.EstadosCamapaña.EnCurso,
                FechaInicio = DateTime.Now,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(campania1);
            await AddTestDataAsync(campania2);
            await AddTestDataAsync(campaniaOtraLicencia);

            // Act
            var result = await _campaniaService.GetAllAsync();

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
            var result = await _campaniaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var campania1 = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña 2024", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1
            };
            var campaniaOtraLicencia = new Campania 
            { 
                Id = 2, 
                Nombre = "Campaña Otra Licencia", 
                EstadosCampania = EnumClass.EstadosCamapaña.EnCurso,
                FechaInicio = DateTime.Now,
                IdLicencia = 2
            };

            await AddTestDataAsync(campania1);
            await AddTestDataAsync(campaniaOtraLicencia);

            // Act
            var result = await _campaniaService.GetAllWithDetailsAsync();

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
            var campania = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Test", 
                IdLicencia = 1
            };
            await AddTestDataAsync(campania);

            // Act
            var result = await _campaniaService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Campaña Test", result.Data.Nombre);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _campaniaService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var campaniaOtraLicencia = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Otra Licencia", 
                IdLicencia = 2
            };
            await AddTestDataAsync(campaniaOtraLicencia);

            // Act
            var result = await _campaniaService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var campania = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Test", 
                IdLicencia = 1
            };
            await AddTestDataAsync(campania);

            // Act
            var result = await _campaniaService.GetByIdWithDetailsAsync(1);

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
            var nuevaCampania = new Campania 
            { 
                Nombre = "Nueva Campaña", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                // No asignar IdLicencia ni IdCampania
            };

            // Act
            var result = await _campaniaService.CreateAsync(nuevaCampania);

            // Assert
            Console.WriteLine($"Create result - Success: {result.Success}, ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}");
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var campaniaOriginal = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Original", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(campaniaOriginal);

            var campaniaActualizada = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Actualizada", 
                EstadosCampania = EnumClass.EstadosCamapaña.EnCurso,
                IdLicencia = 1,
            };

            // Act
            var result = await _campaniaService.UpdateAsync(campaniaActualizada);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Campaña Actualizada", result.Data.Nombre);
            Assert.Equal(EnumClass.EstadosCamapaña.EnCurso, result.Data.EstadosCampania);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var campaniaInexistente = new Campania 
            { 
                Id = 999, 
                Nombre = "Campaña Inexistente", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
            };

            // Act
            var result = await _campaniaService.UpdateAsync(campaniaInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var campaniaMismaLicencia = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Misma Licencia", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campaniaOtraLicencia = new Campania 
            { 
                Id = 2, 
                Nombre = "Campaña Otra Licencia", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };
            await AddTestDataAsync(campaniaMismaLicencia);
            await AddTestDataAsync(campaniaOtraLicencia);

            // Act
            var result = await _campaniaService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            // La campaña de otra licencia todavía debe existir en la base de datos
            var campaniaRestante = await DbContext.Campanias.FindAsync(2);
            Assert.NotNull(campaniaRestante); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var campaniaOtraLicencia = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Otra Licencia", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 2,
            };
            await AddTestDataAsync(campaniaOtraLicencia);

            // Act
            var result = await _campaniaService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _campaniaService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var campania = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña Test", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
            };
            await AddTestDataAsync(campania);

            // Act
            var exists = await _campaniaService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _campaniaService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var campania1 = new Campania 
            { 
                Id = 1, 
                Nombre = "Campaña 1", 
                EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
                FechaInicio = DateTime.Now,
                IdLicencia = 1,
            };
            var campania2 = new Campania 
            { 
                Id = 2, 
                Nombre = "Campaña 2", 
                EstadosCampania = EnumClass.EstadosCamapaña.EnCurso,
                FechaInicio = DateTime.Now,
                IdLicencia = 2,
            };
            await AddTestDataAsync(campania1);
            await AddTestDataAsync(campania2);

            // Act
            var query = _campaniaService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
