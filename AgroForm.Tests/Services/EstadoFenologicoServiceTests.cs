using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class EstadoFenologicoServiceTests : ServiceTestBase
    {
        private readonly IEstadoFenologicoService _estadoFenologicoService;

        public EstadoFenologicoServiceTests()
        {
            _estadoFenologicoService = GetService<IEstadoFenologicoService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodosLosEstados()
        {
            // Arrange
            var estado1 = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Germinación", 
                Descripcion = "Estado de germinación",
                Activo = true
            };
            var estado2 = new EstadoFenologico 
            { 
                Id = 2, 
                Nombre = "Floración", 
                Descripcion = "Estado de floración",
                Activo = true
            };
            var estado3 = new EstadoFenologico 
            { 
                Id = 3, 
                Nombre = "Maduración", 
                Descripcion = "Estado de maduración",
                Activo = false
            };

            await AddTestDataAsync(estado1);
            await AddTestDataAsync(estado2);
            await AddTestDataAsync(estado3);

            // Act
            var result = await _estadoFenologicoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todos los estados (sin filtro por licencia)
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _estadoFenologicoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarTodosLosEstados()
        {
            // Arrange
            var estado1 = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Germinación", 
                Descripcion = "Estado de germinación",
                Activo = true
            };
            var estado2 = new EstadoFenologico 
            { 
                Id = 2, 
                Nombre = "Floración", 
                Descripcion = "Estado de floración",
                Activo = false
            };

            await AddTestDataAsync(estado1);
            await AddTestDataAsync(estado2);

            // Act
            var result = await _estadoFenologicoService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todos los estados (sin filtro por licencia)
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var estado = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado Test", 
                Descripcion = "Descripción test",
                Activo = true
            };
            await AddTestDataAsync(estado);

            // Act
            var result = await _estadoFenologicoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Estado Test", result.Data.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _estadoFenologicoService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var estado = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado Test", 
                Descripcion = "Descripción test",
                Activo = true
            };
            await AddTestDataAsync(estado);

            // Act
            var result = await _estadoFenologicoService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearNuevoEstado()
        {
            // Arrange
            var nuevoEstado = new EstadoFenologico 
            { 
                Nombre = "Nuevo Estado", 
                Descripcion = "Nueva descripción",
                Activo = true
            };

            // Act
            var result = await _estadoFenologicoService.CreateAsync(nuevoEstado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarEstadoExistente()
        {
            // Arrange
            var estadoOriginal = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado Original", 
                Descripcion = "Descripción original",
                Activo = true,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(estadoOriginal);

            var estadoActualizado = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado Actualizado", 
                Descripcion = "Descripción actualizada",
                Activo = false,
                RegistrationUser = "usuario_original" // Preservar el usuario original
                // No incluir RegistrationDate, IdLicencia
            };

            // Act
            var result = await _estadoFenologicoService.UpdateAsync(estadoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Estado Actualizado", result.Data.Nombre);
            Assert.Equal("Descripción actualizada", result.Data.Descripcion);
            Assert.False(result.Data.Activo);
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var estadoInexistente = new EstadoFenologico 
            { 
                Id = 999, 
                Nombre = "Estado Inexistente", 
                Descripcion = "Descripción",
                Activo = true
            };

            // Act
            var result = await _estadoFenologicoService.UpdateAsync(estadoInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarEstadoExistente()
        {
            // Arrange
            var estado = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado a Eliminar", 
                Descripcion = "Descripción",
                Activo = true
            };
            await AddTestDataAsync(estado);

            // Act
            var result = await _estadoFenologicoService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Act
            var result = await _estadoFenologicoService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var estado = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado Test", 
                Descripcion = "Descripción",
                Activo = true
            };
            await AddTestDataAsync(estado);

            // Act
            var exists = await _estadoFenologicoService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _estadoFenologicoService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryable()
        {
            // Arrange
            var estado1 = new EstadoFenologico 
            { 
                Id = 1, 
                Nombre = "Estado 1", 
                Descripcion = "Descripción 1",
                Activo = true
            };
            var estado2 = new EstadoFenologico 
            { 
                Id = 2, 
                Nombre = "Estado 2", 
                Descripcion = "Descripción 2",
                Activo = true
            };
            await AddTestDataAsync(estado1);
            await AddTestDataAsync(estado2);

            // Act
            var query = _estadoFenologicoService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todos los estados (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todos los estados (sin filtro por licencia)
        }
    }
}
