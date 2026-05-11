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

        [Fact]
        public async Task GetAllAsync_DebeRetornarTodosLosCatalogos()
        {
            // Arrange
            var catalogo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var catalogo2 = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Maleza 1", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var catalogoOtraLicencia = new Catalogo 
            { 
                Id = 3, 
                Nombre = "Enfermedad 1", 
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = true,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(catalogo1);
            await AddTestDataAsync(catalogo2);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todos los catálogos (sin filtro por licencia)
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

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarTodosLosCatalogos()
        {
            // Arrange
            var catalogo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogoOtraLicencia = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Maleza 1", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };

            await AddTestDataAsync(catalogo1);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllWithDetailsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Todos los catálogos (sin filtro por licencia)
            Assert.Equal(1, result.Data[0].Id);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var catalogo = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
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
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var catalogo = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_DebeAsignarDatosPorDefecto()
        {
            // Arrange
            var nuevoCatalogo = new Catalogo 
            { 
                Nombre = "Nuevo Catalogo", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
                // No asignar IdLicencia ni IdCampania (entidad sin licencia)
            };

            // Act
            var result = await _catalogoService.CreateAsync(nuevoCatalogo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Id > 0); // Asignado automáticamente
            Assert.Equal(TestUserAuth.UserName, result.Data.RegistrationUser);
            Assert.NotNull(result.Data.RegistrationDate);
        }

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
                Descripcion = "Descripción actualizada",
                RegistrationUser = "usuario_original" // Preservar el usuario original
                // No incluir RegistrationDate, IdLicencia
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

        [Fact]
        public async Task DeleteAsync_DebeEliminarCorrectamente()
        {
            // Arrange
            var catalogoMismaLicencia = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogoOtraLicencia = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Maleza 1", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };
            await AddTestDataAsync(catalogoMismaLicencia);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            await using var context = await GetService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
            var catalogosRestantes = await context.Set<Catalogo>().ToListAsync();
            Assert.Single(catalogosRestantes);
            Assert.Equal(2, catalogosRestantes[0].Id); // Solo queda el de otra licencia
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

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var catalogo = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
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

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var catalogo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogo2 = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Maleza 1", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };
            await AddTestDataAsync(catalogo1);
            await AddTestDataAsync(catalogo2);

            // Act
            var query = _catalogoService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye todos los catálogos (sin filtro por licencia)
            var resultados = await query.ToListAsync();
            Assert.Equal(2, resultados.Count); // Todos los catálogos (sin filtro por licencia)
            Assert.Equal(1, resultados[0].Id);
        }

        // Tests específicos para el método GetByType

        [Fact]
        public async Task GetByType_DebeRetornarCatalogosActivos_CuandoTipoValido()
        {
            // Arrange
            var catalogo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Plaga 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogo2 = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Plaga 2", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogoInactivo = new Catalogo 
            { 
                Id = 3, 
                Nombre = "Plaga Inactivo", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = false, // Inactivo
            };
            var catalogoOtroTipo = new Catalogo 
            { 
                Id = 4, 
                Nombre = "Maleza 1", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };

            await AddTestDataAsync(catalogo1);
            await AddTestDataAsync(catalogo2);
            await AddTestDataAsync(catalogoInactivo);
            await AddTestDataAsync(catalogoOtroTipo);

            // Act
            var result = await _catalogoService.GetByType(TipoCatalogoEnum.Plaga);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Solo los activos del tipo Plaga
            Assert.All(result.Data, c => 
            {
                Assert.Equal(TipoCatalogoEnum.Plaga, c.Tipo);
                Assert.True(c.Activo);
            });
        }

        [Fact]
        public async Task GetByType_DebeRetornarOrdenadosPorNombre()
        {
            // Arrange
            var catalogo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Z Plaga", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogo2 = new Catalogo 
            { 
                Id = 2, 
                Nombre = "A Plaga", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogo3 = new Catalogo 
            { 
                Id = 3, 
                Nombre = "M Plaga", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };

            await AddTestDataAsync(catalogo1);
            await AddTestDataAsync(catalogo2);
            await AddTestDataAsync(catalogo3);

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
                Activo = true
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
            var catalogo = new Catalogo 
            { 
                Id = 1, 
                Nombre = $"Test {tipo}", 
                Tipo = tipo,
                Activo = true
            };
            await AddTestDataAsync(catalogo);

            // Act
            var result = await _catalogoService.GetByType(tipo);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(tipo, result.Data[0].Tipo);
        }

        // Tests específicos para el método GetAllActive

        [Fact]
        public async Task GetAllActive_DebeRetornarSoloCatalogosActivos()
        {
            // Arrange
            var catalogoActivo1 = new Catalogo 
            { 
                Id = 1, 
                Nombre = "Activo 1", 
                Tipo = TipoCatalogoEnum.Plaga,
                Activo = true
            };
            var catalogoActivo2 = new Catalogo 
            { 
                Id = 2, 
                Nombre = "Activo 2", 
                Tipo = TipoCatalogoEnum.Maleza,
                Activo = true
            };
            var catalogoInactivo = new Catalogo 
            { 
                Id = 3, 
                Nombre = "Inactivo", 
                Tipo = TipoCatalogoEnum.Enfermedad,
                Activo = false,
            };
            var catalogoOtraLicencia = new Catalogo 
            { 
                Id = 4, 
                Nombre = "Otra Licencia", 
                Tipo = TipoCatalogoEnum.TipoFertilizante,
                Activo = true
            };

            await AddTestDataAsync(catalogoActivo1);
            await AddTestDataAsync(catalogoActivo2);
            await AddTestDataAsync(catalogoInactivo);
            await AddTestDataAsync(catalogoOtraLicencia);

            // Act
            var result = await _catalogoService.GetAllActive();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count); // Todos los activos (sin filtro por licencia)
            Assert.All(result.Data, c => 
            {
                Assert.True(c.Activo);
            });
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
    }
}
