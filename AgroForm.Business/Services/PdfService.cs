using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class PdfService : IPdfService
    {
    private readonly IWebHostEnvironment _environment;
    private readonly IChartGenerator _chartGenerator;

    public PdfService(IWebHostEnvironment environment, IChartGenerator chartGenerator)
    {
        _environment = environment;
        _chartGenerator = chartGenerator;
    }

    public async Task<byte[]> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte)
    {
        var document = new Document(PageSize.A4, 40, 40, 80, 40);
        var memoryStream = new MemoryStream();
        var writer = PdfWriter.GetInstance(document, memoryStream);

        document.Open();

        // Agregar header con logo y nombre
        await AgregarHeader(document, writer);
        
        // Agregar sección de datos generales
        AgregarDatosGenerales(document, reporte);
        
        // Agregar gráficos después de los datos generales
        await AgregarGraficosSection(document, reporte);
        
        // Agregar sección por cultivo
        AgregarDatosPorCultivo(document, reporte);
        
        // Agregar sección por campo
        AgregarDatosPorCampo(document, reporte);
        
        // Agregar sección climática
        AgregarDatosClimaticos(document, reporte);
        
        // Agregar footer
        AgregarFooter(document);

        document.Close();
        return memoryStream.ToArray();
    }

    private async Task AgregarHeader(Document document, PdfWriter writer)
    {
        try
        {
            // Intentar cargar logo
            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
            if (File.Exists(logoPath))
            {
                var logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(60, 60);
                logo.SetAbsolutePosition(40, document.PageSize.Height - 80);
                document.Add(logo);
            }
        }
        catch
        {
            // Si no hay logo, continuar sin él
        }

        // Nombre de la empresa
        var titleFont = new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD, new BaseColor(34, 139, 34)); // Verde agrícola
        var subtitleFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.GRAY);

        var empresaTitle = new Paragraph("AgroLab", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 5f
        };
        document.Add(empresaTitle);

        var reportTitle = new Paragraph("REPORTE DE CIERRE DE CAMPAÑA", subtitleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 15f
        };
        document.Add(reportTitle);

        // Línea decorativa
        var line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100, new BaseColor(34, 139, 34), Element.ALIGN_CENTER, 1)));
        document.Add(line);
    }

    private void AgregarDatosGenerales(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("📊 DATOS GENERALES", sectionFont)
        {
            SpacingBefore = 25f,
            SpacingAfter = 15f
        };
        document.Add(sectionTitle);

        // Tabla principal de datos generales
        var mainTable = new PdfPTable(2)
        {
            WidthPercentage = 100,
            SpacingBefore = 10f,
            SpacingAfter = 15f
        };
        mainTable.SetWidths(new float[] { 50, 50 });

        // Columna izquierda - Información básica
        var leftCell = new PdfPCell();
        leftCell.Border = Rectangle.NO_BORDER;
        leftCell.Padding = 5;

        var infoTable = new PdfPTable(2);
        infoTable.SetWidths(new float[] { 40, 60 });

        AgregarFilaInfo(infoTable, "Fecha Inicio:", reporte.FechaInicio.ToString("MM/yyyy"));
        AgregarFilaInfo(infoTable, "Fecha Fin:", reporte.FechaFin.ToString("MM/yyyy"));
        AgregarFilaInfo(infoTable, "Superficie Total:", $"{reporte.SuperficieTotalHa:N2} Ha");
        AgregarFilaInfo(infoTable, "Producción Total:", $"{reporte.ToneladasProducidas:N2} Ton");
        AgregarFilaInfo(infoTable, "Rendimiento Promedio:", $"{reporte.RendimientoPromedioHa:N2} Ton/Ha");

        leftCell.AddElement(infoTable);

        // Columna derecha - Costos
        var rightCell = new PdfPCell();
        rightCell.Border = Rectangle.NO_BORDER;
        rightCell.Padding = 5;

        var costTable = new PdfPTable(2);
        costTable.SetWidths(new float[] { 60, 40 });

        AgregarFilaCosto(costTable, "Costo Total:", reporte.CostoTotalArs, reporte.CostoTotalUsd);
        AgregarFilaCosto(costTable, "Costo por Ha:", reporte.CostoPorHa, reporte.CostoPorHa / (reporte.TipoCambio ?? 1));
        AgregarFilaCosto(costTable, "Costo por Ton:", reporte.CostoPorTonelada, reporte.CostoPorTonelada / (reporte.TipoCambio ?? 1));

        rightCell.AddElement(costTable);

        mainTable.AddCell(leftCell);
        mainTable.AddCell(rightCell);
        document.Add(mainTable);

        // Tabla de desglose de costos
        AgregarTablaCostosDetallados(document, reporte);
        
        // Tabla de resumen de labores
        AgregarTablaResumenLabores(document, reporte);
    }

    private async Task AgregarGraficosSection(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("📈 GRÁFICOS Y ESTADÍSTICAS", sectionFont)
        {
            SpacingBefore = 25f,
            SpacingAfter = 15f
        };
        document.Add(sectionTitle);

        // Gráfico de distribución de cultivos
        await AgregarGraficoTortaCultivos(document, reporte);
        
        // Gráfico de producción
        AgregarGraficoBarrasProduccion(document, reporte);
        
        // Gráfico de costos
        AgregarGraficoBarrasCostos(document, reporte);
        
        // Gráfico de lluvias
        AgregarGraficoLineasLluvias(document, reporte);
    }

    private async Task AgregarGraficoTortaCultivos(Document document, ReporteCierreCampania reporte)
    {
        var title = new Paragraph("🌱 Distribución de Cultivos", 
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80)))
        {
            SpacingBefore = 20f,
            SpacingAfter = 10f,
            Alignment = Element.ALIGN_CENTER
        };
        document.Add(title);

        if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
        {
            var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
            
            // Generar gráfico de torta
            var chartData = cultivos.ToDictionary(c => c.NombreCultivo, c => c.SuperficieHa);
            var chartImage = await _chartGenerator.GenerarGraficoTortaAsync(chartData, "Distribución de Cultivos");
            
            if (chartImage != null)
            {
                var image = Image.GetInstance(chartImage);
                image.ScaleToFit(400, 250);
                image.Alignment = Image.ALIGN_CENTER;
                document.Add(image);
            }
            else
            {
                // Fallback a tabla si no se puede generar el gráfico
                AgregarTablaDistribucionCultivos(document, cultivos);
            }
        }
    }

    private void AgregarGraficoBarrasProduccion(Document document, ReporteCierreCampania reporte)
    {
        var title = new Paragraph("📦 Producción por Cultivo", 
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80)))
        {
            SpacingBefore = 25f,
            SpacingAfter = 10f,
            Alignment = Element.ALIGN_CENTER
        };
        document.Add(title);

        if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
        {
            var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
            var maxProduccion = cultivos.Max(c => c.ToneladasProducidas);
            
            var barChartTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingAfter = 15f
            };

            foreach (var cultivo in cultivos)
            {
                var porcentaje = maxProduccion > 0 ? (cultivo.ToneladasProducidas / maxProduccion) * 100 : 0;
                var barras = (int)(porcentaje / 5); // Cada █ representa 5%
                var barra = new string('█', barras);
                
                barChartTable.AddCell(CrearCelda($"{cultivo.NombreCultivo}:", Element.ALIGN_LEFT));
                barChartTable.AddCell(CrearCelda($"{barra} {cultivo.ToneladasProducidas:N1} Ton", Element.ALIGN_RIGHT));
            }

            document.Add(barChartTable);
        }
    }

    private void AgregarGraficoBarrasCostos(Document document, ReporteCierreCampania reporte)
    {
        var title = new Paragraph("💰 Distribución de Costos", 
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80)))
        {
            SpacingBefore = 25f,
            SpacingAfter = 10f,
            Alignment = Element.ALIGN_CENTER
        };
        document.Add(title);

        var costos = new[]
        {
            new { Nombre = "Cosechas", Valor = reporte.CostoCosechasArs },
            new { Nombre = "Siembras", Valor = reporte.CostoSiembrasArs },
            new { Nombre = "Fertilizantes", Valor = reporte.CostoFertilizantesArs },
            new { Nombre = "Riegos", Valor = reporte.CostoRiegosArs },
            new { Nombre = "Pulverizaciones", Valor = reporte.CostoPulverizacionesArs },
            new { Nombre = "Análisis Suelo", Valor = reporte.AnalisisSueloArs },
            new { Nombre = "Monitoreos", Valor = reporte.CostoMonitoreosArs },
            new { Nombre = "Otras Labores", Valor = reporte.CostoOtrasLaboresArs }
        };

        var maxCosto = costos.Max(c => c.Valor);
        var costoTable = new PdfPTable(2)
        {
            WidthPercentage = 100,
            SpacingAfter = 15f
        };

        foreach (var costo in costos)
        {
            if (costo.Valor > 0)
            {
                var porcentaje = maxCosto > 0 ? (costo.Valor / maxCosto) * 100 : 0;
                var barras = (int)(porcentaje / 3); // Cada █ representa 3%
                var barra = new string('█', Math.Max(1, barras));
                
                costoTable.AddCell(CrearCelda($"{costo.Nombre}:", Element.ALIGN_LEFT));
                costoTable.AddCell(CrearCelda($"{barra} ${costo.Valor:N0}", Element.ALIGN_RIGHT));
            }
        }

        document.Add(costoTable);
    }

    private void AgregarGraficoLineasLluvias(Document document, ReporteCierreCampania reporte)
    {
        var title = new Paragraph("🌧️ Lluvias por Mes", 
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80)))
        {
            SpacingBefore = 25f,
            SpacingAfter = 10f,
            Alignment = Element.ALIGN_CENTER
        };
        document.Add(title);

        if (!string.IsNullOrEmpty(reporte.LluviasPorMesJson))
        {
            var lluviasPorMes = JsonSerializer.Deserialize<List<LluviaMes>>(reporte.LluviasPorMesJson);
            var maxLluvia = lluviasPorMes.Max(l => l.Lluvia);
            
            var chartTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingAfter = 15f
            };

            foreach (var lluvia in lluviasPorMes)
            {
                var porcentaje = maxLluvia > 0 ? (lluvia.Lluvia / maxLluvia) * 100 : 0;
                var puntos = (int)(porcentaje / 10); // Cada ● representa 10%
                var linea = new string('●', Math.Max(1, puntos));
                
                chartTable.AddCell(CrearCelda($"{lluvia.Mes}:", Element.ALIGN_LEFT));
                chartTable.AddCell(CrearCelda($"{linea} {lluvia.Lluvia} mm", Element.ALIGN_RIGHT));
            }

            document.Add(chartTable);
        }
    }

    private void AgregarDatosPorCultivo(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("🌿 DATOS POR CULTIVO", sectionFont)
        {
            SpacingBefore = 25f,
            SpacingAfter = 15f
        };
        document.Add(sectionTitle);

        if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
        {
            var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
            
            foreach (var cultivo in cultivos)
            {
                var cultivoTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };
                cultivoTable.SetWidths(new float[] { 25, 25, 25, 25 });

                // Header del cultivo
                var headerCell = new PdfPCell(new Phrase(cultivo.NombreCultivo, 
                    new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)));
                headerCell.BackgroundColor = new BaseColor(52, 152, 219);
                headerCell.Colspan = 4;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                cultivoTable.AddCell(headerCell);

                // Datos del cultivo
                cultivoTable.AddCell(CrearCeldaCentrada($"Superficie:\n{cultivo.SuperficieHa:N2} Ha"));
                cultivoTable.AddCell(CrearCeldaCentrada($"Producción:\n{cultivo.ToneladasProducidas:N2} Ton"));
                cultivoTable.AddCell(CrearCeldaCentrada($"Rendimiento:\n{cultivo.RendimientoHa:N2} Ton/Ha"));
                cultivoTable.AddCell(CrearCeldaCentrada($"Costo Total:\n${cultivo.CostoTotal:N2}"));

                document.Add(cultivoTable);
            }
        }
    }

    private void AgregarDatosPorCampo(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("🏞️ DATOS POR CAMPO", sectionFont)
        {
            SpacingBefore = 25f,
            SpacingAfter = 15f
        };
        document.Add(sectionTitle);

        // Implementar según tu estructura de datos de campos
        var infoCampo = new Paragraph("Los datos detallados por campo y lote están disponibles en el sistema web.", 
            new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 15f
        };
        document.Add(infoCampo);
    }

    private void AgregarDatosClimaticos(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("🌤️ DATOS CLIMÁTICOS", sectionFont)
        {
            SpacingBefore = 25f,
            SpacingAfter = 15f
        };
        document.Add(sectionTitle);

        var climaTable = new PdfPTable(2)
        {
            WidthPercentage = 100,
            SpacingBefore = 10f
        };
        climaTable.SetWidths(new float[] { 50, 50 });

        climaTable.AddCell(CrearCelda($"Lluvia Acumulada Total:\n{reporte.LluviaAcumuladaTotal:N1} mm", Element.ALIGN_CENTER));

        if (!string.IsNullOrEmpty(reporte.EventosExtremosJson))
        {
            var eventos = JsonSerializer.Deserialize<List<dynamic>>(reporte.EventosExtremosJson);
            climaTable.AddCell(CrearCelda($"Eventos Extremos:\n{eventos.Count} registros", Element.ALIGN_CENTER));
        }
        else
        {
            climaTable.AddCell(CrearCelda("Eventos Extremos:\nSin registros", Element.ALIGN_CENTER));
        }

        document.Add(climaTable);
    }

    private void AgregarTablaCostosDetallados(Document document, ReporteCierreCampania reporte)
    {
        var subtitleFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80));
        var subtitle = new Paragraph("Desglose de Costos", subtitleFont)
        {
            SpacingBefore = 15f,
            SpacingAfter = 10f
        };
        document.Add(subtitle);

        var costosTable = new PdfPTable(3)
        {
            WidthPercentage = 100,
            SpacingAfter = 15f
        };
        costosTable.SetWidths(new float[] { 50, 25, 25 });

        // Header
        costosTable.AddCell(CrearCeldaHeader("Concepto"));
        costosTable.AddCell(CrearCeldaHeader("ARS"));
        costosTable.AddCell(CrearCeldaHeader("USD"));

        // Filas de costos
        AgregarFilaCostoDetallado(costosTable, "Cosechas", reporte.CostoCosechasArs, reporte.CostoCosechasUsd);
        AgregarFilaCostoDetallado(costosTable, "Siembras", reporte.CostoSiembrasArs, reporte.CostoSiembrasUsd);
        AgregarFilaCostoDetallado(costosTable, "Fertilizantes", reporte.CostoFertilizantesArs, reporte.CostoFertilizantesUsd);
        AgregarFilaCostoDetallado(costosTable, "Riegos", reporte.CostoRiegosArs, reporte.CostoRiegosUsd);
        AgregarFilaCostoDetallado(costosTable, "Pulverizaciones", reporte.CostoPulverizacionesArs, reporte.CostoPulverizacionesUsd);
        AgregarFilaCostoDetallado(costosTable, "Análisis de Suelo", reporte.AnalisisSueloArs, reporte.AnalisisSueloUsd);
        AgregarFilaCostoDetallado(costosTable, "Monitoreos", reporte.CostoMonitoreosArs, reporte.CostoMonitoreosUsd);
        AgregarFilaCostoDetallado(costosTable, "Otras Labores", reporte.CostoOtrasLaboresArs, reporte.CostoOtrasLaboresUsd);

        // Total
        costosTable.AddCell(CrearCelda("TOTAL", Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLACK, Element.ALIGN_RIGHT));
        costosTable.AddCell(CrearCelda($"${reporte.CostoTotalArs:N2}", Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLACK, Element.ALIGN_RIGHT));
        costosTable.AddCell(CrearCelda($"${reporte.CostoTotalUsd:N2}", Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLACK, Element.ALIGN_RIGHT));

        document.Add(costosTable);
    }

    private void AgregarTablaResumenLabores(Document document, ReporteCierreCampania reporte)
    {
        var subtitleFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, new BaseColor(44, 62, 80));
        var subtitle = new Paragraph("Resumen de Labores", subtitleFont)
        {
            SpacingBefore = 15f,
            SpacingAfter = 10f
        };
        document.Add(subtitle);

        var laboresTable = new PdfPTable(4)
        {
            WidthPercentage = 100,
            SpacingAfter = 15f
        };
        laboresTable.SetWidths(new float[] { 25, 25, 25, 25 });

        laboresTable.AddCell(CrearCeldaHeader("Siembras"));
        laboresTable.AddCell(CrearCeldaHeader("Riegos"));
        laboresTable.AddCell(CrearCeldaHeader("Pulverizaciones"));
        laboresTable.AddCell(CrearCeldaHeader("Cosechas"));

        // Aquí deberías agregar los conteos reales de cada tipo de labor
        // Por ahora, valores de ejemplo
        laboresTable.AddCell(CrearCeldaCentrada("15"));
        laboresTable.AddCell(CrearCeldaCentrada("42"));
        laboresTable.AddCell(CrearCeldaCentrada("28"));
        laboresTable.AddCell(CrearCeldaCentrada("8"));

        document.Add(laboresTable);
    }

    private void AgregarTablaDistribucionCultivos(Document document, List<ResumenCultivo> cultivos)
    {
        var distribucionTable = new PdfPTable(3)
        {
            WidthPercentage = 80,
            HorizontalAlignment = Element.ALIGN_CENTER,
            SpacingAfter = 15f
        };

        distribucionTable.AddCell(CrearCeldaHeader("Cultivo"));
        distribucionTable.AddCell(CrearCeldaHeader("Superficie (Ha)"));
        distribucionTable.AddCell(CrearCeldaHeader("Porcentaje"));

        var totalSuperficie = cultivos.Sum(c => c.SuperficieHa);

        foreach (var cultivo in cultivos)
        {
            var porcentaje = totalSuperficie > 0 ? (cultivo.SuperficieHa / totalSuperficie) * 100 : 0;
            
            distribucionTable.AddCell(CrearCelda(cultivo.NombreCultivo));
            distribucionTable.AddCell(CrearCelda(cultivo.SuperficieHa.ToString("N2"), Element.ALIGN_RIGHT));
            distribucionTable.AddCell(CrearCelda($"{porcentaje:N1}%", Element.ALIGN_RIGHT));
        }

        document.Add(distribucionTable);
    }

    private void AgregarFooter(Document document)
    {
        var footer = new Paragraph($"AgroLab - Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}", 
            new Font(Font.FontFamily.HELVETICA, 8, Font.ITALIC, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingBefore = 20f
        };
        document.Add(footer);
    }

    #region Métodos auxiliares
    private void AgregarFilaInfo(PdfPTable table, string label, string value)
    {
        table.AddCell(CrearCelda(label, Font.FontFamily.HELVETICA, 9, Font.BOLD, BaseColor.BLACK));
        table.AddCell(CrearCelda(value, Font.FontFamily.HELVETICA, 9, Font.NORMAL, BaseColor.BLACK));
    }

    private void AgregarFilaCosto(PdfPTable table, string label, decimal ars, decimal usd)
    {
        table.AddCell(CrearCelda(label, Font.FontFamily.HELVETICA, 9, Font.BOLD, BaseColor.BLACK));
        table.AddCell(CrearCelda($"${ars:N2} (${usd:N2})", Font.FontFamily.HELVETICA, 9, Font.NORMAL, BaseColor.BLACK));
    }

    private void AgregarFilaCostoDetallado(PdfPTable table, string concepto, decimal ars, decimal usd)
    {
        table.AddCell(CrearCelda(concepto));
        table.AddCell(CrearCelda($"${ars:N2}", Element.ALIGN_RIGHT));
        table.AddCell(CrearCelda($"${usd:N2}", Element.ALIGN_RIGHT));
    }

    private PdfPCell CrearCelda(string texto, string fontFamily = Font.FontFamily.HELVETICA, float size = 10, int style = Font.NORMAL, BaseColor color = null, int alignment = Element.ALIGN_LEFT)
    {
        var font = new Font(fontFamily, size, style, color ?? BaseColor.BLACK);
        var cell = new PdfPCell(new Phrase(texto, font))
        {
            Padding = 8,
            Border = Rectangle.BOTTOM_BORDER,
            BorderColor = BaseColor.LIGHT_GRAY,
            BorderWidth = 0.5f,
            HorizontalAlignment = alignment
        };
        return cell;
    }

    private PdfPCell CrearCeldaCentrada(string texto)
    {
        return CrearCelda(texto, alignment: Element.ALIGN_CENTER);
    }

    private PdfPCell CrearCeldaHeader(string texto)
    {
        var cell = CrearCelda(texto, Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE, Element.ALIGN_CENTER);
        cell.BackgroundColor = new BaseColor(44, 62, 80);
        return cell;
    }
    #endregion
}

    }

