using Microsoft.EntityFrameworkCore;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class PdfServiceTests : ServiceTestBase
    {
        private readonly IPdfService _pdfService;

        public PdfServiceTests()
        {
            _pdfService = GetService<IPdfService>();
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeGenerarPdfCorrectamente()
        {
            // Arrange
            var reporte = new ReporteCierreCampania
            {
                IdLicencia = 1,
                IdCampania = 1,
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now.AddMonths(-6),
                FechaFin = DateTime.Now,
                FechaCreacion = DateTime.Now,
                SuperficieTotalHa = 1000,
                ToneladasProducidas = 3325,
                CostoPorHaArs = 350,
                CostoPorToneladaArs = 105,
                CostoPorHaUsd = 1.17m,
                CostoPorToneladaUsd = 0.35m,
                RendimientoPromedioHa = 3.325m,
                AnalisisSueloArs = 5000,
                AnalisisSueloUsd = 16.67m,
                CostoSiembrasArs = 50000,
                CostoSiembrasUsd = 166.67m,
                CostoRiegosArs = 20000,
                CostoRiegosUsd = 66.67m,
                CostoPulverizacionesArs = 30000,
                CostoPulverizacionesUsd = 100m,
                CostoCosechasArs = 40000,
                CostoCosechasUsd = 133.33m,
                CostoMonitoreosArs = 10000,
                CostoMonitoreosUsd = 33.33m,
                CostoFertilizantesArs = 80000,
                CostoFertilizantesUsd = 266.67m,
                CostoOtrasLaboresArs = 15000,
                CostoOtrasLaboresUsd = 50m,
                GastosTotalesArs = 250000,
                GastosTotalesUsd = 833.33m,
                GastosPorCategoriaJson = @"[{""Categoria"":""Fertilizantes"",""CostoArs"":80000,""CostoUsd"":266.67}]",
                LluviaAcumuladaTotal = 800,
                LluviasPorMesJson = @"[{""Mes"":""Enero"",""Lluvia"":50},{""Mes"":""Febrero"",""Lluvia"":45}]",
                ResumenPorCultivoJson = @"[{""NombreCultivo"":""Trigo"",""SuperficieHa"":500,""ToneladasProducidas"":1600}]",
                ResumenPorCampoJson = @"[{""NombreCampo"":""Campo Norte"",""SuperficieHa"":500,""ToneladasProducidas"":1600}]",
                ResumenPorLoteJson = @"[{""NombreLote"":""Lote A-1"",""SuperficieHa"":250,""ToneladasProducidas"":800}]"
            };

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0); // El PDF debe tener contenido

            // Verificar que es un PDF válido (debe comenzar con el header PDF)
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            Assert.Equal(pdfHeader, result.Data.Take(4));
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeRetornarError_CuandoReporteNulo()
        {
            // Arrange
            ReporteCierreCampania reporte = null;

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeGenerarPdfConDatosMinimos()
        {
            // Arrange
            var reporte = new ReporteCierreCampania
            {
                IdLicencia = 1,
                IdCampania = 1,
                NombreCampania = "Campaña Test",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now,
                FechaCreacion = DateTime.Now
            };

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeManejarDatosVacios()
        {
            // Arrange
            var reporte = new ReporteCierreCampania
            {
                IdLicencia = 1,
                IdCampania = 1,
                NombreCampania = "",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now,
                FechaCreacion = DateTime.Now
            };

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeGenerarPdfConMuchosDatos()
        {
            // Arrange
            var reporte = new ReporteCierreCampania
            {
                IdLicencia = 1,
                IdCampania = 1,
                NombreCampania = "Campaña 2024",
                FechaInicio = DateTime.Now.AddMonths(-6),
                FechaFin = DateTime.Now,
                FechaCreacion = DateTime.Now,
                SuperficieTotalHa = 5000,
                ToneladasProducidas = 20160,
                CostoPorHaArs = 350,
                CostoPorToneladaArs = 87,
                CostoPorHaUsd = 1.17m,
                CostoPorToneladaUsd = 0.29m,
                RendimientoPromedioHa = 4.032m,
                GastosTotalesArs = 1750000,
                GastosTotalesUsd = 5833.33m,
                LluviaAcumuladaTotal = 1200
            };

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);
            Assert.True(result.Data.Length > 10000); // PDF grande con muchos datos
        }

        [Fact]
        public async Task GenerarPdfCierreCampaniaAsync_DebeManejarValoresNumericosExtremos()
        {
            // Arrange
            var reporte = new ReporteCierreCampania
            {
                IdLicencia = 1,
                IdCampania = 1,
                NombreCampania = "Campaña Test",
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now,
                FechaCreacion = DateTime.Now,
                SuperficieTotalHa = decimal.MaxValue,
                ToneladasProducidas = decimal.MaxValue,
                CostoPorHaArs = decimal.MaxValue,
                CostoPorToneladaArs = decimal.MaxValue,
                CostoPorHaUsd = decimal.MaxValue,
                CostoPorToneladaUsd = decimal.MaxValue,
                RendimientoPromedioHa = decimal.MaxValue
            };

            // Act
            var result = await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);
        }
    }
}
