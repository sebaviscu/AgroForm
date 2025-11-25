using AgroForm.Business.Contracts;
using AgroForm.Model;
using AlbaServicios.Services;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Text.Json;

namespace AgroForm.Business.Services
{
    public class PdfService : IPdfService
    {
        private readonly Color _colorPrimario = new DeviceRgb(44, 62, 80);    // Gris oscuro profesional
        private readonly Color _colorSecundario = new DeviceRgb(52, 73, 94);  // Gris medio
        private readonly Color _colorAcento = new DeviceRgb(26, 82, 118);     // Azul oscuro
        private readonly Color _colorTexto = new DeviceRgb(44, 62, 80);       // Texto oscuro
        private readonly Color _colorFondo = new DeviceRgb(248, 249, 250);    // Gris claro
        private readonly Color _colorBorde = new DeviceRgb(189, 195, 199);    // Gris borde

        public async Task<OperationResult<byte[]>> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                // Configurar márgenes profesionales
                document.SetMargins(20, 25, 20, 25);

                // Agregar secciones
                AgregarHeaderProfesional(document, reporte);
                AgregarIndicadoresPrincipales(document, reporte);
                AgregarDistribucionCostos(document, reporte);

                if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
                    AgregarDatosPorCultivoProfesional(document, reporte);

                AgregarDatosClimaticosProfesional(document, reporte);
                AgregarFooterProfesional(document);

                document.Close();

                return OperationResult<byte[]>.SuccessResult(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                return OperationResult<byte[]>.Failure($"Error al generar PDF: {e.Message}");
            }
        }

        private void AgregarHeaderProfesional(Document document, ReporteCierreCampania reporte)
        {
            var headerTable = new Table(1)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            var headerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);

            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var subtitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título principal
            var mainTitle = new Paragraph($"INFORME DE CIERRE {reporte.NombreCampania.ToUpper()}")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetFontColor(_colorPrimario)
                .SetMarginBottom(8);
            headerCell.Add(mainTitle);

            // Período
            var period = new Paragraph($"Período: {reporte.FechaInicio:MM/yyyy} al {reporte.FechaFin:MM/yyyy}")
                .SetFont(subtitleFont)
                .SetFontSize(10)
                .SetFontColor(_colorSecundario);
            headerCell.Add(period);