public interface IChartGenerator
{
    Task<byte[]> GenerarGraficoTortaAsync(Dictionary<string, decimal> datos, string titulo);
    Task<byte[]> GenerarGraficoBarrasAsync(Dictionary<string, decimal> datos, string titulo);
}

public class ChartGenerator : IChartGenerator
{
    private readonly BaseColor[] _colores = new[]
    {
        new SKColor(65, 105, 225),   // Royal Blue
        new SKColor(34, 139, 34),    // Forest Green
        new SKColor(220, 20, 60),    // Crimson
        new SKColor(255, 140, 0),    // Dark Orange
        new SKColor(138, 43, 226),   // Blue Violet
        new SKColor(50, 205, 50),    // Lime Green
        new SKColor(210, 105, 30),   // Chocolate
        new SKColor(70, 130, 180)    // Steel Blue
    };

    public async Task<byte[]> GenerarGraficoTortaAsync(Dictionary<string, decimal> datos, string titulo)
    {
        try
        {
            const int width = 600;
            const int height = 400;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Fondo blanco
            canvas.Clear(SKColors.White);

            var total = datos.Values.Sum();
            var startAngle = 0f;

            // Dibujar gráfico de torta
            var rect = new SKRect(50, 80, width - 50, height - 80);
            var i = 0;

            foreach (var item in datos)
            {
                var sweepAngle = (float)((item.Value / total) * 360);
                var color = _colores[i % _colores.Length];

                using var paint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                using var path = new SKPath();
                path.MoveTo(rect.MidX, rect.MidY);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();

                canvas.DrawPath(path, paint);

                // Dibujar borde
                using var borderPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };
                canvas.DrawPath(path, borderPaint);

                startAngle += sweepAngle;
                i++;
            }

            // Dibujar título
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 20,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(titulo, width / 2, 40, titlePaint);

            // Leyenda
            var legendY = 100f;
            i = 0;

            using var legendPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 12
            };

