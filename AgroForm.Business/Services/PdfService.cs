using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
public class PdfService : IPdfService
{
    private readonly IWebHostEnvironment _environment;

    public PdfService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<byte[]> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte)
    {
        var document = new Document(PageSize.A4, 40, 40, 60, 40);
        var memoryStream = new MemoryStream();
        var writer = PdfWriter.GetInstance(document, memoryStream);

        document.Open();

        // Agregar header
        await AgregarHeader(document, reporte);
        
        // Agregar sección de datos generales
        AgregarDatosGenerales(document, reporte);
        
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

    private async Task AgregarHeader(Document document, ReporteCierreCampania reporte)
    {
        // Logo (opcional)
        var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
        if (File.Exists(logoPath))
        {
            var logo = Image.GetInstance(logoPath);
            logo.ScaleToFit(80, 80);
            logo.SetAbsolutePosition(40, document.PageSize.Height - 80);
            document.Add(logo);
        }

        var titleFont = new Font(Font.FontFamily.HELVETICA, 20, Font.BOLD, BaseColor.DARK_GRAY);
        var subtitleFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.GRAY);

        var title = new Paragraph("REPORTE DE CIERRE DE CAMPAÑA", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 5f
        };
        document.Add(title);

        var campaignName = new Paragraph(reporte.NombreCampania.ToUpper(), subtitleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 15f
        };
        document.Add(campaignName);

        // Línea decorativa
        var line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, 1)));
        document.Add(line);
    }

    private void AgregarDatosGenerales(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("📊 DATOS GENERALES", sectionFont)
        {
            SpacingBefore = 20f,
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

    private void AgregarDatosPorCultivo(Document document, ReporteCierreCampania reporte)
    {
        var sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, new BaseColor(44, 62, 80));
        var sectionTitle = new Paragraph("🌱 DATOS POR CULTIVO", sectionFont)
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
                var headerCell = new PdfPCell(new Phrase(cultivo.NombreCultivo, new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)));
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

        // Similar a la sección de cultivos pero para campos
        // Implementar según tu estructura de datos
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

        laboresTable.AddCell(CrearCeldaCentrada("Siembras"));
        laboresTable.AddCell(CrearCeldaCentrada("Riegos"));
        laboresTable.AddCell(CrearCeldaCentrada("Pulverizaciones"));
        laboresTable.AddCell(CrearCeldaCentrada("Cosechas"));

        // Aquí deberías agregar los conteos reales de cada tipo de labor
        laboresTable.AddCell(CrearCeldaCentrada("X"));
        laboresTable.AddCell(CrearCeldaCentrada("X"));
        laboresTable.AddCell(CrearCeldaCentrada("X"));
        laboresTable.AddCell(CrearCeldaCentrada("X"));

        document.Add(laboresTable);
    }

    private void AgregarFooter(Document document)
    {
        var footer = new Paragraph($"Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}", 
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
