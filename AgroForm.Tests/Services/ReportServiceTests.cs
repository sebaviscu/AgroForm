using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using Xunit;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public class ReportServiceTests : ServiceTestBase
    {
        private readonly IReportService _reportService;

        public ReportServiceTests()
        {
            _reportService = GetService<IReportService>();
        }

        [Fact]
        public async Task GetComparativaCamposAsync_DebeRetornarResultados_CuandoExistenDatos()
        {
            // Arrange
            var campo = new Campo
            {
                Id = 1,
                Nombre = "Campo Test",
                IdLicencia = 1,
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
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote 1",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                SuperficieHectareas = 50,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(campo);
            await AddTestDataAsync(campania);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(lote);

            var ciclo = new CicloCultivo
            {
                Id = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdCampania = 1,
                IdLicencia = 1,
                Epoca = EpocaSiembra.primera,
                FechaInicio = TimeHelper.GetArgentinaTime().AddDays(-60),
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(ciclo);

            var siembra = new Siembra
            {
                Id = 1,
                IdCicloCultivo = 1,
                IdCultivo = 1,
                IdLote = 1,
                IdLicencia = 1,
                IdCampania = 1,
                IdMoneda = 1,
                Fecha = TimeHelper.GetArgentinaTime().AddDays(-60),
                SuperficieHa = 50,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(siembra);

            // Act
            var result = await _reportService.GetComparativaCamposAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.Contains(result.Data, r => r.IdLote == 1);
        }

        [Fact]
        public async Task GetComparativaCamposAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _reportService.GetComparativaCamposAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetComparativaCamposAsync_DebeFiltrarPorCampania_CuandoSeEspecifica()
        {
            // Arrange
            var campo = new Campo
            {
                Id = 1,
                Nombre = "Campo Test",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania1 = new Campania
            {
                Id = 1,
                Nombre = "Campaña 2024/25",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campania2 = new Campania
            {
                Id = 2,
                Nombre = "Campaña 2023/24",
                IdLicencia = 1,
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
            var lote1 = new Lote
            {
                Id = 1,
                Nombre = "Lote 1",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                SuperficieHectareas = 50,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote2 = new Lote
            {
                Id = 2,
                Nombre = "Lote 2",
                IdLicencia = 1,
                IdCampania = 2,
                IdCampo = 1,
                SuperficieHectareas = 30,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(campo);
            await AddTestDataAsync(campania1);
            await AddTestDataAsync(campania2);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);

            // Act
            var result = await _reportService.GetComparativaCamposAsync(idCampania: 1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].IdLote);
        }

        [Fact]
        public async Task GetReporteCampoIntegralAsync_DebeRetornarReporte_CuandoCampoExiste()
        {
            // Arrange
            var campo = new Campo
            {
                Id = 1,
                Nombre = "Campo Test",
                IdLicencia = 1,
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
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote = new Lote
            {
                Id = 1,
                Nombre = "Lote 1",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                SuperficieHectareas = 50,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(campo);
            await AddTestDataAsync(campania);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(lote);

            // Act
            var result = await _reportService.GetReporteCampoIntegralAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.ResumenEjecutivo);
            Assert.NotNull(result.Data.Timeline);
            Assert.NotNull(result.Data.EvolucionCultivo);
            Assert.NotNull(result.Data.AnalisisClimatico);
        }

        [Fact]
        public async Task GetReporteCampoIntegralAsync_DebeRetornarNotFound_CuandoCampoNoExiste()
        {
            // Act
            var result = await _reportService.GetReporteCampoIntegralAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetComparativaCamposIntegralAsync_DebeRetornarComparativa_CuandoCamposExisten()
        {
            // Arrange
            var campo1 = new Campo
            {
                Id = 1,
                Nombre = "Campo Principal",
                IdLicencia = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var campo2 = new Campo
            {
                Id = 2,
                Nombre = "Campo Secundario",
                IdLicencia = 1,
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
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Maíz",
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote1 = new Lote
            {
                Id = 1,
                Nombre = "Lote 1",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 1,
                SuperficieHectareas = 50,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var lote2 = new Lote
            {
                Id = 2,
                Nombre = "Lote 2",
                IdLicencia = 1,
                IdCampania = 1,
                IdCampo = 2,
                SuperficieHectareas = 30,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(campo1);
            await AddTestDataAsync(campo2);
            await AddTestDataAsync(campania);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);

            // Act
            var result = await _reportService.GetComparativaCamposIntegralAsync(1, 2);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.CampoPrincipal);
            Assert.NotNull(result.Data.CampoSecundario);
            Assert.Equal("Campo Principal", result.Data.CampoPrincipal.Nombre);
            Assert.Equal("Campo Secundario", result.Data.CampoSecundario!.Nombre);
        }

        [Fact]
        public async Task GetComparativaCamposIntegralAsync_DebeRetornarNotFound_CuandoCampoPrincipalNoExiste()
        {
            // Act
            var result = await _reportService.GetComparativaCamposIntegralAsync(999, null);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }
    }
}
