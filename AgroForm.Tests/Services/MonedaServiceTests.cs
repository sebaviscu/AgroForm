using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class MonedaServiceTests : ServiceTestBase
    {
        private readonly IMonedaService _monedaService;

        public MonedaServiceTests()
        {
            _monedaService = GetService<IMonedaService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodasLasMonedas()
        {
            // Arrange
            var moneda1 = new Moneda 
            { 
                Id = 1, 
                Nombre = "Dólar Americano", 
                Codigo = "USD",
                Simbolo = "$",
            };
            var moneda2 = new Moneda 
            { 
                Id = 2, 
                Nombre = "Euro", 
                Codigo = "EUR",
                Simbolo = "€",
            };
            var moneda3 = new Moneda 
            { 
                Id = 3, 
                Nombre = "Peso Argentino", 
                Codigo = "ARS",
                Simbolo = "$",
            };

            await AddTestDataAsync(moneda1);
            await AddTestDataAsync(moneda2);
            await AddTestDataAsync(moneda3);

            // Act
            var result = await _monedaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todas las monedas (sin filtro por licencia)
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _monedaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarTodasLasMonedas()
        {
            // Arrange
            var moneda1 = new Moneda 
            { 
                Id = 1, 
                Nombre = "Dólar Americano", 
                Codigo = "USD",
                Simbolo = "$",
            };
            var moneda2 = new Moneda 
            { 
                Id = 2, 
                Nombre = "Euro", 
                Codigo = "EUR",
                Simbolo = "€",
            };

            await AddTestDataAsync(moneda1);
            await AddTestDataAsync(moneda2);

            // Act
            var result = await _monedaService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todas las monedas (sin filtro por licencia)
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var moneda = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda Test", 
                Codigo = "TEST",
                Simbolo = "T",
            };
            await AddTestDataAsync(moneda);

            // Act
            var result = await _monedaService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Moneda Test", result.Data.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _monedaService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var moneda = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda Test", 
                Codigo = "TEST",
                Simbolo = "T",
            };
            await AddTestDataAsync(moneda);

            // Act
            var result = await _monedaService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearNuevaMoneda()
        {
            // Arrange
            var nuevaMoneda = new Moneda 
            { 
                Nombre = "Nueva Moneda", 
                Codigo = "NEW",
                Simbolo = "N",
            };

            // Act
            var result = await _monedaService.CreateAsync(nuevaMoneda);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarMonedaExistente()
        {
            // Arrange
            var monedaOriginal = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda Original", 
                Codigo = "OLD",
                Simbolo = "O",
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(monedaOriginal);

            var monedaActualizada = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda Actualizada", 
                Codigo = "UPD",
                Simbolo = "U",
            };

            // Act
            var result = await _monedaService.UpdateAsync(monedaActualizada);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Moneda Actualizada", result.Data.Nombre);
            Assert.Equal("UPD", result.Data.Codigo);
            Assert.Equal("U", result.Data.Simbolo);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var monedaInexistente = new Moneda 
            { 
                Id = 999, 
                Nombre = "Moneda Inexistente", 
                Codigo = "INV",
                Simbolo = "I",
            };

            // Act
            var result = await _monedaService.UpdateAsync(monedaInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarMonedaExistente()
        {
            // Arrange
            var moneda = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda a Eliminar", 
                Codigo = "DEL",
                Simbolo = "D",
            };
            await AddTestDataAsync(moneda);

            // Act
            var result = await _monedaService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _monedaService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var moneda = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda Test", 
                Codigo = "TEST",
                Simbolo = "T",
            };
            await AddTestDataAsync(moneda);

            // Act
            var exists = await _monedaService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _monedaService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryable()
        {
            // Arrange
            var moneda1 = new Moneda 
            { 
                Id = 1, 
                Nombre = "Moneda 1", 
                Codigo = "M1",
                Simbolo = "1",
            };
            var moneda2 = new Moneda 
            { 
                Id = 2, 
                Nombre = "Moneda 2", 
                Codigo = "M2",
                Simbolo = "2",
            };
            await AddTestDataAsync(moneda1);
            await AddTestDataAsync(moneda2);

            // Act
            var query = _monedaService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todas las monedas (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todas las monedas (sin filtro por licencia)
        }
    }
}
