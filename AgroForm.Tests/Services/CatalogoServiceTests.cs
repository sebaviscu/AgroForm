using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model.Actividades;
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
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public class CatalogoServiceTests : ServiceTestBase
    {
        private ICatalogoService _catalogoService;

        public CatalogoServiceTests()
        {
            _catalogoService = GetService<ICatalogoService>();
        }

        #region GetAllAsync - Multitenant OR filter

        [Fact]
        public async Task GetAllAsync_DebeRetornarGlobalesYPropios()
        {
            // Arrange: Catalogo is IOptionalLicenciaEntity → OR filter: NULL OR IdLicencia=1
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
                // IdLicencia = NULL → global
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Propia",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1, // Owned by current license
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 3,
                Nombre = "Enfermedad Otra",
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = true,
                IdLicencia = 2, // Another license → should be excluded
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllAsync();

            // Assert: Returns globals (NULL) + own (IdLicencia=1), excludes other licenses
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
            var result = await _catalogoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetAllWithDetailsAsync

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarGlobalesYPropios()
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Propia",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 3,
                Nombre = "Enfermedad Otra",
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = true,
                IdLicencia = 2
            };

            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Global + own
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_DebeRetornarGlobal_CuandoIdValido()
        {
            // Arrange: global records are accessible to all licenses
            var catalogo = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Plaga 1", result.Data.Nombre);
            Assert.Equal(TipoCatalogoEnum.Plaga, result.Data.Tipo);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarPropio_CuandoIdValido()
        {
            // Arrange: own records are accessible
            var catalogo = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _catalogoService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoRegistroDeOtraLicencia()
        {
            // Arrange
            var catalogo = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Otra",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByIdAsync(1);

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
            var catalogo = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_DebeAsignarDatosPorDefecto()
        {
            // Arrange
            var nuevoCatalogo = new Catalogo
            {
                Nombre = "Nuevo Catalogo",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // No IdLicencia → ServiceBase will assign it (user is not SuperAdmin)
            };

            // Act
            var result = await _catalogoService.CreateAsync(nuevoCatalogo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0);
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
            // IdLicencia should be assigned (IOptionalLicenciaEntity, not SuperAdmin)
            Assert.Equal(1, result.Data.IdLicencia);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_DebePreservarDatosOriginales()
        {
            // Arrange
            var catalogoOriginal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Original",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                Descripcion = "Descripción original",
                IdLicencia = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(catalogoOriginal);

            var catalogoActualizado = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Actualizado",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = false,
                Descripcion = "Descripción actualizada"
                // No incluir RegistrationDate, RegistrationUser, IdLicencia
            };

            // Act
            var result = await _catalogoService.UpdateAsync(catalogoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Plaga Actualizado", result.Data.Nombre);
            Assert.Equal(TipoCatalogoEnum.Maleza, result.Data.Tipo);
            Assert.False(result.Data.Activo);
            Assert.Equal("Descripción actualizada", result.Data.Descripcion);
            Assert.Equal(1, result.Data.Id); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
            Assert.Equal(catalogoOriginal.RegistrationDate, result.Data.RegistrationDate); // Preservado
            Assert.NotNull(result.Data.ModificationDate);
            Assert.Equal(TestUserAuth.UserName, result.Data.ModificationUser);
            // IdLicencia should be preserved (IOptionalLicenciaEntity → preserve original)
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task UpdateAsync_DebePreservarIdLicenciaGlobal()
        {
            // Arrange: update of a global record should preserve NULL IdLicencia
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                Descripcion = "Original",
                IdLicencia = null, // Global
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "admin"
            };
            await AddTestDataAsync(catalogoGlobal);

            var catalogoActualizado = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global Actualizado",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                Descripcion = "Actualizado"
            };

            // Act
            var result = await _catalogoService.UpdateAsync(catalogoActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Plaga Global Actualizado", result.Data.Nombre);
            Assert.Null(result.Data.IdLicencia); // Preserved as global
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var catalogoNoExistente = new Catalogo
            {
                Id = 999,
                Nombre = "Inexistente",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };

            // Act
            var result = await _catalogoService.UpdateAsync(catalogoNoExistente);

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
            var catalogoPropio = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1 // Owned by current license → can delete
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza 1",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 2 // Another license → should survive
            };
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);

            // Verify only the owned record was deleted
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var catalogosRestantes = await context.Set<Catalogo>().ToListAsync();
            Assert.Single(catalogosRestantes);
            Assert.Equal(2, catalogosRestantes[0].Id); // Only the other license's record remains
        }

        [Fact]
        public async Task DeleteAsync_DebeRechazarGlobal()
        {
            // Arrange: globals can't be deleted by regular users
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            await AddTestDataAsync(catalogoGlobal);

            // Act: null != 1 → rejected
            var result = await _catalogoService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRechazarDeOtraLicencia()
        {
            // Arrange
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Otra",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _catalogoService.DeleteAsync(999);

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
            var catalogo = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogo);

            // Act
            var exists = await _catalogoService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _catalogoService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_DebeRetornarExitoso_CuandoEntidadValida()
        {
            // Arrange
            var catalogo = new Catalogo
            {
                Nombre = "Plaga 1",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                Descripcion = "Descripción de prueba"
            };

            // Act
            var result = await _catalogoService.ValidateAsync(catalogo);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region GetQuery - Multitenant filter

        [Fact]
        public async Task GetQuery_DebeRetornarGlobalesYPropios()
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Propia",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 3,
                Nombre = "Enfermedad Otra",
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = true,
                IdLicencia = 2
            };
            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var query = _catalogoService.GetQuery();

            // Assert
            Assert.NotNull(query);

            // Verify OR filter: globals (NULL) + own (IdLicencia=1)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count);
            Assert.Contains(resultados, c => c.Id == 1);
            Assert.Contains(resultados, c => c.Id == 2);
            Assert.DoesNotContain(resultados, c => c.Id == 3);
        }

        #endregion

        #region GetByType - Multitenant OR filter

        [Fact]
        public async Task GetByType_DebeRetornarGlobalesYPropios()
        {
            // Arrange: GetByType has explicit OR filter: IdLicencia == NULL OR IdLicencia == 1
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Plaga Propia",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 3,
                Nombre = "Plaga Otra",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 2 // Another license → excluded
            };
            var catalogoInactivo = new Catalogo
            {
                Id = 4,
                Nombre = "Plaga Inactiva",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = false // Inactive → excluded
            };

            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoOtraLicencia);
            await AddTestDataAsync(catalogoInactivo);

            // Act
            var result = await _catalogoService.GetByType(TipoCatalogoEnum.Plaga);

            // Assert: only active globals + own records of Plaga type
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1); // Global
            Assert.Contains(result.Data, c => c.Id == 2); // Own
            Assert.DoesNotContain(result.Data, c => c.Id == 3); // Other license excluded
            Assert.DoesNotContain(result.Data, c => c.Id == 4); // Inactive excluded
        }

        [Fact]
        public async Task GetByType_DebeRetornarOrdenadosPorNombre()
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Z Plaga",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogoPropioA = new Catalogo
            {
                Id = 2,
                Nombre = "A Plaga",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };
            var catalogoPropioM = new Catalogo
            {
                Id = 3,
                Nombre = "M Plaga",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1
            };

            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropioA);
            await AddTestDataAsync(catalogoPropioM);

            // Act
            var result = await _catalogoService.GetByType(TipoCatalogoEnum.Plaga);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("A Plaga", result.Data[0].Nombre);
            Assert.Equal("M Plaga", result.Data[1].Nombre);
            Assert.Equal("Z Plaga", result.Data[2].Nombre);
        }

        [Fact]
        public async Task GetByType_DebeRetornarError_CuandoNoHayCatalogosDelTipo()
        {
            // Arrange
            var catalogoOtroTipo = new Catalogo
            {
                Id = 1,
                Nombre = "Maleza 1",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogoOtroTipo);

            // Act
            var result = await _catalogoService.GetByType(TipoCatalogoEnum.Plaga);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Catálogo por tipo no encontrado", result.ErrorMessage);
        }

        [Theory]
        [InlineData(TipoCatalogoEnum.Plaga)]
        [InlineData(TipoCatalogoEnum.Maleza)]
        [InlineData(TipoCatalogoEnum.Enfermedad)]
        [InlineData(TipoCatalogoEnum.TipoFertilizante)]
        [InlineData(TipoCatalogoEnum.Nutriente)]
        [InlineData(TipoCatalogoEnum.ProductoAgroquimico)]
        [InlineData(TipoCatalogoEnum.MetodoSiembra)]
        [InlineData(TipoCatalogoEnum.MetodoRiego)]
        [InlineData(TipoCatalogoEnum.MetodoAplicacion)]
        [InlineData(TipoCatalogoEnum.Maquinaria)]
        [InlineData(TipoCatalogoEnum.FuenteAgua)]
        [InlineData(TipoCatalogoEnum.Laboratorio)]
        [InlineData(TipoCatalogoEnum.Otro)]
        public async Task GetByType_DebeFuncionarConTodosLosTipos(TipoCatalogoEnum tipo)
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = $"Test {tipo}",
                Tipo = tipo,
                Activo = true
                // Global → always visible
            };

            await AddTestDataAsync(catalogoGlobal);

            // Act
            var result = await _catalogoService.GetByType(tipo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(tipo, result.Data[0].Tipo);
        }

        #endregion

        #region GetAllActive - Multitenant OR filter

        [Fact]
        public async Task GetAllActive_DebeRetornarGlobalesYPropios()
        {
            // Arrange: GetAllActive has explicit OR filter: IdLicencia == NULL OR IdLicencia == 1
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Propia",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1
            };
            var catalogoInactivo = new Catalogo
            {
                Id = 3,
                Nombre = "Inactivo",
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = false
            };
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 4,
                Nombre = "Otra Licencia",
                Tipo = TipoCatalogoEnum.TipoFertilizante,
                Activo = true,
                IdLicencia = 2
            };

            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);
            await AddTestDataAsync(catalogoInactivo);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllActive();

            // Assert: active globals + own, excluding inactive and other licenses
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Id == 1);
            Assert.Contains(result.Data, c => c.Id == 2);
            Assert.All(result.Data, c => Assert.True(c.Activo));
        }

        [Fact]
        public async Task GetAllActive_DebeRetornarError_CuandoNoHayCatalogosActivos()
        {
            // Arrange
            var catalogoInactivo = new Catalogo
            {
                Id = 1,
                Nombre = "Inactivo",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = false,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogoInactivo);

            // Act
            var result = await _catalogoService.GetAllActive();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Catálogos activos no encontrados", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllActive_DebeRetornarError_CuandoNoHayDatos()
        {
            // Act
            var result = await _catalogoService.GetAllActive();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Catálogos activos no encontrados", result.ErrorMessage);
        }

        #endregion

        #region GetVisibleByLicenseAsync

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeRetornarGlobalesVisiblesYPropios()
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // IdLicencia = NULL → global, no LicenciasCatalogos entry → visible
            };
            var catalogoPropio = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Propia",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                IdLicencia = 1
            };
            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(catalogoPropio);

            // Act
            var result = await _catalogoService.GetVisibleByLicenseAsync();

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
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var otroGlobal = new Catalogo
            {
                Id = 2,
                Nombre = "Maleza Global",
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };
            await AddTestDataAsync(catalogoGlobal);
            await AddTestDataAsync(otroGlobal);

            // Hide catalogoGlobal via LicenciasCatalogos side table
            var licenciaCatalogo = new LicenciasCatalogos
            {
                Id = 1,
                IdLicencia = 1,
                IdCatalogo = 1,
                Activo = false, // Hidden
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(licenciaCatalogo);

            // Act
            var result = await _catalogoService.GetVisibleByLicenseAsync();

            // Assert: hidden global excluded, non-hidden global remains
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(2, result.Data[0].Id); // Only the non-hidden global
        }

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeIncluirGlobalConRegistroActivo()
        {
            // Arrange: explicit Activo=true should still show the global
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            await AddTestDataAsync(catalogoGlobal);

            var licenciaCatalogo = new LicenciasCatalogos
            {
                Id = 1,
                IdLicencia = 1,
                IdCatalogo = 1,
                Activo = true, // Explicitly visible
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(licenciaCatalogo);

            // Act
            var result = await _catalogoService.GetVisibleByLicenseAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(1, result.Data[0].Id);
        }

        [Fact]
        public async Task GetVisibleByLicenseAsync_DebeExcluirPropioDeOtraLicencia()
        {
            // Arrange
            var catalogoOtraLicencia = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Otra",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 2 // Another license
            };
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetVisibleByLicenseAsync();

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
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            await AddTestDataAsync(catalogoGlobal);

            // Act: hide the global catalog item
            var result = await _catalogoService.SetVisibilityAsync(1, false);

            // Assert
            Assert.True(result.Success);

            // Verify LicenciasCatalogos entry was created with Activo=false
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCatalogos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCatalogo == 1);
            Assert.NotNull(entry);
            Assert.False(entry.Activo);
            Assert.Equal(TestUserAuth.UserName, entry.RegistrationUser);
            Assert.NotNull(entry.RegistrationDate);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeMostrarGlobal()
        {
            // Arrange
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            await AddTestDataAsync(catalogoGlobal);

            // Act: explicitly show the global catalog item
            var result = await _catalogoService.SetVisibilityAsync(1, true);

            // Assert
            Assert.True(result.Success);

            // Verify LicenciasCatalogos entry was created with Activo=true
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCatalogos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCatalogo == 1);
            Assert.NotNull(entry);
            Assert.True(entry.Activo);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeActualizarRegistroExistente()
        {
            // Arrange: create an existing hidden entry
            var catalogoGlobal = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Global",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            await AddTestDataAsync(catalogoGlobal);

            var existingEntry = new LicenciasCatalogos
            {
                Id = 1,
                IdLicencia = 1,
                IdCatalogo = 1,
                Activo = false, // Currently hidden
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            await AddTestDataAsync(existingEntry);

            // Act: show it again
            var result = await _catalogoService.SetVisibilityAsync(1, true);

            // Assert
            Assert.True(result.Success);

            // Verify the existing entry was updated to Activo=true
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var entry = await context.Set<LicenciasCatalogos>()
                .FirstOrDefaultAsync(lc => lc.IdLicencia == 1 && lc.IdCatalogo == 1);
            Assert.NotNull(entry);
            Assert.True(entry.Activo);
            Assert.NotNull(entry.ModificationDate);
            Assert.Equal(TestUserAuth.UserName, entry.ModificationUser);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeRetornarError_SiNoEsGlobal()
        {
            // Arrange: try to set visibility on an owned catalog (not global)
            var catalogoPropio = new Catalogo
            {
                Id = 1,
                Nombre = "Plaga Propia",
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                IdLicencia = 1 // Owned, not global
            };
            await AddTestDataAsync(catalogoPropio);

            // Act: SetVisibilityAsync seeks IdLicencia=null, this has IdLicencia=1
            var result = await _catalogoService.SetVisibilityAsync(1, false);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("El catálogo global no existe.", result.ErrorMessage);
        }

        [Fact]
        public async Task SetVisibilityAsync_DebeRetornarError_SiNoExiste()
        {
            // Act: non-existent Id
            var result = await _catalogoService.SetVisibilityAsync(999, false);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        #endregion
    }
}
