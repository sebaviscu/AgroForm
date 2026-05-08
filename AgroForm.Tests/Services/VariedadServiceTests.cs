using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Data.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class VariedadServiceTests : ServiceTestBase
    {
        private IVariedadService _variedadService;

        public VariedadServiceTests()
        {
            _variedadService = GetService<IVariedadService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad1 = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var variedad2 = new Variedad 
            { 
                Id = 2, 
                Nombre = "Trigo Candeal", 
                IdCultivo = 1,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var variedadOtraLicencia = new Variedad 
            { 
                Id = 3, 
                Nombre = "Trigo Otro", 
                IdCultivo = 1,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(variedad1);
            await AddTestDataAsync(variedad2);
            await AddTestDataAsync(variedadOtraLicencia);

            // Act
            var result = await _variedadService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todas las variedades (sin filtro por licencia)
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _variedadService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad1 = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            var variedadOtraLicencia = new Variedad 
            { 
                Id = 2, 
                Nombre = "Trigo Otro", 
                IdCultivo = 1,
                Activo = true
            };

            await AddTestDataAsync(variedad1);
            await AddTestDataAsync(variedadOtraLicencia);

            // Act
            var result = await _variedadService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todas las variedades (sin filtro por licencia)
            Assert.Equal(1, result.Data[0].Id);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad);

            // Act
            var result = await _variedadService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Trigo Pan", result.Data.Nombre);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _variedadService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoExiste()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad);

            // Act
            var result = await _variedadService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Trigo Pan", result.Data.Nombre);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad);

            // Act
            var result = await _variedadService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeAsignarDatosPorDefecto()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var nuevaVariedad = new Variedad 
            { 
                Nombre = "Nueva Variedad", 
                IdCultivo = 1,
                Activo = true
                // No asignar IdLicencia ni IdCampania (entidad sin licencia)
            };

            // Act
            var result = await _variedadService.CreateAsync(nuevaVariedad);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedadOriginal = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Original", 
                IdCultivo = 1,
                Activo = true,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(variedadOriginal);

            var variedadActualizada = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Actualizado", 
                IdCultivo = 1,
                Activo = false
                // No incluir RegistrationDate, RegistrationUser, IdLicencia
            };

            // Act
            var result = await _variedadService.UpdateAsync(variedadActualizada);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Trigo Actualizado", result.Data.Nombre);
            Assert.Equal(1, result.Data.IdCultivo);
            Assert.False(result.Data.Activo);
            Assert.Equal(1, result.Data.Id); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
            Assert.Equal(variedadOriginal.RegistrationDate, result.Data.RegistrationDate); // Preservado
            Assert.NotNull(result.Data.ModificationDate);
            Assert.Equal(TestUserAuth.UserName, result.Data.ModificationUser);
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var variedadNoExistente = new Variedad 
            { 
                Id = 999, 
                Nombre = "Inexistente", 
                IdCultivo = 1,
                Activo = true
            };

            // Act
            var result = await _variedadService.UpdateAsync(variedadNoExistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El registro que intenta actualizar no existe.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            var cultivo2 = new Cultivo { Id = 2, Nombre = "Maíz" };
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(cultivo2);

            var variedadMismaLicencia = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            var variedadOtraLicencia = new Variedad 
            { 
                Id = 2, 
                Nombre = "Maíz Dulce", 
                IdCultivo = 2,
                Activo = true
            };
            await AddTestDataAsync(variedadMismaLicencia);
            await AddTestDataAsync(variedadOtraLicencia);

            // Act
            var result = await _variedadService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var variedadesRestantes = await context.Set<Variedad>().ToListAsync();
            Assert.Single(variedadesRestantes);
            Assert.Equal(2, variedadesRestantes[0].Id); // Solo queda el de otra licencia
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _variedadService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El registro que intenta eliminar no existe.", result.ErrorMessage);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad);

            // Act
            var exists = await _variedadService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _variedadService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ValidateAsync_DebeRetornarExitoso_CuandoEntidadValida()
        {
            // Arrange
            var variedad = new Variedad 
            { 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };

            // Act
            var result = await _variedadService.ValidateAsync(variedad);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            var cultivo2 = new Cultivo { Id = 2, Nombre = "Maíz" };
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(cultivo2);

            var variedad1 = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            var variedad2 = new Variedad 
            { 
                Id = 2, 
                Nombre = "Maíz Dulce", 
                IdCultivo = 2,
                Activo = true
            };
            await AddTestDataAsync(variedad1);
            await AddTestDataAsync(variedad2);

            // Act
            var query = _variedadService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todas las variedades (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todas las variedades (sin filtro por licencia)
            Assert.Equal(1, resultados[0].Id);
        }

        // Tests específicos para el método GetByCultivo

        [Fact]
        public async Task GetByCultivo_DebeRetornarVariedades_CuandoCultivoValido()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            var variedad1 = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            var variedad2 = new Variedad 
            { 
                Id = 2, 
                Nombre = "Trigo Candeal", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad1);
            await AddTestDataAsync(variedad2);

            // Act
            var result = await _variedadService.GetByCultivo(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, v => Assert.Equal(1, v.IdCultivo));
        }

        [Fact]
        public async Task GetByCultivo_DebeRetornarVacio_CuandoCultivoNoTieneVariedades()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            await AddTestDataAsync(cultivo);

            // Act
            var result = await _variedadService.GetByCultivo(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Variedad por idCultivo no encontrado", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByCultivo_DebeRetornarError_CuandoCultivoNoExiste()
        {
            // Act
            var result = await _variedadService.GetByCultivo(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Variedad por idCultivo no encontrado", result.ErrorMessage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetByCultivo_DebeRetornarError_CuandoIdCultivoInvalido(int idCultivoInvalido)
        {
            // Act
            var result = await _variedadService.GetByCultivo(idCultivoInvalido);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Variedad por idCultivo no encontrado", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByCultivo_DebeRetornarVariedades_DeDiferentesLicencias_SinFiltro()
        {
            // Arrange
            var cultivo = new Cultivo { Id = 1, Nombre = "Trigo" };
            var cultivo2 = new Cultivo { Id = 2, Nombre = "Maíz" };
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(cultivo2);

            var variedad1 = new Variedad 
            { 
                Id = 1, 
                Nombre = "Trigo Pan", 
                IdCultivo = 1,
                Activo = true
            };
            var variedad2 = new Variedad 
            { 
                Id = 2, 
                Nombre = "Trigo Candeal", 
                IdCultivo = 1,
                Activo = true
            };
            await AddTestDataAsync(variedad1);
            await AddTestDataAsync(variedad2);

            // Act
            var result = await _variedadService.GetByCultivo(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // GetByCultivo no filtra por licencia
            Assert.All(result.Data, v => Assert.Equal(1, v.IdCultivo));
        }
    }
}