            headerTable.AddCell(headerCell);
            document.Add(headerTable);
        }

        private void AgregarIndicadoresPrincipales(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccionProfesional("INDICADORES");
            document.Add(sectionTitle);

            var indicadoresTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20)
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15);

            // Métricas operativas
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.SuperficieTotalHa:N1} ha", "Superficie Total"));
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.ToneladasProducidas:N0} Tn", "Producción Total"));
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.RendimientoPromedioHa:N1} Tn/ha", "Rendimiento Promedio"));

            // Inversión Total con ambas monedas
            var inversionContent = new Div()
                .Add(new Paragraph($"${reporte.CostoTotalArs:N0} ARS")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(11)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"${reporte.CostoTotalUsd:N0} USD")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9)
                    .SetFontColor(_colorSecundario));

            var inversionCell = new Cell()
                .Add(inversionContent)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .Add(new Paragraph("Inversión Total")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8)
                    .SetFontColor(_colorSecundario)
                    .SetMarginTop(4));

            indicadoresTable.AddCell(inversionCell);

            document.Add(indicadoresTable);

            // Segunda fila - Costos por unidad
            var costosTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0)
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15);

            // Costo por Hectárea
            var costoHaContent = new Div()
                .Add(new Paragraph($"${reporte.CostoPorHaArs:N0} ARS")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(11)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"${reporte.CostoPorHaUsd:N0} USD")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9)
                    .SetFontColor(_colorSecundario));

            var costoHaCell = new Cell()
                .Add(costoHaContent)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .Add(new Paragraph("Costo por Hectárea")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8)
                    .SetFontColor(_colorSecundario)
                    .SetMarginTop(4));

            costosTable.AddCell(costoHaCell);

            // Costo por Tonelada
            var costoTnContent = new Div()
                .Add(new Paragraph($"${reporte.CostoPorToneladaArs:N0} ARS")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(11)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"${reporte.CostoPorToneladaUsd:N0} USD")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9)
                    .SetFontColor(_colorSecundario));

            var costoTnCell = new Cell()
                .Add(costoTnContent)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .Add(new Paragraph("Costo por Tonelada")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8)
                    .SetFontColor(_colorSecundario)
                    .SetMarginTop(4));

            costosTable.AddCell(costoTnCell);

            document.Add(costosTable);
        }

        private void AgregarDistribucionCostos(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccionProfesional("DISTRIBUCIÓN DE COSTOS OPERATIVOS");
            document.Add(sectionTitle);

            var costosTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(10);

            // Header de la tabla
            costosTable.AddCell(CrearCeldaHeaderProfesional("CONCEPTO OPERATIVO"));
            costosTable.AddCell(CrearCeldaHeaderProfesional("INVERSIÓN ARS"));
            costosTable.AddCell(CrearCeldaHeaderProfesional("INVERSIÓN USD"));
            costosTable.AddCell(CrearCeldaHeaderProfesional("PARTICIPACIÓN"));

            // Datos de costos
            var costos = new[]
            {
                new { Nombre = "Cosechas", Ars = reporte.CostoCosechasArs, Usd = reporte.CostoCosechasUsd },
                new { Nombre = "Siembras", Ars = reporte.CostoSiembrasArs, Usd = reporte.CostoSiembrasUsd },
                new { Nombre = "Fertilizantes", Ars = reporte.CostoFertilizantesArs, Usd = reporte.CostoFertilizantesUsd },
                new { Nombre = "Riegos", Ars = reporte.CostoRiegosArs, Usd = reporte.CostoRiegosUsd },
                new { Nombre = "Pulverizaciones", Ars = reporte.CostoPulverizacionesArs, Usd = reporte.CostoPulverizacionesUsd },
                new { Nombre = "Análisis de Suelo", Ars = reporte.AnalisisSueloArs, Usd = reporte.AnalisisSueloUsd },
                new { Nombre = "Monitoreos", Ars = reporte.CostoMonitoreosArs, Usd = reporte.CostoMonitoreosUsd },
                new { Nombre = "Otras Labores", Ars = reporte.CostoOtrasLaboresArs, Usd = reporte.CostoOtrasLaboresUsd }
            };

            foreach (var costo in costos)
            {
                if (costo.Ars > 0)
                {
                    var porcentaje = reporte.CostoTotalArs > 0 ? (costo.Ars / reporte.CostoTotalArs) * 100 : 0;

                    costosTable.AddCell(CrearCeldaDatosProfesional(costo.Nombre));
                    costosTable.AddCell(CrearCeldaDatosProfesional($"${costo.Ars:N0}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatosProfesional($"${costo.Usd:N2}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatosProfesional($"{porcentaje:N1}%", TextAlignment.RIGHT));
                }
            }

            // Total
            costosTable.AddCell(CrearCeldaTotalProfesional("TOTAL GENERAL"));
            costosTable.AddCell(CrearCeldaTotalProfesional($"${reporte.CostoTotalArs:N0}"));
            costosTable.AddCell(CrearCeldaTotalProfesional($"${reporte.CostoTotalUsd:N2}"));
            costosTable.AddCell(CrearCeldaTotalProfesional("100%"));

            document.Add(costosTable);
        }

        private void AgregarDatosPorCultivoProfesional(Document document, ReporteCierreCampania reporte)
        {
            try
            {
                var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
                if (cultivos == null || !cultivos.Any()) return;

                var sectionTitle = CrearTituloSeccionProfesional("ANÁLISIS POR CULTIVO");
                document.Add(sectionTitle);

                // Crear tabla principal con 3 columnas (3 cultivos por fila)
                var mainTable = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(10);

                for (int i = 0; i < cultivos.Count; i++)
                {
                    var cultivo = cultivos[i];

                    // Crear tarjeta individual para cada cultivo
                    var cultivoCard = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMargin(5)
                        .SetBackgroundColor(_colorFondo)
                        .SetPadding(12)
                        .SetBorder(new SolidBorder(_colorBorde, 1))
                        .SetBorderRadius(new BorderRadius(4));

                    // Header del cultivo
                    var headerCell = new Cell()
                        .Add(new Paragraph(cultivo.NombreCultivo.ToUpper())
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(10)
                            .SetFontColor(_colorPrimario))
                        .SetBorderBottom(new SolidBorder(_colorPrimario, 1))
                        .SetPaddingBottom(8)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(5);
                    cultivoCard.AddCell(headerCell);

                    // Métricas del cultivo en tabla 2x2
                    var metricsTable = new Table(2)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(5);

                    // Fila 1
                    metricsTable.AddCell(CrearCeldaCultivoCompacta("Superficie", $"{cultivo.SuperficieHa:N1} ha"));
                    metricsTable.AddCell(CrearCeldaCultivoCompacta("Producción", $"{cultivo.ToneladasProducidas:N1} Tn"));

                    // Fila 2
                    metricsTable.AddCell(CrearCeldaCultivoCompacta("Rendimiento", $"{cultivo.RendimientoHa:N1} Tn/ha"));
                    metricsTable.AddCell(CrearCeldaCultivoCompacta("Inversión", $"${cultivo.CostoTotal:N0}"));

                    cultivoCard.AddCell(new Cell().Add(metricsTable).SetBorder(Border.NO_BORDER));

                    // Agregar la tarjeta a la tabla principal
                    var containerCell = new Cell()
                        .Add(cultivoCard)
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(3);

                    mainTable.AddCell(containerCell);

                    // Si es el último cultivo y no completa una fila, agregar celdas vacías
                    if (i == cultivos.Count - 1)
                    {
                        int remainingCells = 3 - ((i + 1) % 3);
                        if (remainingCells < 3) // Solo si no está completa la fila
                        {
                            for (int j = 0; j < remainingCells; j++)
                            {
                                mainTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                            }
                        }
                    }
                }

                document.Add(mainTable);
            }
            catch (Exception)
            {
                // Si hay error en la deserialización, omitir esta sección
            }
        }

        private Cell CrearCeldaCultivoCompacta(string label, string valor)
        {
            return new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph(label)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(_colorSecundario));
        }

        private void AgregarDatosClimaticosProfesional(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccionProfesional("CONDICIONES CLIMÁTICAS");
            document.Add(sectionTitle);

            var climaTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(10);

            int eventosCount = 0;
            if (!string.IsNullOrEmpty(reporte.EventosExtremosJson))
            {
                try
                {
                    var eventos = JsonSerializer.Deserialize<List<dynamic>>(reporte.EventosExtremosJson);
                    eventosCount = eventos?.Count ?? 0;
                }
                catch (Exception)
                {
                    eventosCount = 0;
                }
            }

            // Precipitación
            var lluviaCell = new Cell()
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15)
                .SetBorder(new SolidBorder(_colorBorde, 1))
                .SetTextAlignment(TextAlignment.CENTER);

            lluviaCell.Add(new Paragraph("PRECIPITACIÓN ACUMULADA")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(10)
                .SetFontColor(_colorPrimario));

            lluviaCell.Add(new Paragraph($"{reporte.LluviaAcumuladaTotal:N1} mm")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14)
                .SetFontColor(_colorAcento)
                .SetMarginTop(5));

            climaTable.AddCell(lluviaCell);

            // Eventos climáticos
            var eventosCell = new Cell()
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15)
                .SetBorder(new SolidBorder(_colorBorde, 1))
                .SetTextAlignment(TextAlignment.CENTER);

            eventosCell.Add(new Paragraph("EVENTOS CLIMÁTICOS")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(10)
                .SetFontColor(_colorPrimario));

            eventosCell.Add(new Paragraph($"{eventosCount} registros")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14)
                .SetFontColor(_colorAcento)
                .SetMarginTop(5));

            climaTable.AddCell(eventosCell);

            document.Add(climaTable);
        }

        private void AgregarFooterProfesional(Document document)
        {
            var footer = new Paragraph($"Documento confidencial - Generado por el Sistema de Gestión AgroLab el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(8)
                .SetFontColor(_colorSecundario)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30)
                .SetPaddingTop(10)
                .SetBorderTop(new SolidBorder(_colorBorde, 0.5f));
            document.Add(footer);
        }

        #region Métodos Auxiliares Profesionales
        private Paragraph CrearTituloSeccionProfesional(string titulo)
        {
            return new Paragraph(titulo)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(_colorPrimario)
                .SetMarginTop(20)
                .SetMarginBottom(12)
                .SetBorderBottom(new SolidBorder(_colorPrimario, 1))
                .SetPaddingBottom(4);
        }

        private Cell CrearCeldaResumen(string valor, string label)
        {
            return new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(11)
                    .SetFontColor(_colorAcento))
                .Add(new Paragraph(label)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8)
                    .SetFontColor(_colorSecundario)
                    .SetMarginTop(2));
        }

        private Cell CrearTarjetaMetricaProfesional(string titulo, string valor)
        {
            return new Cell()
                .SetBackgroundColor(ColorConstants.WHITE)
                .SetPadding(15)
                .SetBorder(new SolidBorder(_colorBorde, 1))
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(12)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(4))
                .Add(new Paragraph(titulo)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9)
                    .SetFontColor(_colorSecundario)
                    .SetTextAlignment(TextAlignment.CENTER));
        }

        private Cell CrearCeldaHeaderProfesional(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(_colorPrimario)
                .SetPadding(8)
                .SetTextAlignment(TextAlignment.LEFT);
        }

        private Cell CrearCeldaDatosProfesional(string texto, TextAlignment alignment = TextAlignment.LEFT)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8))
                .SetPadding(6)
                .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
                .SetTextAlignment(alignment);
        }

        private Cell CrearCeldaTotalProfesional(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(232, 244, 248))
                .SetPadding(8)
                .SetBorderBottom(new SolidBorder(_colorPrimario, 1))
                .SetTextAlignment(TextAlignment.LEFT);
        }

        #endregion
    }

    public class LluviaMes
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Lluvia { get; set; }
    }

    public class GastoCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal CostoArs { get; set; }
        public decimal CostoUsd { get; set; }
    }
}