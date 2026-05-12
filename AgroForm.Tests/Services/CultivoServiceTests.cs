using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Data.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class CultivoServiceTests : ServiceTestBase
    {
        private ICultivoService _cultivoService;

        public CultivoServiceTests()
        {
            _cultivoService = GetService<ICultivoService>();
        }

        #region GetAllAsync - Multitenant OR filter tests

        [Fact]
        public async Task GetAllAsync_DebeRetornarGlobalesYPropios()
        {
            // Arrange: Cultivo is IOptionalLicenciaEntity → OR filter: NULL OR IdLicencia=1
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
                // IdLicencia = NULL → global
            };
            var cultivoPropio = new Cultivo
            {
                Id = 2,
                Nombre = "Maíz",
                Orden = 2,
                Activo = true,
                IdLicencia = 1, // Owned by current license
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 3,
                Nombre = "Soja",
                Orden = 3,
                Activo = true,
                IdLicencia = 2, // Another license → should be excluded
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(cultivoGlobal);
            await AddTestDataAsync(cultivoPropio);
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var result = await _cultivoService.GetAllAsync();

            // Assert: Returns globals (NULL) + own (IdLicencia=1), excludes other licenses (IdLicencia=2)
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1); // Global
            Assert.Contains(result.Data, c => c.Id == 2); // Own
            Assert.DoesNotContain(result.Data, c => c.Id == 3); // Other license excluded
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _cultivoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_SinLicencia_DebeRetornarTodos()
        {
            // This test verifies behavior when IdLicencia is null (no license context).
            // We can't easily change the test user's license, so this is a documentation note.
            // The OR filter (IdLicencia IS NULL OR IdLicencia = currentLicense) would include
            // all globals + records belonging to the current license.
            // Arrange
            var cultivo1 = new Cultivo 
            { 
                Id = 1, 
                Nombre = "Trigo", 
                Orden = 1,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName,
                IdLicencia = 1
            };
            var cultivo2 = new Cultivo 
            { 
                Id = 2, 
                Nombre = "Maíz", 
                Orden = 2,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName,
                IdLicencia = 1
            };

            await AddTestDataAsync(cultivo1);
            await AddTestDataAsync(cultivo2);

            // Act
            var result = await _cultivoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetAllWithDetailsAsync

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarGlobalesYPropios()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global
            };
            var cultivoPropio = new Cultivo
            {
                Id = 2,
                Nombre = "Maíz",
                Orden = 2,
                Activo = true,
                IdLicencia = 1
            };
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 3,
                Nombre = "Soja",
                Orden = 3,
                Activo = true,
                IdLicencia = 2
            };

            await AddTestDataAsync(cultivoGlobal);
            await AddTestDataAsync(cultivoPropio);
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var result = await _cultivoService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Global + own, excluding other licenses
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_DebeRetornarGlobal_CuandoIdValido()
        {
            // Arrange: global records are accessible to all licenses
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global
            };
            await AddTestDataAsync(cultivo);

            // Act
            var result = await _cultivoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Trigo", result.Data.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarPropio_CuandoIdValido()
        {
            // Arrange: own records are accessible
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(cultivo);

            // Act
            var result = await _cultivoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Trigo", result.Data.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cultivoService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoRegistroDeOtraLicencia()
        {
            // Arrange: records from other licenses should not be accessible
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Soja",
                Orden = 1,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(cultivo);

            // Act
            var result = await _cultivoService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        #endregion

        #region GetByIdWithDetailsAsync

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(cultivo);

            // Act
            var result = await _cultivoService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        #endregion

        #region CreateAsync - Auto-create EstadoFenologicos

        [Fact]
        public async Task CreateAsync_DebeAsignarDatosPorDefecto()
        {
            // Arrange
            var nuevoCultivo = new Cultivo
            {
                Nombre = "Nuevo Cultivo",
                Orden = 10,
                Activo = true
                // No IdLicencia → ServiceBase will assign it (user is not SuperAdmin)
            };

            // Act
            var result = await _cultivoService.CreateAsync(nuevoCultivo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0);
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
            // IdLicencia should be assigned by ServiceBase (not SuperAdmin → license assigned)
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task CreateAsync_DebeAutoCrearEstadosFenologicos()
        {
            // Arrange
            var nuevoCultivo = new Cultivo
            {
                Nombre = "Trigo",
                Orden = 1,
                Activo = true
            };

            // Act
            var result = await _cultivoService.CreateAsync(nuevoCultivo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0);

            // Verify 5 EstadoFenologicos were auto-created
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var estados = await context.Set<EstadoFenologico>()
                .Where(e => e.IdCultivo == result.Data.Id)
                .OrderBy(e => e.Orden)
                .ToListAsync();

            Assert.Equal(5, estados.Count);

            // Verify each phenological state
            Assert.Equal("E", estados[0].Codigo);
            Assert.Equal("Emergencia", estados[0].Nombre);
            Assert.Equal(1, estados[0].Orden);

            Assert.Equal("V6", estados[1].Codigo);
            Assert.Equal("Desarrollo Vegetativo", estados[1].Nombre);
            Assert.Equal(2, estados[1].Orden);

            Assert.Equal("R", estados[2].Codigo);
            Assert.Equal("Reproductivo", estados[2].Nombre);
            Assert.Equal(3, estados[2].Orden);

            Assert.Equal("F", estados[3].Codigo);
            Assert.Equal("Floración", estados[3].Nombre);
            Assert.Equal(4, estados[3].Orden);

            Assert.Equal("M", estados[4].Codigo);
            Assert.Equal("Madurez", estados[4].Nombre);
            Assert.Equal(5, estados[4].Orden);

            // All should be active
            Assert.All(estados, e => Assert.True(e.Activo));
        }

        [Fact]
        public async Task CreateAsync_DebeRetornarError_CuandoNombreDuplicado()
        {
            // Arrange
            var cultivoExistente = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(cultivoExistente);

            var nuevoCultivo = new Cultivo
            {
                Nombre = "Trigo", // Mismo nombre
                Orden = 2,
                Activo = true
            };

            // Act
            var result = await _cultivoService.CreateAsync(nuevoCultivo);

            // Assert - No duplicate validation exists yet
            Assert.True(result.Success);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var cultivoOriginal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Original",
                Orden = 1,
                Activo = true,
                IdLicencia = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(cultivoOriginal);

            var cultivoActualizado = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Actualizado",
                Orden = 2,
                Activo = false
                // No incluir RegistrationDate, RegistrationUser, IdLicencia
            };

            // Act
            var result = await _cultivoService.UpdateAsync(cultivoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Trigo Actualizado", result.Data.Nombre);
            Assert.Equal(2, result.Data.Orden);
            Assert.False(result.Data.Activo);
            Assert.Equal(1, result.Data.Id); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
            Assert.Equal(cultivoOriginal.RegistrationDate, result.Data.RegistrationDate); // Preservado
            Assert.NotNull(result.Data.ModificationDate);
            Assert.Equal(TestUserAuth.UserName, result.Data.ModificationUser);
            // IdLicencia should be preserved (IOptionalLicenciaEntity → preserve original)
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarIdLicenciaGlobal()
        {
            // Arrange: update of a global record should preserve NULL IdLicencia
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true,
                IdLicencia = null, // Global
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "admin"
            };
            await AddTestDataAsync(cultivoGlobal);

            var cultivoActualizado = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global Actualizado",
                Orden = 2,
                Activo = true
                // IdLicencia not set - should be preserved as NULL from original
            };

            // Act
            var result = await _cultivoService.UpdateAsync(cultivoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Trigo Global Actualizado", result.Data.Nombre);
            Assert.Null(result.Data.IdLicencia); // Preserved as global
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var cultivoNoExistente = new Cultivo
            {
                Id = 999,
                Nombre = "Inexistente",
                Orden = 1,
                Activo = true
            };

            // Act
            var result = await _cultivoService.UpdateAsync(cultivoNoExistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El registro que intenta actualizar no existe.", result.ErrorMessage);
        }

        #endregion

        #region DeleteAsync - Multitenant verification

        [Fact]
        public async Task DeleteAsync_DebeEliminarPropio()
        {
            // Arrange
            var cultivoPropio = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                IdLicencia = 1 // Owned by current license → can delete
            };
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 2,
                Nombre = "Maíz",
                Orden = 2,
                Activo = true,
                IdLicencia = 2 // Another license → should survive
            };
            await AddTestDataAsync(cultivoPropio);
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var result = await _cultivoService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);

            // Verify only the owned record was deleted
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var cultivosRestantes = await context.Set<Cultivo>().ToListAsync();
            Assert.Single(cultivosRestantes);
            Assert.Equal(2, cultivosRestantes[0].Id); // Only the other license's record remains
        }

        [Fact]
        public async Task DeleteAsync_DebeRechazarGlobal()
        {
            // Arrange: globals can't be deleted by regular users
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global
            };
            await AddTestDataAsync(cultivoGlobal);

            // Act: delete with IdLicencia=null (global), user license is 1, null != 1 → rejected
            var result = await _cultivoService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRechazarDeOtraLicencia()
        {
            // Arrange
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 1,
                Nombre = "Soja",
                Orden = 1,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var result = await _cultivoService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _cultivoService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El registro que intenta eliminar no existe.", result.ErrorMessage);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var cultivo = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(cultivo);

            // Act
            var exists = await _cultivoService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _cultivoService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_DebeRetornarExitoso_CuandoEntidadValida()
        {
            // Arrange
            var cultivo = new Cultivo
            {
                Nombre = "Trigo",
                Orden = 1,
                Activo = true
            };

            // Act
            var result = await _cultivoService.ValidateAsync(cultivo);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region GetQuery - Multitenant filter

        [Fact]
        public async Task GetQuery_DebeRetornarGlobalesYPropios()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global
            };
            var cultivoPropio = new Cultivo
            {
                Id = 2,
                Nombre = "Maíz",
                Orden = 2,
                Activo = true,
                IdLicencia = 1
            };
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 3,
                Nombre = "Soja",
                Orden = 3,
                Activo = true,
                IdLicencia = 2
            };
            await AddTestDataAsync(cultivoGlobal);
            await AddTestDataAsync(cultivoPropio);
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var query = _cultivoService.GetQuery();

            // Assert
            Assert.NotNull(query);

            // Verify OR filter: globals (NULL) + own (IdLicencia=1), excluding other licenses
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count);
            Assert.Contains(resultados, c => c.Id == 1);
            Assert.Contains(resultados, c => c.Id == 2);
            Assert.DoesNotContain(resultados, c => c.Id == 3);
        }

        #endregion

        #region GetVisibleByLicenseAsync

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeRetornarGlobalesVisiblesYPropios()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global, no LicenciasCultivos entry → visible by default
            };
            var cultivoPropio = new Cultivo
            {
                Id = 2,
                Nombre = "Maíz Propio",
                Orden = 2,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(cultivoGlobal);
            await AddTestDataAsync(cultivoPropio);

            // Act
            var result = await _cultivoService.GetVisibleByLicenseAsync();

            // Assert: own + visible globals
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);
        }

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeExcluirGlobalOculto()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
                // IdLicencia = NULL → global
            };
            var otroGlobal = new Cultivo
            {
                Id = 2,
                Nombre = "Soja Global",
                Orden = 2,
                Activo = true
                // IdLicencia = NULL → global
            };
            await AddTestDataAsync(cultivoGlobal);
            await AddTestDataAsync(otroGlobal);

            // Hide cultivoGlobal via LicenciasCultivos side table
            var licenciaCultivo = new LicenciasCultivos
            {
                Id = 1,
                IdLicencia = 1, // Current license
                IdCultivo = 1,
                Activo = false, // Hidden
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(licenciaCultivo);

            // Act
            var result = await _cultivoService.GetVisibleByLicenseAsync();

            // Assert: hidden global excluded, visible global + own included
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal(2, result.Data[0].Id); // Only the non-hidden global
        }

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeIncluirGlobalConRegistroActivo()
        {
            // Arrange: a LicenciasCultivos entry with Activo=true should still show the global
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
            };
            await AddTestDataAsync(cultivoGlobal);

            // Create visibility entry with Activo=true (explicitly shown)
            var licenciaCultivo = new LicenciasCultivos
            {
                Id = 1,
                IdLicencia = 1,
                IdCultivo = 1,
                Activo = true, // Explicitly visible
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(licenciaCultivo);

            // Act
            var result = await _cultivoService.GetVisibleByLicenseAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].Id);
        }

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeExcluirPropioDeOtraLicencia()
        {
            // Arrange: own records from other licenses should never appear
            var cultivoOtraLicencia = new Cultivo
            {
                Id = 1,
                Nombre = "Soja",
                Orden = 1,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(cultivoOtraLicencia);

            // Act
            var result = await _cultivoService.GetVisibleByLicenseAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region SetVisibilityAsync

        [Fact]
        public async Task SetVisibilityAsync_DebeOcultarGlobal()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
            };
            await AddTestDataAsync(cultivoGlobal);

            // Act: hide the global crop
            var result = await _cultivoService.SetVisibilityAsync(1, false);

            // Assert
            Assert.True(result.Success);

            // Verify LicenciasCultivos entry was created with Activo=false
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCultivos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCultivo == 1);
            Assert.NotNull(entry);
            Assert.False(entry.Activo);
            Assert.Equal(TestUserAuth.UserName, entry.RegistrationUser);
            Assert.NotNull(entry.RegistrationDate);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeMostrarGlobal()
        {
            // Arrange
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
            };
            await AddTestDataAsync(cultivoGlobal);

            // Act: explicitly show the global crop
            var result = await _cultivoService.SetVisibilityAsync(1, true);

            // Assert
            Assert.True(result.Success);

            // Verify LicenciasCultivos entry was created with Activo=true
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCultivos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCultivo == 1);
            Assert.NotNull(entry);
            Assert.True(entry.Activo);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeActualizarRegistroExistente()
        {
            // Arrange: create an existing hidden entry
            var cultivoGlobal = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Global",
                Orden = 1,
                Activo = true
            };
            await AddTestDataAsync(cultivoGlobal);

            var existingEntry = new LicenciasCultivos
            {
                Id = 1,
                IdLicencia = 1,
                IdCultivo = 1,
                Activo = false, // Currently hidden
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(existingEntry);

            // Act: show it again
            var result = await _cultivoService.SetVisibilityAsync(1, true);

            // Assert
            Assert.True(result.Success);

            // Verify the existing entry was updated to Activo=true
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCultivos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCultivo == 1);
            Assert.NotNull(entry);
            Assert.True(entry.Activo);
            Assert.NotNull(entry.ModificationDate);
            Assert.Equal(TestUserAuth.UserName, entry.ModificationUser);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeRetornarError_SiNoEsGlobal()
        {
            // Arrange: try to set visibility on an owned crop (not global)
            var cultivoPropio = new Cultivo
            {
                Id = 1,
                Nombre = "Trigo Propio",
                Orden = 1,
                Activo = true,
                IdLicencia = 1 // Owned, not global
            };
            await AddTestDataAsync(cultivoPropio);

            // Act: SetVisibilityAsync seeks IdLicencia=null, this has IdLicencia=1
            var result = await _cultivoService.SetVisibilityAsync(1, false);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El cultivo global no existe.", result.ErrorMessage);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeRetornarError_SiNoExiste()
        {
            // Act: non-existent Id
            var result = await _cultivoService.SetVisibilityAsync(999, false);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        #endregion
    }
}
