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
            var result = await _actividadService.GetLaboresByAsync();

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
                SuperficieHa = 100,
                DensidadSemillaKgHa = 80,
                Cultivo = cultivo,
                Campania = campania,
                Lote = lote,
                TipoActividad = tipoActividad,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembra);

            // Act
            var result = await _actividadService.GetLaboresByAsync();

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
                SuperficieHa = 100,
                DensidadSemillaKgHa = 80,
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
            Assert.Equal(100, siembrasGuardadas.First().SuperficieHa);
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
                SuperficieHa = 100,
                Cultivo = cultivo,
                Campania = campania,
                RegistrationUser = TestUserAuth.UserName,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            await AddTestDataAsync(siembra);

            // Verificar que el servicio tenga el UserAuth configurado
            var httpContextAccessor = HttpContextAccessorMock.Object;
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine("Usuario autenticado correctamente en el mock");
            }
            else
            {
                Console.WriteLine("ERROR: Usuario no autenticado en el mock");
            }

            // Verificar que los datos se guardaron correctamente
            var contextFactory = GetService<Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext>>();
            await using var context = await contextFactory.CreateDbContextAsync();
            var siembrasEnDb = await context.Set<Siembra>()
                .Include(s => s.Cultivo)
                .Include(s => s.Campania)
                .ToListAsync();
            
            // Mostrar información de depuración
            Console.WriteLine($"Siembras en DB: {siembrasEnDb.Count}");
            if (siembrasEnDb.Any())
            {
                var s = siembrasEnDb.First();
                Console.WriteLine($"Primera siembra - IdLicencia: {s.IdLicencia}, IdCampania: {s.IdCampania}, IdCultivo: {s.IdCultivo}");
            }
            
            // Act
            var result = await _actividadService.GetSiembrasAsync();

            // Depuración
            if (!result.Success)
            {
                Console.WriteLine($"Error en GetSiembrasAsync: {result.ErrorMessage}");
            }
            else
            {
                Console.WriteLine($"Cantidad de siembras retornadas: {result.Data?.Count ?? 0}");
                if (result.Data != null && result.Data.Any())
                {
                    Console.WriteLine($"Primera siembra - IdLicencia: {result.Data.First().IdLicencia}, IdCampania: {result.Data.First().IdCampania}");
                }
            }

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
                SuperficieHa = 100,
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
                SuperficieHa = 200,
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
            var result = await _actividadService.GetLaboresByAsync(idCampania: 1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Campaña 2024", result.Data.First().Campania);
        }
    }
}
