using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class AjusteServiceTests : ServiceTestBase
    {
        private readonly IAjusteService _ajusteService;

        public AjusteServiceTests()
        {
            _ajusteService = GetService<IAjusteService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodosLosAjustes()
        {
            // Arrange
            var ajuste1 = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ajuste2 = new Ajuste 
            { 
                Id = 2, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ajuste3 = new Ajuste 
            { 
                Id = 3, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            await AddTestDataAsync(ajuste1);
            await AddTestDataAsync(ajuste2);
            await AddTestDataAsync(ajuste3);

            // Act
            var result = await _ajusteService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todos los ajustes (sin filtro por licencia)
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _ajusteService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarTodosLosAjustes()
        {
            // Arrange
            var ajuste1 = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ajuste2 = new Ajuste 
            { 
                Id = 2, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            await AddTestDataAsync(ajuste1);
            await AddTestDataAsync(ajuste2);

            // Act
            var result = await _ajusteService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todos los ajustes (sin filtro por licencia)
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var ajuste = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ajuste);

            // Act
            var result = await _ajusteService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _ajusteService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var ajuste = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ajuste);

            // Act
            var result = await _ajusteService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearNuevoAjuste()
        {
            // Arrange
            var nuevoAjuste = new Ajuste 
            { 
                IdLicencia = 1
            };

            // Act
            var result = await _ajusteService.CreateAsync(nuevoAjuste);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarAjusteExistente()
        {
            // Arrange
            var ajusteOriginal = new Ajuste 
            { 
                Id = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(ajusteOriginal);

            var ajusteActualizado = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1
            };

            // Act
            var result = await _ajusteService.UpdateAsync(ajusteActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var ajusteInexistente = new Ajuste 
            { 
                Id = 999, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            // Act
            var result = await _ajusteService.UpdateAsync(ajusteInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarAjusteExistente()
        {
            // Arrange
            var ajuste = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ajuste);

            // Act
            var result = await _ajusteService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _ajusteService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var ajuste = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ajuste);

            // Act
            var exists = await _ajusteService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _ajusteService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryable()
        {
            // Arrange
            var ajuste1 = new Ajuste 
            { 
                Id = 1, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ajuste2 = new Ajuste 
            { 
                Id = 2, 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ajuste1);
            await AddTestDataAsync(ajuste2);

            // Act
            var query = _ajusteService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todos los ajustes (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todos los ajustes (sin filtro por licencia)
        }
    }
}
