using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using Xunit;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public class CicloCultivoServiceTests : ServiceTestBase
    {
        private readonly ICicloCultivoService _cicloCultivoService;

        public CicloCultivoServiceTests()
        {
            _cicloCultivoService = GetService<ICicloCultivoService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodosLosCiclos_CuandoExisten()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo1 = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ciclo2 = new CicloCultivo
            {
                Id = 2,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.segunda,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            await AddTestDataAsync(ciclo1);
            await AddTestDataAsync(ciclo2);

            // Act
            var result = await _cicloCultivoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _cicloCultivoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCiclo_CuandoIdValido()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Act
            var result = await _cicloCultivoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(EpocaSiembra.primera, result.Data.Epoca);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cicloCultivoService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearCiclo_CuandoDatosValidos()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                IdLote = 1,
                IdCultivo = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera
            };

            // Act
            var result = await _cicloCultivoService.CreateAsync(ciclo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IdLote);
            Assert.Equal(1, result.Data.IdCultivo);
            Assert.Equal(EpocaSiembra.primera, result.Data.Epoca);
            Assert.NotNull(result.Data.FechaInicio);
            Assert.Null(result.Data.FechaFin);
        }

        [Fact]
        public async Task CreateAsync_DebeRetornarCicloExistente_CuandoYaHayActivo()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var cicloExistente = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-10),
                FechaFin = null,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(cicloExistente);

            var nuevoCiclo = new CicloCultivo
            {
                IdLote = 1,
                IdCultivo = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera
            };

            // Act
            var result = await _cicloCultivoService.CreateAsync(nuevoCiclo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id); // Returns existing cycle
            Assert.Null(result.Data.FechaFin); // Still active
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarCiclo_CuandoExiste()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Clear change tracker to avoid EF tracking conflicts
            // when the service calls UpdateAsync (which does AsNoTracking get + _context.Update)
            DbContext.ChangeTracker.Clear();

            // Create a NEW entity instance with modified values (detached from context)
            var cicloActualizado = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.segunda,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };

            // Act
            var result = await _cicloCultivoService.UpdateAsync(cicloActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(EpocaSiembra.segunda, result.Data.Epoca);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarCiclo_CuandoExiste()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Clear change tracker to avoid EF tracking conflict
            // when DeleteAsync does AsNoTracking get + _context.Remove
            DbContext.ChangeTracker.Clear();

            // Act
            var result = await _cicloCultivoService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);

            var verifyResult = await _cicloCultivoService.GetByIdAsync(1);
            Assert.False(verifyResult.Success);
            Assert.Equal("NOT_FOUND", verifyResult.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cicloCultivoService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task CrearCicloAsync_DebeCrearCiclo_CuandoDatosValidos()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            // Act
            var result = await _cicloCultivoService.CrearCicloAsync(1, 1, EpocaSiembra.primera);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IdLote);
            Assert.Equal(1, result.Data.IdCultivo);
            Assert.Equal(EpocaSiembra.primera, result.Data.Epoca);
            Assert.NotNull(result.Data.FechaInicio);
            Assert.Null(result.Data.FechaFin);
        }

        [Fact]
        public async Task CerrarCicloAsync_DebeCerrarCiclo_CuandoExiste()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-30),
                FechaFin = null,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Clear change tracker to avoid EF tracking conflict
            // when CerrarCicloAsync does GetAsync (AsNoTracking) then UpdateAsync
            DbContext.ChangeTracker.Clear();

            // Act
            var result = await _cicloCultivoService.CerrarCicloAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.FechaFin);
        }

        [Fact]
        public async Task CerrarCicloAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cicloCultivoService.CerrarCicloAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ObtenerCicloActivoAsync_DebeRetornarCicloActivo_CuandoExiste()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var cicloActivo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-10),
                FechaFin = null,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cicloCerrado = new CicloCultivo
            {
                Id = 2,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.segunda,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-60),
                FechaFin = TimeHelper.GetArgentinaTime().AddDays(-30),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(cicloActivo);
            await AddTestDataAsync(cicloCerrado);

            // Act
            var result = await _cicloCultivoService.ObtenerCicloActivoAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Null(result.Data.FechaFin);
        }

        [Fact]
        public async Task ObtenerCicloActivoAsync_DebeRetornarNull_CuandoNoHayCicloActivo()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            // Act
            var result = await _cicloCultivoService.ObtenerCicloActivoAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task ObtenerCicloActivoAsync_DebeFiltrarPorEpoca_CuandoSeEspecifica()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var cicloPrimera = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-10),
                FechaFin = null,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cicloSegunda = new CicloCultivo
            {
                Id = 2,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.segunda,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-5),
                FechaFin = null,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(cicloPrimera);
            await AddTestDataAsync(cicloSegunda);

            // Act
            var result = await _cicloCultivoService.ObtenerCicloActivoAsync(1, EpocaSiembra.segunda);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Id);
            Assert.Equal(EpocaSiembra.segunda, result.Data.Epoca);
        }

        [Fact]
        public async Task ObtenerCiclosPorLoteAsync_DebeRetornarCiclos_CuandoExisten()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo1 = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-10),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var ciclo2 = new CicloCultivo
            {
                Id = 2,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.segunda,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-5),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo1);
            await AddTestDataAsync(ciclo2);

            // Act
            var result = await _cicloCultivoService.ObtenerCiclosPorLoteAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.Equal(1, c.IdLote));
        }

        [Fact]
        public async Task ObtenerCiclosPorLoteAsync_DebeRetornarVacio_CuandoNoHayCiclos()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);

            // Act
            var result = await _cicloCultivoService.ObtenerCiclosPorLoteAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllByCamapniaAsync_DebeRetornarCiclos_DeLaCampaniaActual()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Act
            var result = await _cicloCultivoService.GetAllByCamapniaAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].IdCampania);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCicloConDetalles_CuandoExiste()
        {
            // Arrange
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote Test",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(campania);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime(),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            // Act
            var result = await _cicloCultivoService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.NotNull(result.Data.Cultivo);
            Assert.Equal("Maíz", result.Data.Cultivo.Nombre);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cicloCultivoService.GetByIdWithDetailsAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }
    }
}
