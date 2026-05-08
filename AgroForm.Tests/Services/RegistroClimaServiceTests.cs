using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class RegistroClimaServiceTests : ServiceTestBase
    {
        private readonly IRegistroClimaService _registroClimaService;

        public RegistroClimaServiceTests()
        {
            _registroClimaService = GetService<IRegistroClimaService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var registro1 = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-2),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro seco",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var registro2 = new RegistroClima 
            { 
                Id = 2, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 5,
                TipoClima = EnumClass.TipoClima.Granizo,
                Observaciones = "Registro lluvioso",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var registroOtraLicencia = new RegistroClima 
            { 
                Id = 3, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdCampo = 1,
                IdLicencia = 2,
                IdCampania = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(registro1);
            await AddTestDataAsync(registro2);
            await AddTestDataAsync(registroOtraLicencia);

            // Act
            var result = await _registroClimaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Solo los de licencia 1
            Assert.All(result.Data, r => Assert.Equal(1, r.IdLicencia));
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _registroClimaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var registro1 = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-2),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro seco",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            var registroOtraLicencia = new RegistroClima 
            { 
                Id = 2, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdCampo = 1,
                IdLicencia = 2,
                IdCampania = 1
            };

            await AddTestDataAsync(registro1);
            await AddTestDataAsync(registroOtraLicencia);

            // Act
            var result = await _registroClimaService.GetAllWithDetailsAsync();

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
            var registro = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(registro);

            // Act
            var result = await _registroClimaService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(0, result.Data.Milimetros);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _registroClimaService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var registroOtraLicencia = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(registroOtraLicencia);

            // Act
            var result = await _registroClimaService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var registro = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(registro);

            // Act
            var result = await _registroClimaService.GetByIdWithDetailsAsync(1);

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
            var nuevoRegistro = new RegistroClima 
            { 
                Fecha = DateTime.Now,
                // No asignar IdLicencia ni IdCampania
            };

            // Act
            var result = await _registroClimaService.CreateAsync(nuevoRegistro);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IdLicencia); // Asignado automáticamente
            Assert.Equal(1, result.Data.IdCampania); // Asignado automáticamente
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var registroOriginal = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(registroOriginal);

            var registroActualizado = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now,
                Milimetros = 5,
                TipoClima = EnumClass.TipoClima.Granizo,
                Observaciones = "Registro actualizado",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };

            // Act
            var result = await _registroClimaService.UpdateAsync(registroActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(DateTime.Now.Date, result.Data.Fecha.Date);
            Assert.Equal(1, result.Data.IdLicencia); // Preservado
            Assert.Equal(1, result.Data.IdCampania); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var registroInexistente = new RegistroClima 
            { 
                Id = 999, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };

            // Act
            var result = await _registroClimaService.UpdateAsync(registroInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var registroMismaLicencia = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            var registroOtraLicencia = new RegistroClima 
            { 
                Id = 2, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(registroMismaLicencia);
            await AddTestDataAsync(registroOtraLicencia);

            // Act
            var result = await _registroClimaService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            var registroRestante = await _registroClimaService.GetByIdAsync(2);
            Assert.True(registroRestante.Success); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var registroOtraLicencia = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(registroOtraLicencia);

            // Act
            var result = await _registroClimaService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _registroClimaService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var registro = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-1),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(registro);

            // Act
            var exists = await _registroClimaService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _registroClimaService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var registro1 = new RegistroClima 
            { 
                Id = 1, 
                Fecha = DateTime.Now.AddDays(-2),
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro test",
                IdCampo = 1,
                IdLicencia = 1,
                IdCampania = 1
            };
            var registro2 = new RegistroClima 
            { 
                Id = 2, 
                Fecha = DateTime.Now,
                Milimetros = 0,
                TipoClima = EnumClass.TipoClima.Lluvia,
                Observaciones = "Registro de otra licencia",
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(registro1);
            await AddTestDataAsync(registro2);

            // Act
            var query = _registroClimaService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
