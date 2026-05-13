using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public class ActividadServiceTests : ServiceTestBase
    {
        private ActividadService _actividadService;

        public ActividadServiceTests()
        {
            _actividadService = (ActividadService)GetService<IActividadService>();
        }

        [Fact]
        public async Task GetLaboresByAsync_DebeRetornarListaVacia_CuandoNoHayActividades()
        {
            // Act
            var result = await _actividadService.GetLaboresByAsyncLegacy();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetLaboresByAsync_DebeRetornarSiembras_CuandoExistenSiembras()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembra = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Densidad = 80,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembra);

            // Act
            var result = await _actividadService.GetLaboresByAsyncLegacy();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            
            var labor = result.Data.First();
            Assert.Equal("Siembra", labor.TipoActividad);
            Assert.Contains("Soja", labor.Detalle);
            Assert.Equal("Lote Test", labor.Lote);
            Assert.Equal("Campo Test", labor.Campo);
        }

        [Fact]
        public async Task SaveActividadAsync_DebeGuardarSiembra_CuandoDatosSonValidos()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembra = new Siembra
            {
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Densidad = 80,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            var actividades = new List<ILabor> { siembra };

            // Act
            var result = await _actividadService.SaveActividadAsync(actividades);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);

            // Verificar que se guardó en la base de datos
            var siembrasGuardadas = await DbContext.Set<Siembra>().ToListAsync();
            Assert.Single(siembrasGuardadas);
            Assert.Equal(100, siembrasGuardadas.First().Superficie);
        }

        [Fact]
        public async Task GetSiembrasAsync_DebeRetornarListaSiembras_CuandoExistenSiembras()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(cultivo);

            var siembra = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                IdLicencia = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Cultivo = cultivo,
                Campania = campania,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembra);

            // Act
            var result = await _actividadService.GetSiembrasAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Soja", result.Data.First().Cultivo.Nombre);
        }

        [Fact]
        public async Task GetLaboresByAsync_DebeFiltrarPorCampania_CuandoSeEspecificaIdCampania()
        {
            // Arrange
            var campania1 = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campania2 = new Campania { Id = 2, Nombre = "Campaña 2025", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania1);
            await AddTestDataAsync(campania2);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembraCampania1 = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Cultivo = cultivo,
                Campania = campania1,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            var siembraCampania2 = new Siembra
            {
                Id = 2,
                IdCampania = 2,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 200,
                Cultivo = cultivo,
                Campania = campania2,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembraCampania1);
            await AddTestDataAsync(siembraCampania2);

            // Act
            var result = await _actividadService.GetLaboresByAsyncLegacy(idCampania: 1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Campaña 2024", result.Data.First().Campania);
        }

        [Fact]
        public async Task GetLaboresByAsync_DebeFiltrarPorLote_CuandoSeEspecificaIdLote()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote1 = new Lote { Id = 1, Nombre = "Lote 1", IdCampo = 1, Campo = campo };
            var lote2 = new Lote { Id = 2, Nombre = "Lote 2", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembraLote1 = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote1,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            var siembraLote2 = new Siembra
            {
                Id = 2,
                IdCampania = 1,
                IdLote = 2,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 200,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote2,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembraLote1);
            await AddTestDataAsync(siembraLote2);

            // Act
            var result = await _actividadService.GetLaboresByAsyncLegacy(idLote: 1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Lote 1", result.Data.First().Lote);
        }

        [Fact]
        public async Task GetLaboresByAsync_DebeFiltrarPorMultiplesLotes_CuandoSeEspecificanIdsLotes()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote1 = new Lote { Id = 1, Nombre = "Lote 1", IdCampo = 1, Campo = campo };
            var lote2 = new Lote { Id = 2, Nombre = "Lote 2", IdCampo = 1, Campo = campo };
            var lote3 = new Lote { Id = 3, Nombre = "Lote 3", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote1);
            await AddTestDataAsync(lote2);
            await AddTestDataAsync(lote3);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembra1 = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote1,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            var siembra2 = new Siembra
            {
                Id = 2,
                IdCampania = 1,
                IdLote = 2,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 200,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote2,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            var siembra3 = new Siembra
            {
                Id = 3,
                IdCampania = 1,
                IdLote = 3,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 300,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote3,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembra1);
            await AddTestDataAsync(siembra2);
            await AddTestDataAsync(siembra3);

            // Act
            var result = await _actividadService.GetLaboresByAsyncLegacy(idsLotes: new List<int> { 1, 3 });

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, l => l.Lote == "Lote 1");
            Assert.Contains(result.Data, l => l.Lote == "Lote 3");
        }

        [Fact]
        public async Task UpdateActividadAsync_DebeActualizarActividad_CuandoExiste()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembra = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Densidad = 80,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };
            await AddTestDataAsync(siembra);

            var siembraActualizada = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 150, // Valor actualizado
                Densidad = 90, // Valor actualizado
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            // Act
            var result = await _actividadService.UpdateActividadAsync(siembraActualizada);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var siembraResult = result.Data as Siembra;
            Assert.NotNull(siembraResult);
            Assert.Equal(150, siembraResult.Superficie);
            Assert.Equal(90, siembraResult.Densidad);
        }

        [Fact]
        public async Task DeteleActividadAsync_DebeEliminarActividad_CuandoExiste()
        {
            // Arrange
            var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
            var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
            var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };
            var cultivo = new Cultivo { Id = 1, Nombre = "Soja" };
            var tipoActividad = new TipoActividad { Id = 1, Nombre = "Siembra", Icono = "seeding", ColorIcono = "green" };

            await AddTestDataAsync(campania);
            await AddTestDataAsync(campo);
            await AddTestDataAsync(lote);
            await AddTestDataAsync(cultivo);
            await AddTestDataAsync(tipoActividad);

            var siembra = new Siembra
            {
                Id = 1,
                IdCampania = 1,
                IdLote = 1,
                IdCultivo = 1,
                IdTipoActividad = 1,
                Fecha = DateTime.Now,
                Superficie = 100,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };
            await AddTestDataAsync(siembra);

            // Act
            await _actividadService.DeteleActividadAsync(1, TipoActividadEnum.Siembra);

            // Assert
            var siembrasRestantes = await DbContext.Set<Siembra>().ToListAsync();
            Assert.Empty(siembrasRestantes);
        }
    }
}
