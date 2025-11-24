using AgroForm.Business.Contracts;
using AgroForm.Model;
using AlbaServicios.Services;
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
        private readonly Color _colorPrimario = new DeviceRgb(46, 139, 87); // Verde agrícola
        private readonly Color _colorSecundario = new DeviceRgb(52, 152, 219); // Azul
        private readonly Color _colorAcento = new DeviceRgb(241, 196, 15); // Amarillo
        private readonly Color _colorTexto = new DeviceRgb(44, 62, 80); // Gris oscuro
        private readonly Color _colorFondo = new DeviceRgb(248, 249, 250); // Gris claro

        public async Task<OperationResult<byte[]>> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                // Configurar márgenes más amplios para diseño moderno
                document.SetMargins(30, 30, 60, 30);

                // Agregar header con diseño moderno
                await AgregarHeaderModerno(document, pdf, reporte);

                // Sección de métricas principales
                AgregarMetricasPrincipales(document, reporte);

                // Sección de distribución de costos
                AgregarDistribucionCostos(document, reporte);

                // Sección por cultivo
                AgregarDatosPorCultivoModerno(document, reporte);

                // Sección por campo y lote
                AgregarDatosPorCampoYlote(document, reporte);

                // Sección climática mejorada
                AgregarDatosClimaticosModerno(document, reporte);

                // Sección de gastos
                AgregarDatosGastos(document, reporte);

                // Footer moderno
                AgregarFooterModerno(document);

                document.Close();

                return OperationResult<byte[]>.SuccessResult(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                return OperationResult<byte[]>.Failure(e.Message);
            }
        }

        private async Task AgregarHeaderModerno(Document document, PdfDocument pdf, ReporteCierreCampania reporte)
        {
            // Header con gradiente simulado
            var headerTable = new Table(1)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            var headerCell = new Cell()
                .SetBackgroundColor(_colorPrimario)
                .SetPadding(25)
                .SetBorderRadius(new BorderRadius(10))
                .SetTextAlignment(TextAlignment.CENTER);

            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var subtitleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título principal
            var mainTitle = new Paragraph("REPORTE DE CIERRE DE CAMPAÑA")
                .SetFont(titleFont)
                .SetFontSize(20)
                .SetFontColor(ColorConstants.WHITE)
                .SetMarginBottom(5);
            headerCell.Add(mainTitle);

            // Nombre de la campaña
            var campaignName = new Paragraph(reporte.NombreCampania)
                .SetFont(subtitleFont)
                .SetFontSize(16)
                .SetFontColor(new DeviceRgb(220, 237, 200))
                .SetMarginBottom(8);
            headerCell.Add(campaignName);

            // Período
            var period = new Paragraph($"{reporte.FechaInicio:dd/MM/yyyy} - {reporte.FechaFin:dd/MM/yyyy}")
                .SetFont(subtitleFont)
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(200, 230, 201));
            headerCell.Add(period);

            headerTable.AddCell(headerCell);
            document.Add(headerTable);
        }

        private void AgregarMetricasPrincipales(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccion("📊 MÉTRICAS PRINCIPALES");
            document.Add(sectionTitle);

            var metricsTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(25);

            // Tarjetas de métricas
            metricsTable.AddCell(CrearTarjetaMetrica("Superficie Total", $"{reporte.SuperficieTotalHa:N1} ha", "🌾"));
            metricsTable.AddCell(CrearTarjetaMetrica("Producción Total", $"{reporte.ToneladasProducidas:N0} Tn", "📦"));
            metricsTable.AddCell(CrearTarjetaMetrica("Rendimiento", $"{reporte.RendimientoPromedioHa:N1} Tn/ha", "📈"));
            metricsTable.AddCell(CrearTarjetaMetrica("Costo Total", $"${reporte.CostoTotalArs:N0}", "💰"));

            document.Add(metricsTable);

            // Métricas secundarias
            var secondaryMetricsTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            secondaryMetricsTable.AddCell(CrearTarjetaMetricaSecundaria("Costo por Hectárea", $"${reporte.CostoPorHa:N0}"));
            secondaryMetricsTable.AddCell(CrearTarjetaMetricaSecundaria("Costo por Tonelada", $"${reporte.CostoPorTonelada:N0}"));

            document.Add(secondaryMetricsTable);
        }

        private void AgregarDistribucionCostos(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccion("💰 DISTRIBUCIÓN DE COSTOS");
            document.Add(sectionTitle);

            var costosTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Header de la tabla
            costosTable.AddCell(CrearCeldaHeader("CONCEPTO"));
            costosTable.AddCell(CrearCeldaHeader("COSTO ARS"));
            costosTable.AddCell(CrearCeldaHeader("COSTO USD"));
            costosTable.AddCell(CrearCeldaHeader("% TOTAL"));

            // Datos de costos
            var costos = new[]
            {
                new { Nombre = "Cosechas", Ars = reporte.CostoCosechasArs, Usd = reporte.CostoCosechasUsd },
                new { Nombre = "Siembras", Ars = reporte.CostoSiembrasArs, Usd = reporte.CostoSiembrasUsd },
                new { Nombre = "Fertilizantes", Ars = reporte.CostoFertilizantesArs, Usd = reporte.CostoFertilizantesUsd },
                new { Nombre = "Riegos", Ars = reporte.CostoRiegosArs, Usd = reporte.CostoRiegosUsd },
                new { Nombre = "Pulverizaciones", Ars = reporte.CostoPulverizacionesArs, Usd = reporte.CostoPulverizacionesUsd },
                new { Nombre = "Análisis Suelo", Ars = reporte.AnalisisSueloArs, Usd = reporte.AnalisisSueloUsd },
                new { Nombre = "Monitoreos", Ars = reporte.CostoMonitoreosArs, Usd = reporte.CostoMonitoreosUsd },
                new { Nombre = "Otras Labores", Ars = reporte.CostoOtrasLaboresArs, Usd = reporte.CostoOtrasLaboresUsd }
            };

            foreach (var costo in costos)
            {
                if (costo.Ars > 0)
                {
                    var porcentaje = reporte.CostoTotalArs > 0 ? (costo.Ars / reporte.CostoTotalArs) * 100 : 0;

                    costosTable.AddCell(CrearCeldaDatos(costo.Nombre));
                    costosTable.AddCell(CrearCeldaDatos($"${costo.Ars:N0}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatos($"${costo.Usd:N2}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatos($"{porcentaje:N1}%", TextAlignment.RIGHT));
                }
            }

            // Total
            costosTable.AddCell(CrearCeldaTotal("TOTAL"));
            costosTable.AddCell(CrearCeldaTotal($"${reporte.CostoTotalArs:N0}"));
            costosTable.AddCell(CrearCeldaTotal($"${reporte.CostoTotalUsd:N2}"));
            costosTable.AddCell(CrearCeldaTotal("100%"));

            document.Add(costosTable);
        }

        private void AgregarDatosPorCultivoModerno(Document document, ReporteCierreCampania reporte)
        {
            if (string.IsNullOrEmpty(reporte.ResumenPorCultivoJson)) return;

            var sectionTitle = CrearTituloSeccion("🌿 DATOS POR CULTIVO");
            document.Add(sectionTitle);

            var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);

            foreach (var cultivo in cultivos)
            {
                var cultivoCard = new Table(1)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(15)
                    .SetBackgroundColor(_colorFondo)
                    .SetBorderRadius(new BorderRadius(8))
                    .SetPadding(15);

                // Header del cultivo
                var headerCell = new Cell()
                    .Add(new Paragraph(cultivo.NombreCultivo)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                        .SetFontSize(14)
                        .SetFontColor(_colorPrimario))
                    .SetBorderBottom(new SolidBorder(_colorPrimario, 1))
                    .SetPaddingBottom(8)
                    .SetMarginBottom(10);
                cultivoCard.AddCell(headerCell);

                // Métricas del cultivo
                var metricsTable = new Table(4)
                    .SetWidth(UnitValue.CreatePercentValue(100));

                metricsTable.AddCell(CrearCeldaCultivo("Superficie", $"{cultivo.SuperficieHa:N1} ha"));
                metricsTable.AddCell(CrearCeldaCultivo("Producción", $"{cultivo.ToneladasProducidas:N1} Tn"));
                metricsTable.AddCell(CrearCeldaCultivo("Rendimiento", $"{cultivo.RendimientoHa:N1} Tn/ha"));
                metricsTable.AddCell(CrearCeldaCultivo("Costo Total", $"${cultivo.CostoTotal:N0}"));

                cultivoCard.AddCell(new Cell().Add(metricsTable).SetBorder(null));
                document.Add(cultivoCard);
            }
        }

        private void AgregarDatosPorCampoYlote(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccion("🏞️ DATOS POR CAMPO Y LOTE");
            document.Add(sectionTitle);

            if (!string.IsNullOrEmpty(reporte.ResumenPorCampoJson))
            {
                var campos = JsonSerializer.Deserialize<List<ResumenCampo>>(reporte.ResumenPorCampoJson);

                foreach (var campo in campos)
                {
                    var campoCard = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(15)
                        .SetBackgroundColor(new DeviceRgb(235, 245, 251))
                        .SetBorderRadius(new BorderRadius(8))
                        .SetPadding(15);

                    // Header del campo
                    var headerCell = new Cell()
                        .Add(new Paragraph($"Campo: {campo.NombreCampo}")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(12)
                            .SetFontColor(_colorSecundario))
                        .SetBorderBottom(new SolidBorder(_colorSecundario, 1))
                        .SetPaddingBottom(8);
                    campoCard.AddCell(headerCell);

                    // Métricas del campo
                    var metricsTable = new Table(3)
                        .SetWidth(UnitValue.CreatePercentValue(100));

                    metricsTable.AddCell(CrearCeldaCampo("Superficie", $"{campo.SuperficieHa:N1} ha"));
                    metricsTable.AddCell(CrearCeldaCampo("Producción", $"{campo.ToneladasProducidas:N1} Tn"));
                    metricsTable.AddCell(CrearCeldaCampo("Costo Total", $"${campo.CostoTotalArs:N0}"));
                    metricsTable.AddCell(CrearCeldaCampo("Costo Total", $"US${campo.CostoTotalUsd:N0}"));

                    campoCard.AddCell(new Cell().Add(metricsTable).SetBorder(null));

                    // Lotes del campo
                    if (campo.Lotes.Any())
                    {
                        var lotesTitle = new Paragraph("Lotes:")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(10)
                            .SetMarginTop(10)
                            .SetMarginBottom(5);
                        campoCard.AddCell(new Cell().Add(lotesTitle).SetBorder(null));

                        foreach (var lote in campo.Lotes)
                        {
                            var loteInfo = new Paragraph($"• {lote.NombreLote} - {lote.Cultivo}: {lote.SuperficieHa:N1} ha, {lote.ToneladasProducidas:N1} Tn, ${lote.CostoTotalArs:N0} (US${lote.CostoTotalUsd:N2})")
                                .SetFontSize(9)
                                .SetMarginBottom(2);
                            campoCard.AddCell(new Cell().Add(loteInfo).SetBorder(null));
                        }
                    }

                    document.Add(campoCard);
                }
            }
            else
            {
                var mensaje = new Paragraph("Los datos detallados por campo y lote están disponibles en el sistema web.")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(20);
                document.Add(mensaje);
            }
        }

        private void AgregarDatosClimaticosModerno(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccion("🌤️ DATOS CLIMÁTICOS");
            document.Add(sectionTitle);

            var climaTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Lluvia acumulada
            var lluviaCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(232, 245, 233))
                .SetPadding(15)
                .SetBorderRadius(new BorderRadius(8))
                .SetTextAlignment(TextAlignment.CENTER);

            lluviaCell.Add(new Paragraph("🌧️ Lluvia Acumulada")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(_colorPrimario));

            lluviaCell.Add(new Paragraph($"{reporte.LluviaAcumuladaTotal:N1} mm")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(16)
                .SetFontColor(_colorTexto)
                .SetMarginTop(5));

            climaTable.AddCell(lluviaCell);

            // Eventos extremos
            var eventosCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(255, 243, 224))
                .SetPadding(15)
                .SetBorderRadius(new BorderRadius(8))
                .SetTextAlignment(TextAlignment.CENTER);

            int eventosCount = 0;
            if (!string.IsNullOrEmpty(reporte.EventosExtremosJson))
            {
                var eventos = JsonSerializer.Deserialize<List<dynamic>>(reporte.EventosExtremosJson);
                eventosCount = eventos?.Count ?? 0;
            }

            eventosCell.Add(new Paragraph("⚡ Eventos Extremos")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(_colorAcento));

            eventosCell.Add(new Paragraph($"{eventosCount} registros")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(16)
                .SetFontColor(_colorTexto)
                .SetMarginTop(5));

            climaTable.AddCell(eventosCell);

            document.Add(climaTable);

            // Gráfico de lluvias por mes (simulado con tabla)
            if (!string.IsNullOrEmpty(reporte.LluviasPorMesJson))
            {
                AgregarGraficoLluvias(document, reporte);
            }
        }

        private void AgregarDatosGastos(Document document, ReporteCierreCampania reporte)
        {
            var sectionTitle = CrearTituloSeccion("💳 GASTOS ADICIONALES");
            document.Add(sectionTitle);

            var gastosTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            gastosTable.AddCell(CrearCeldaHeader("CATEGORÍA"));
            gastosTable.AddCell(CrearCeldaHeader("COSTO ARS"));
            gastosTable.AddCell(CrearCeldaHeader("COSTO USD"));

            if (!string.IsNullOrEmpty(reporte.GastosPorCategoriaJson))
            {
                var gastos = JsonSerializer.Deserialize<List<GastoCategoria>>(reporte.GastosPorCategoriaJson);

                foreach (var gasto in gastos)
                {
                    gastosTable.AddCell(CrearCeldaDatos(gasto.Categoria));
                    gastosTable.AddCell(CrearCeldaDatos($"${gasto.CostoArs:N0}", TextAlignment.RIGHT));
                    gastosTable.AddCell(CrearCeldaDatos($"${gasto.CostoUsd:N2}", TextAlignment.RIGHT));
                }
            }

            // Total de gastos
            gastosTable.AddCell(CrearCeldaTotal("TOTAL GASTOS"));
            gastosTable.AddCell(CrearCeldaTotal($"${reporte.GastosTotalesArs:N0}"));
            gastosTable.AddCell(CrearCeldaTotal($"${reporte.GastosTotalesUsd:N2}"));

            document.Add(gastosTable);
        }

        private void AgregarGraficoLluvias(Document document, ReporteCierreCampania reporte)
        {
            var lluviasPorMes = JsonSerializer.Deserialize<List<LluviaMes>>(reporte.LluviasPorMesJson);
            var maxLluvia = lluviasPorMes.Max(l => l.Lluvia);

            var graficoTitle = new Paragraph("Distribución Mensual de Lluvias")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(11)
                .SetFontColor(_colorTexto)
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(graficoTitle);

            foreach (var lluvia in lluviasPorMes)
            {
                var porcentaje = maxLluvia > 0 ? (lluvia.Lluvia / maxLluvia) * 100 : 0;
                var barraLength = (int)(porcentaje / 2); // Cada █ representa 2%

                var barraRow = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(3);

                barraRow.AddCell(CrearCeldaGrafico(lluvia.Mes, TextAlignment.LEFT));
                barraRow.AddCell(CrearCeldaGrafico(new string('█', Math.Max(1, barraLength)), TextAlignment.LEFT));
                barraRow.AddCell(CrearCeldaGrafico($"{lluvia.Lluvia} mm", TextAlignment.RIGHT));

                document.Add(barraRow);
            }
        }

        private void AgregarFooterModerno(Document document)
        {
            var footer = new Paragraph($"AgroForm - Reporte generado el {DateTime.Now:dd/MM/yyyy 'a las' HH:mm}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(9)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30)
                .SetPaddingTop(10)
                .SetBorderTop(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f));
            document.Add(footer);
        }

        #region Métodos Auxiliares Mejorados
        private Paragraph CrearTituloSeccion(string titulo)
        {
            return new Paragraph(titulo)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(16)
                .SetFontColor(_colorTexto)
                .SetMarginTop(25)
                .SetMarginBottom(15)
                .SetBorderBottom(new SolidBorder(_colorPrimario, 2))
                .SetPaddingBottom(5);
        }

        private Cell CrearTarjetaMetrica(string titulo, string valor, string emoji)
        {
            return new Cell()
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15)
                .SetBorderRadius(new BorderRadius(8))
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph(emoji)
                    .SetFontSize(16)
                    .SetMarginBottom(5))
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(14)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(3))
                .Add(new Paragraph(titulo)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(10)
                    .SetFontColor(_colorTexto));
        }

        private Cell CrearTarjetaMetricaSecundaria(string titulo, string valor)
        {
            return new Cell()
                .SetBackgroundColor(new DeviceRgb(240, 248, 255))
                .SetPadding(12)
                .SetBorderRadius(new BorderRadius(6))
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(12)
                    .SetFontColor(_colorSecundario)
                    .SetMarginBottom(2))
                .Add(new Paragraph(titulo)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9)
                    .SetFontColor(_colorTexto));
        }

        private Cell CrearCeldaHeader(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(_colorPrimario)
                .SetPadding(10)
                .SetTextAlignment(TextAlignment.CENTER);
        }

        private Cell CrearCeldaDatos(string texto, TextAlignment alignment = TextAlignment.LEFT)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(9))
                .SetPadding(8)
                .SetBorderBottom(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f))
                .SetTextAlignment(alignment);
        }

        private Cell CrearCeldaTotal(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetPadding(10)
                .SetBorderBottom(new SolidBorder(_colorTexto, 1))
                .SetTextAlignment(TextAlignment.CENTER);
        }

        private Cell CrearCeldaCultivo(string label, string valor)
        {
            return new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .Add(new Paragraph(label)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.GRAY))
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10)
                    .SetFontColor(_colorTexto));
        }

        private Cell CrearCeldaCampo(string label, string valor)
        {
            return new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .Add(new Paragraph(label)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.GRAY))
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9)
                    .SetFontColor(_colorSecundario));
        }

        private Cell CrearCeldaGrafico(string texto, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8))
                .SetPadding(3)
                .SetBorder(null)
                .SetTextAlignment(alignment);
        }
        #endregion
    }

    // Clases auxiliares para deserialización
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