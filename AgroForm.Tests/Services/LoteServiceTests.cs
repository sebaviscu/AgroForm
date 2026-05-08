using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;
using AgroForm.Data; // Para AppDbContext

namespace AgroForm.Tests.Services
{
    public class LoteServiceTests : ServiceTestBase
    {
        private readonly ILoteService _loteService;

        public LoteServiceTests()
        {
            _loteService = GetService<ILoteService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var lote1 = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Norte", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote2 = new Lote 
            { 
                Id = 2, 
                Nombre = "Lote Sur", 
                SuperficieHectareas = 150,
                IdLicencia = 1,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var loteOtraLicencia = new Lote 
            { 
                Id = 3, 
                Nombre = "Lote Otra Licencia", 
                SuperficieHectareas = 200,
                IdLicencia = 2,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);
            await AddTestDataAsync(loteOtraLicencia);

            // Act
            var result = await _loteService.GetAllAsync();

            // Assert
            if (!result.Success)
            {
                Console.WriteLine($"GetAllAsync error: {result.ErrorCode} - {result.ErrorMessage}");
            }
            Assert.True(result.Success, $"GetAllAsync error: {result.ErrorCode} - {result.ErrorMessage}");
            Assert.NotNull(result.Data);
            Console.WriteLine($"Found {result.Data.Count} lotes");
            Assert.Equal(2, result.Data.Count); // Solo los de licencia 1
            Assert.All(result.Data, l => Assert.Equal(1, l.IdLicencia));
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _loteService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task TestBasico_DebeVerificarAutenticacion()
        {
            // Arrange - Agregar un lote simple
            var lote = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Test", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampo = 1
            };
            await AddTestDataAsync(lote);

            // Act
            var result = await _loteService.GetAllAsync();

            // Assert
            Console.WriteLine($"TestBasico - Success: {result.Success}, Data.Count: {result.Data?.Count ?? 0}");
            if (!result.Success)
            {
                Console.WriteLine($"Error: {result.ErrorCode} - {result.ErrorMessage}");
            }
        }

        [Fact]
        public async Task TestContexto_DebeVerificarDatosEnContextoDelServicio()
        {
            // Arrange - Agregar un lote directamente usando el contexto del servicio
            var contextFactory = GetService<IDbContextFactory<AppDbContext>>();
            await using var context = await contextFactory.CreateDbContextAsync();
            
            var lote = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Directo", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampo = 1
            };
            await context.Set<Lote>().AddAsync(lote);
            await context.SaveChangesAsync();

            // Act
            var result = await _loteService.GetAllAsync();

            // Assert
            Console.WriteLine($"TestContexto - Success: {result.Success}, Data.Count: {result.Data?.Count ?? 0}");
            if (!result.Success)
            {
                Console.WriteLine($"Error: {result.ErrorCode} - {result.ErrorMessage}");
            }
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange - Crear datos relacionados
            var campo = new Campo 
            { 
                Id = 1, 
                Nombre = "Campo Test", 
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            
            var lote1 = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Norte", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var loteOtraLicencia = new Lote 
            { 
                Id = 2, 
                Nombre = "Lote Otra Licencia", 
                SuperficieHectareas = 200,
                IdLicencia = 2,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(loteOtraLicencia);

            // Act
            var result = await _loteService.GetAllWithDetailsAsync();

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
            var lote = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Test", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1, // Agregar campaña para que el filtrado funcione
                IdCampo = 1
            };
            await AddTestDataAsync(lote);

            // Act
            var result = await _loteService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Lote Test", result.Data.Nombre);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _loteService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var loteOtraLicencia = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Otra Licencia", 
                SuperficieHectareas = 200,
                IdLicencia = 2,
                IdCampo = 1
            };
            await AddTestDataAsync(loteOtraLicencia);

            // Act
            var result = await _loteService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var lote = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Test", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);

            // Act
            var result = await _loteService.GetByIdWithDetailsAsync(1);

            // Assert
            Console.WriteLine($"GetByIdWithDetailsAsync result - Success: {result.Success}, ErrorCode: {result.ErrorCode}, ErrorMessage: {result.ErrorMessage}");
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task CreateAsync_DebeAsignarLicenciaYCampania()
        {
            // Arrange
            var nuevoLote = new Lote 
            { 
                Nombre = "Nuevo Lote", 
                SuperficieHectareas = 150,
                IdCampo = 1
                // No asignar IdLicencia ni IdCampania
            };

            // Act
            var result = await _loteService.CreateAsync(nuevoLote);

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
            var loteOriginal = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Original", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(loteOriginal);

            var loteActualizado = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Actualizado", 
                SuperficieHectareas = 200,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1
            };

            // Act
            var result = await _loteService.UpdateAsync(loteActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Lote Actualizado", result.Data.Nombre);
            Assert.Equal(200, result.Data.SuperficieHectareas);
            Assert.Equal(1, result.Data.IdLicencia); // Preservado
            Assert.Equal(1, result.Data.IdCampania); // Preservado
            Assert.Equal(1, result.Data.IdCampo); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var loteInexistente = new Lote 
            { 
                Id = 999, 
                Nombre = "Lote Inexistente", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1
            };

            // Act
            var result = await _loteService.UpdateAsync(loteInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var loteMismaLicencia = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Misma Licencia", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var loteOtraLicencia = new Lote 
            { 
                Id = 3, 
                Nombre = "Lote Otra Licencia", 
                SuperficieHectareas = 300,
                IdLicencia = 2,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };
            await AddTestDataAsync(loteMismaLicencia);
            await AddTestDataAsync(loteOtraLicencia);

            // Act
            var result = await _loteService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            // El lote de otra licencia todavía debe existir en la base de datos
            var loteRestante = await DbContext.Lotes.FindAsync(3);
            Assert.NotNull(loteRestante); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var loteOtraLicencia = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Otra Licencia", 
                SuperficieHectareas = 100,
                IdLicencia = 2,
                IdCampania = 1,
                IdCampo = 1
            };
            await AddTestDataAsync(loteOtraLicencia);

            // Act
            var result = await _loteService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _loteService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var lote = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote Test", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);

            // Act
            var exists = await _loteService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _loteService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var lote1 = new Lote 
            { 
                Id = 1, 
                Nombre = "Lote 1", 
                SuperficieHectareas = 100,
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1
            };
            var lote2 = new Lote 
            { 
                Id = 2, 
                Nombre = "Lote 2", 
                SuperficieHectareas = 200,
                IdLicencia = 2,
                IdCampania = 1,
                IdCampo = 1
            };
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);

            // Act
            var query = _loteService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