            foreach (var item in datos)
            {
                var porcentaje = (item.Value / total) * 100;
                var color = _colores[i % _colores.Length];

                // Cuadrado de color
                using var colorPaint = new SKPaint { Color = color, IsAntialias = true };
                canvas.DrawRect(width - 150, legendY - 10, 15, 15, colorPaint);

                // Texto
                var texto = $"{item.Key}: {porcentaje:F1}%";
                canvas.DrawText(texto, width - 130, legendY, legendPaint);

                legendY += 25;
                i++;
            }

            // Convertir a imagen
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch
        {
            return null; // Fallback a tablas si hay error
        }
    }

    public async Task<byte[]> GenerarGraficoBarrasAsync(Dictionary<string, decimal> datos, string titulo)
    {
        try
        {
            const int width = 600;
            const int height = 400;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Fondo blanco
            canvas.Clear(SKColors.White);

            var maxValue = datos.Values.Max();
            var barWidth = (width - 100) / datos.Count;
            var x = 50f;
            var i = 0;

            // Dibujar título
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 20,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(titulo, width / 2, 30, titlePaint);

            // Dibujar barras
            foreach (var item in datos)
            {
                var barHeight = (float)((item.Value / maxValue) * (height - 100));
                var color = _colores[i % _colores.Length];

                using var paint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawRect(x, height - barHeight - 50, barWidth - 10, barHeight, paint);

                // Etiqueta del valor
                using var valuePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 10,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText(item.Value.ToString("N0"), x + (barWidth - 10) / 2, height - barHeight - 55, valuePaint);

                // Etiqueta del nombre
                using var labelPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 10,
                    TextAlign = SKTextAlign.Center
                };

                // Rotar texto si es necesario
                canvas.Save();
                canvas.Translate(x + (barWidth - 10) / 2, height - 25);
                canvas.RotateDegrees(-45);
                canvas.DrawText(item.Key, 0, 0, labelPaint);
                canvas.Restore();

                x += barWidth;
                i++;
            }

            // Convertir a imagen
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch
        {
            return null; // Fallback a tablas si hay error
        }
    }
}

public class LluviaMes
{
    public string Mes { get; set; }
    public decimal Lluvia { get; set; }
}

public class ResumenCultivo
{
    public string NombreCultivo { get; set; }
    public decimal SuperficieHa { get; set; }
    public decimal ToneladasProducidas { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal RendimientoHa { get; set; }
}
}
