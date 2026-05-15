using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class CierreCampaniaServiceTests : ServiceTestBase
    {
        private readonly ICierreCampaniaService _cierreCampaniaService;

        public CierreCampaniaServiceTests()
        {
            _cierreCampaniaService = GetService<ICierreCampaniaService>();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var reporte1 = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 1,
                IdCampania = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var reporte2 = new ReporteCierreCampania 
            { 
                Id = 2, 
                NombreCampania = "Campaña 2023",
                FechaCreacion = DateTime.Now,
                IdLicencia = 1,
                IdCampania = 2,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = TestUserAuth.UserName
            };
            var reporteOtraLicencia = new ReporteCierreCampania 
            { 
                Id = 3, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 2,
                IdCampania = 1,
                RegistrationDate = TimeHelper.GetArgentinaTime(),
                RegistrationUser = "otro_usuario"
            };

            await AddTestDataAsync(reporte1);
            await AddTestDataAsync(reporte2);
            await AddTestDataAsync(reporteOtraLicencia);

            // Act
            var result = await _cierreCampaniaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // Solo los de licencia 1
            Assert.All(result.Data, r => Assert.Equal(1, r.IdLicencia));
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarVacio_CuandoNoHayDatos()
        {
            // Act
            var result = await _cierreCampaniaService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual()
        {
            // Arrange
            var reporte1 = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 1,
                IdCampania = 1
            };
            var reporteOtraLicencia = new ReporteCierreCampania 
            { 
                Id = 2, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 2,
                IdCampania = 1
            };

            await AddTestDataAsync(reporte1);
            await AddTestDataAsync(reporteOtraLicencia);

            // Act
            var result = await _cierreCampaniaService.GetAllWithDetailsAsync();

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
            var reporte = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña Test",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(reporte);

            // Act
            var result = await _cierreCampaniaService.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Campaña Test", result.Data.NombreCampania);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido()
        {
            // Act
            var result = await _cierreCampaniaService.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
            Assert.Equal("No se encontró el registro", result.ErrorMessage);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNotFound_CuandoIdDeOtraLicencia()
        {
            // Arrange
            var reporteOtraLicencia = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(reporteOtraLicencia);

            // Act
            var result = await _cierreCampaniaService.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido()
        {
            // Arrange
            var reporte = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña Test",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(reporte);

            // Act
            var result = await _cierreCampaniaService.GetByIdWithDetailsAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal(1, result.Data.IdLicencia);
        }

        [Fact]
        public async Task CreateAsync_DebeAsignarLicenciaYCampania()
        {
            // Arrange
            var nuevoReporte = new ReporteCierreCampania 
            { 
                NombreCampania = "Nueva Campaña",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                // No asignar IdLicencia ni IdCampania
            };

            // Act
            var result = await _cierreCampaniaService.CreateAsync(nuevoReporte);

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
            var reporteOriginal = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña Original",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1,
                RegistrationDate = DateTime.Now.AddDays(-1),
                RegistrationUser = "usuario_original"
            };
            await AddTestDataAsync(reporteOriginal);

            var reporteActualizado = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña Actualizada",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };

            // Act
            var result = await _cierreCampaniaService.UpdateAsync(reporteActualizado);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Campaña Actualizada", result.Data.NombreCampania);
            Assert.Equal(1, result.Data.IdLicencia); // Preservado
            Assert.Equal(1, result.Data.IdCampania); // Preservado
            Assert.Equal("usuario_original", result.Data.RegistrationUser); // Preservado
        }

        [Fact]
        public async Task UpdateAsync_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var reporteInexistente = new ReporteCierreCampania 
            { 
                Id = 999, 
                NombreCampania = "Campaña Inexistente",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };

            // Act
            var result = await _cierreCampaniaService.UpdateAsync(reporteInexistente);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarSoloDeLicenciaActual()
        {
            // Arrange
            var reporteMismaLicencia = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };
            var reporteOtraLicencia = new ReporteCierreCampania 
            { 
                Id = 2, 
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(reporteMismaLicencia);
            await AddTestDataAsync(reporteOtraLicencia);

            // Clear change tracker to avoid EF tracking conflict
            DbContext.ChangeTracker.Clear();

            // Act
            var result = await _cierreCampaniaService.DeleteAsync(1);
            System.IO.File.AppendAllText("d:\\Repositorios\\AgroForm\\debug_log.txt",
                $"DEBUG CierreCampania DeleteAsync: Success={result.Success}, ErrorCode={result.ErrorCode}, ErrorMessage={result.ErrorMessage}{Environment.NewLine}");

            // Assert
            Assert.True(result.Success);
            
            // Verificar que solo se eliminó el de la licencia correcta
            var reporteRestante = await _cierreCampaniaService.GetByIdAsync(2);
            Assert.True(reporteRestante.Success); // El de otra licencia todavía existe
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoEsDeOtraLicencia()
        {
            // Arrange
            var reporteOtraLicencia = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(reporteOtraLicencia);

            // Act
            var result = await _cierreCampaniaService.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Act
            var result = await _cierreCampaniaService.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var reporte = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña Test",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMonths(6),
                IdLicencia = 1,
                IdCampania = 1
            };
            await AddTestDataAsync(reporte);

            // Act
            var exists = await _cierreCampaniaService.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarFalse_CuandoNoExiste()
        {
            // Act
            var exists = await _cierreCampaniaService.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetQuery_DebeRetornarQueryableConFiltros()
        {
            // Arrange
            var reporte1 = new ReporteCierreCampania 
            { 
                Id = 1, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 1,
                IdCampania = 1
            };
            var reporte2 = new ReporteCierreCampania 
            { 
                Id = 2, 
                NombreCampania = "Campaña 2024",
                FechaCreacion = DateTime.Now,
                IdLicencia = 2,
                IdCampania = 1
            };
            await AddTestDataAsync(reporte1);
            await AddTestDataAsync(reporte2);

            // Act
            var query = _cierreCampaniaService.GetQuery();

            // Assert
            Assert.NotNull(query);
            
            // Verificar que el query incluye solo los de la licencia actual
            var resultados = await query.ToListAsync();
            Assert.Single(resultados); // Solo el de licencia 1
            Assert.Equal(1, resultados[0].IdLicencia);
        }
    }
}
