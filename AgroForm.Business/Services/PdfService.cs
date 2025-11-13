using AgroForm.Business.Contracts;
using AgroForm.Model;
using iText.IO.Font.Constants;
using iText.IO.Image;
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
        private readonly IChartGeneratorService _chartGenerator;

        public PdfService(IChartGeneratorService chartGenerator)
        {
            _chartGenerator = chartGenerator;
        }

        public async Task<byte[]> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);

            // Configurar márgenes
            document.SetMargins(40, 40, 80, 40);

            // Agregar header con logo y nombre
            await AgregarHeader(document, pdf);

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
            AgregarFooter(document, pdf);

            document.Close();
            return memoryStream.ToArray();
        }

        private async Task AgregarHeader(Document document, PdfDocument pdf)
        {
            try
            {
                // Intentar cargar logo
                var logoPath = System.IO.Path.Combine("images", "logo.png");
                if (File.Exists(logoPath))
                {
                    var logoData = await File.ReadAllBytesAsync(logoPath);
                    var logo = new Image(ImageDataFactory.Create(logoData))
                        .SetWidth(60)
                        .SetHeight(60)
                        .SetFixedPosition(40, document.GetPdfDocument().GetDefaultPageSize().GetTop() - 80);
                    document.Add(logo);
                }
            }
            catch
            {
                // Si no hay logo, continuar sin él
            }

            // Nombre de la empresa
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var subtitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var empresaTitle = new Paragraph("AgroLab")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetFontColor(ColorConstants.GREEN)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(5);
            document.Add(empresaTitle);

            var reportTitle = new Paragraph("REPORTE DE CIERRE DE CAMPAÑA")
                .SetFont(subtitleFont)
                .SetFontSize(12)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(15);
            document.Add(reportTitle);

            // Línea decorativa
            var line = new LineSeparator(new SolidLine())
                .SetStrokeColor(ColorConstants.GREEN)
                .SetStrokeWidth(1);
            document.Add(line);
        }

        private void AgregarDatosGenerales(Document document, ReporteCierreCampania reporte)
        {
            var sectionFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var sectionTitle = new Paragraph("📊 DATOS GENERALES")
                .SetFont(sectionFont)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(15);
            document.Add(sectionTitle);

            // Tabla principal de datos generales
            var mainTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10)
                .SetMarginBottom(15);

            // Columna izquierda - Información básica
            var infoTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100));

            AgregarFilaInfo(infoTable, "Fecha Inicio:", reporte.FechaInicio.ToString("MM/yyyy"));
            AgregarFilaInfo(infoTable, "Fecha Fin:", reporte.FechaFin.ToString("MM/yyyy"));
            AgregarFilaInfo(infoTable, "Superficie Total:", $"{reporte.SuperficieTotalHa:N2} Ha");
            AgregarFilaInfo(infoTable, "Producción Total:", $"{reporte.ToneladasProducidas:N2} Ton");
            AgregarFilaInfo(infoTable, "Rendimiento Promedio:", $"{reporte.RendimientoPromedioHa:N2} Ton/Ha");

            // Columna derecha - Costos
            var costTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100));

            AgregarFilaCosto(costTable, "Costo Total:", reporte.CostoTotalArs, reporte.CostoTotalUsd);
            //AgregarFilaCosto(costTable, "Costo por Ha:", reporte.CostoPorHa, reporte.CostoPorHa / (reporte.TipoCambio ?? 1));
            AgregarFilaCosto(costTable, "Costo por Ha:", reporte.CostoPorHa, -99);
            //AgregarFilaCosto(costTable, "Costo por Ton:", reporte.CostoPorTonelada, reporte.CostoPorTonelada / (reporte.TipoCambio ?? 1));
            AgregarFilaCosto(costTable, "Costo por Ton:", reporte.CostoPorTonelada, -99);

            var leftCell = new Cell().Add(infoTable).SetBorder(null).SetPadding(5);
            var rightCell = new Cell().Add(costTable).SetBorder(null).SetPadding(5);

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
            var sectionFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var sectionTitle = new Paragraph("📈 GRÁFICOS Y ESTADÍSTICAS")
                .SetFont(sectionFont)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(15);
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
            var title = new Paragraph("🌱 Distribución de Cultivos")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(20)
                .SetMarginBottom(10)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
            {
                var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);

                // Generar gráfico de torta
                var chartData = cultivos.ToDictionary(c => c.NombreCultivo, c => c.SuperficieHa);
                var chartImage = await _chartGenerator.GenerarGraficoTortaAsync(chartData, "Distribución de Cultivos");

                if (chartImage != null)
                {
                    var image = new Image(ImageDataFactory.Create(chartImage))
                        .SetMaxWidth(400)
                        .SetMaxHeight(250)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);
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
            var title = new Paragraph("📦 Producción por Cultivo")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(10)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
            {
                var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
                var maxProduccion = cultivos.Max(c => c.ToneladasProducidas);

                var barChartTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(15);

                foreach (var cultivo in cultivos)
                {
                    var porcentaje = maxProduccion > 0 ? (cultivo.ToneladasProducidas / maxProduccion) * 100 : 0;
                    var barras = (int)(porcentaje / 5); // Cada █ representa 5%
                    var barra = new string('█', barras);

                    barChartTable.AddCell(CrearCelda($"{cultivo.NombreCultivo}:", TextAlignment.LEFT));
                    barChartTable.AddCell(CrearCelda($"{barra} {cultivo.ToneladasProducidas:N1} Ton", TextAlignment.RIGHT));
                }

                document.Add(barChartTable);
            }
        }

        private void AgregarGraficoBarrasCostos(Document document, ReporteCierreCampania reporte)
        {
            var title = new Paragraph("💰 Distribución de Costos")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(10)
                .SetTextAlignment(TextAlignment.CENTER);
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
            var costoTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(15);

            foreach (var costo in costos)
            {
                if (costo.Valor > 0)
                {
                    var porcentaje = maxCosto > 0 ? (costo.Valor / maxCosto) * 100 : 0;
                    var barras = (int)(porcentaje / 3); // Cada █ representa 3%
                    var barra = new string('█', Math.Max(1, barras));

                    costoTable.AddCell(CrearCelda($"{costo.Nombre}:", TextAlignment.LEFT));
                    costoTable.AddCell(CrearCelda($"{barra} ${costo.Valor:N0}", TextAlignment.RIGHT));
                }
            }

            document.Add(costoTable);
        }

        private void AgregarGraficoLineasLluvias(Document document, ReporteCierreCampania reporte)
        {
            var title = new Paragraph("🌧️ Lluvias por Mes")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(10)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            if (!string.IsNullOrEmpty(reporte.LluviasPorMesJson))
            {
                var lluviasPorMes = JsonSerializer.Deserialize<List<LluviaMes>>(reporte.LluviasPorMesJson);
                var maxLluvia = lluviasPorMes.Max(l => l.Lluvia);

                var chartTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(15);

                foreach (var lluvia in lluviasPorMes)
                {
                    var porcentaje = maxLluvia > 0 ? (lluvia.Lluvia / maxLluvia) * 100 : 0;
                    var puntos = (int)(porcentaje / 10); // Cada ● representa 10%
                    var linea = new string('●', Math.Max(1, puntos));

                    chartTable.AddCell(CrearCelda($"{lluvia.Mes}:", TextAlignment.LEFT));
                    chartTable.AddCell(CrearCelda($"{linea} {lluvia.Lluvia} mm", TextAlignment.RIGHT));
                }

                document.Add(chartTable);
            }
        }

        private void AgregarDatosPorCultivo(Document document, ReporteCierreCampania reporte)
        {
            var sectionFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var sectionTitle = new Paragraph("🌿 DATOS POR CULTIVO")
                .SetFont(sectionFont)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(15);
            document.Add(sectionTitle);

            if (!string.IsNullOrEmpty(reporte.ResumenPorCultivoJson))
            {
                var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);

                foreach (var cultivo in cultivos)
                {
                    var cultivoTable = new Table(4)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(10)
                        .SetMarginBottom(10);

                    // Header del cultivo
                    var headerCell = new Cell(1, 4)
                        .Add(new Paragraph(cultivo.NombreCultivo)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(12)
                            .SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                        .SetTextAlignment(TextAlignment.CENTER);
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
            var sectionFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var sectionTitle = new Paragraph("🏞️ DATOS POR CAMPO")
                .SetFont(sectionFont)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(15);
            document.Add(sectionTitle);

            var infoCampo = new Paragraph("Los datos detallados por campo y lote están disponibles en el sistema web.")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(15);
            document.Add(infoCampo);
        }

        private void AgregarDatosClimaticos(Document document, ReporteCierreCampania reporte)
        {
            var sectionFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var sectionTitle = new Paragraph("🌤️ DATOS CLIMÁTICOS")
                .SetFont(sectionFont)
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(25)
                .SetMarginBottom(15);
            document.Add(sectionTitle);

            var climaTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(10);

            climaTable.AddCell(CrearCelda($"Lluvia Acumulada Total:\n{reporte.LluviaAcumuladaTotal:N1} mm", TextAlignment.CENTER));

            if (!string.IsNullOrEmpty(reporte.EventosExtremosJson))
            {
                var eventos = JsonSerializer.Deserialize<List<dynamic>>(reporte.EventosExtremosJson);
                climaTable.AddCell(CrearCelda($"Eventos Extremos:\n{eventos.Count} registros", TextAlignment.CENTER));
            }
            else
            {
                climaTable.AddCell(CrearCelda("Eventos Extremos:\nSin registros", TextAlignment.CENTER));
            }

            document.Add(climaTable);
        }

        private void AgregarTablaCostosDetallados(Document document, ReporteCierreCampania reporte)
        {
            var subtitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var subtitle = new Paragraph("Desglose de Costos")
                .SetFont(subtitleFont)
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(subtitle);

            var costosTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(15);

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
            costosTable.AddCell(CrearCelda("TOTAL", TextAlignment.RIGHT, true));
            costosTable.AddCell(CrearCelda($"${reporte.CostoTotalArs:N2}", TextAlignment.RIGHT, true));
            costosTable.AddCell(CrearCelda($"${reporte.CostoTotalUsd:N2}", TextAlignment.RIGHT, true));

            document.Add(costosTable);
        }

        private void AgregarTablaResumenLabores(Document document, ReporteCierreCampania reporte)
        {
            var subtitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var subtitle = new Paragraph("Resumen de Labores")
                .SetFont(subtitleFont)
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(44, 62, 80))
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(subtitle);

            var laboresTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(15);

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
            var distribucionTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(80))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetMarginBottom(15);

            distribucionTable.AddCell(CrearCeldaHeader("Cultivo"));
            distribucionTable.AddCell(CrearCeldaHeader("Superficie (Ha)"));
            distribucionTable.AddCell(CrearCeldaHeader("Porcentaje"));

            var totalSuperficie = cultivos.Sum(c => c.SuperficieHa);

            foreach (var cultivo in cultivos)
            {
                var porcentaje = totalSuperficie > 0 ? (cultivo.SuperficieHa / totalSuperficie) * 100 : 0;

                distribucionTable.AddCell(CrearCelda(cultivo.NombreCultivo));
                distribucionTable.AddCell(CrearCelda(cultivo.SuperficieHa.ToString("N2"), TextAlignment.RIGHT));
                distribucionTable.AddCell(CrearCelda($"{porcentaje:N1}%", TextAlignment.RIGHT));
            }

            document.Add(distribucionTable);
        }

        private void AgregarFooter(Document document, PdfDocument pdf)
        {
            var footer = new Paragraph($"AgroLab - Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(8)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(20);
            document.Add(footer);
        }

        #region Métodos auxiliares
        private void AgregarFilaInfo(Table table, string label, string value)
        {
            table.AddCell(CrearCelda(label, TextAlignment.LEFT, true));
            table.AddCell(CrearCelda(value));
        }

        private void AgregarFilaCosto(Table table, string label, decimal ars, decimal usd)
        {
            table.AddCell(CrearCelda(label, TextAlignment.LEFT, true));
            table.AddCell(CrearCelda($"${ars:N2} (${usd:N2})"));
        }

        private void AgregarFilaCostoDetallado(Table table, string concepto, decimal ars, decimal usd)
        {
            table.AddCell(CrearCelda(concepto));
            table.AddCell(CrearCelda($"${ars:N2}", TextAlignment.RIGHT));
            table.AddCell(CrearCelda($"${usd:N2}", TextAlignment.RIGHT));
        }

        private Cell CrearCelda(string texto, TextAlignment alignment = TextAlignment.LEFT, bool bold = false)
        {
            var font = bold ?
                PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD) :
                PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            return new Cell()
                .Add(new Paragraph(texto).SetFont(font).SetFontSize(10))
                .SetPadding(8)
                .SetBorderBottom(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f))
                .SetTextAlignment(alignment);
        }

        private Cell CrearCeldaCentrada(string texto)
        {
            return CrearCelda(texto, TextAlignment.CENTER);
        }

        private Cell CrearCeldaHeader(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(44, 62, 80))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8);
        }
        #endregion
    }

    public class LluviaMes
    {
        public string Mes { get; set; }
        public decimal Lluvia { get; set; }
    }
}