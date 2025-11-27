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
        private readonly Color _colorPrimario = new DeviceRgb(44, 62, 80);    // Gris oscuro 
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

                // Configurar márgenes es
                document.SetMargins(20, 25, 20, 30);

                // Agregar secciones
                AgregarHeader(document, reporte);
                AgregarIndicadoresPrincipales(document, reporte);

                AgregarDatosPorCultivo(document, reporte);
                AgregarDistribucionCostos(document, reporte);

                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                AgregarDistribucionGastos(document, reporte);
                AgregarDatosPorCampoYLotes(document, reporte);

                AgregarFooter(document);

                document.Close();

                return OperationResult<byte[]>.SuccessResult(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                return OperationResult<byte[]>.Failure($"Error al generar PDF: {e.Message}");
            }
        }

        private void AgregarHeader(Document document, ReporteCierreCampania reporte)
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
                .SetMarginBottom(0);
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
            var sectionTitle = CrearTituloSeccion("INDICADORES");
            document.Add(sectionTitle);

            var indicadoresTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(10)
                .SetBackgroundColor(_colorFondo)
                .SetPadding(10);

            // Métricas operativas
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.SuperficieTotalHa:N1} ha", "Superficie Total"));
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.ToneladasProducidas:N0} Tn", "Producción Total"));
            indicadoresTable.AddCell(CrearCeldaResumen($"{reporte.RendimientoPromedioHa:N1} Tn/ha", "Rendimiento Promedio"));

            document.Add(indicadoresTable);

            // Segunda fila - Costos por unidad
            var costosTable = new Table(3)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0)
                .SetBackgroundColor(_colorFondo)
                .SetPadding(15);

            // Inversión Total con ambas monedas
            var inversionContent = new Div()
                .Add(new Paragraph($"${reporte.CostoTotalArs:N0}")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(11)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"US${reporte.CostoTotalUsd:N0}")
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

            costosTable.AddCell(inversionCell);

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
            var sectionTitle = CrearTituloSeccion("DISTRIBUCIÓN DE COSTOS OPERATIVOS");
            document.Add(sectionTitle);

            var costosTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(5);

            // Header de la tabla
            costosTable.AddCell(CrearCeldaHeader("LABORES"));
            costosTable.AddCell(CrearCeldaHeader("INVERSIÓN ARS"));
            costosTable.AddCell(CrearCeldaHeader("INVERSIÓN USD"));
            costosTable.AddCell(CrearCeldaHeader("PARTICIPACIÓN"));

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

                    costosTable.AddCell(CrearCeldaDatos(costo.Nombre));
                    costosTable.AddCell(CrearCeldaDatos($"${costo.Ars:N0}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatos($"${costo.Usd:N2}", TextAlignment.RIGHT));
                    costosTable.AddCell(CrearCeldaDatos($"{porcentaje:N1}%", TextAlignment.RIGHT));
                }
            }

            // Total
            costosTable.AddCell(CrearCeldaTotal("TOTAL GENERAL"));
            costosTable.AddCell(CrearCeldaTotal($"${reporte.CostoTotalArs:N0}", TextAlignment.RIGHT));
            costosTable.AddCell(CrearCeldaTotal($"${reporte.CostoTotalUsd:N2}", TextAlignment.RIGHT));
            costosTable.AddCell(CrearCeldaTotal("100%", TextAlignment.RIGHT));

            document.Add(costosTable);
        }

        private void AgregarDatosPorCultivo(Document document, ReporteCierreCampania reporte)
        {
            try
            {
                var cultivos = JsonSerializer.Deserialize<List<ResumenCultivo>>(reporte.ResumenPorCultivoJson);
                if (cultivos == null || !cultivos.Any()) return;

                var sectionTitle = CrearTituloSeccion("ANÁLISIS POR CULTIVO");
                document.Add(sectionTitle);

                // Crear tabla principal con 3 columnas (3 cultivos por fila)
                var mainTable = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);

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

        private void AgregarDatosPorCampoYLotes(Document document, ReporteCierreCampania reporte)
        {
            try
            {
                var campos = JsonSerializer.Deserialize<List<ResumenCampo>>(reporte.ResumenPorCampoJson);
                if (campos == null || !campos.Any()) return;

                // Cargar datos climáticos
                var lluviasPorCampo = ObtenerLluviasPorCampo(reporte);

                var sectionTitle = CrearTituloSeccion("ANÁLISIS POR CAMPOS Y LOTES");
                document.Add(sectionTitle);

                foreach (var campo in campos)
                {
                    // Obtener datos climáticos para este campo
                    var lluviasCampo = lluviasPorCampo.ContainsKey(campo.IdCampo) ? lluviasPorCampo[campo.IdCampo] : new List<ResumenClima>();
                    var promedioMensual = lluviasCampo.Any() ? lluviasCampo.Average(l => l.Lluvia) : 0;

                    // Tarjeta principal del campo
                    var campoCard = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(7)
                        .SetBackgroundColor(_colorFondo)
                        .SetPadding(12)
                        .SetBorder(new SolidBorder(_colorBorde, 1))
                        .SetBorderRadius(new BorderRadius(6));

                    // Header del campo con métricas principales - MODIFICADO
                    var headerTable = new Table(4) // Reducido a 4 columnas
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(0);

                    headerTable.AddCell(CrearHeaderCampo("CAMPO", campo.NombreCampo.ToUpper()));
                    headerTable.AddCell(CrearHeaderCampo("SUPERFICIE", $"{campo.SuperficieHa:N1} ha"));
                    headerTable.AddCell(CrearHeaderCampo("PRODUCCIÓN", $"{campo.ToneladasProducidas:N1} Tn"));

                    // NUEVO: Celda unificada para inversión con diseño mejorado
                    // NUEVO: Celda unificada para inversión
                    // NUEVO: Celda unificada para inversión compacta
                    // NUEVO: Celda unificada para inversión con diseño mejorado
                    var tablaInversion = new Table(3) // 3 columnas: ARS | Separador | USD
                        .SetWidth(UnitValue.CreatePercentValue(80))
                        .SetBorder(Border.NO_BORDER)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    // Número ARS
                    tablaInversion.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(2)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .Add(new Paragraph($"${campo.CostoTotalArs:N0}")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(9)
                            .SetFontColor(_colorPrimario)));

                    // Separador |
                    tablaInversion.AddCell(new Cell()
                        .SetWidth(8)
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(2)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph("|")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                            .SetFontSize(8)
                            .SetFontColor(_colorBorde)));

                    // Número USD
                    tablaInversion.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(2)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .Add(new Paragraph($"US${campo.CostoTotalUsd:N2}")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetFontSize(8)
                            .SetFontColor(_colorPrimario)));

                    // Fila inferior: "INVERSIÓN" que ocupa las 3 columnas
                    var filaInversion = new Cell(1, 3) // Span de 3 columnas
                        .SetBorder(Border.NO_BORDER)
                        .SetPaddingTop(2)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph("INVERSIÓN")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                            .SetFontSize(6)
                            .SetFontColor(_colorSecundario));

                    // Crear tabla contenedora para ambas filas
                    var tablaContenedora = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(Border.NO_BORDER);

                    tablaContenedora.AddCell(new Cell().Add(tablaInversion).SetBorder(Border.NO_BORDER).SetPadding(0));
                    tablaContenedora.AddCell(filaInversion);

                    headerTable.AddCell(new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(5)
                        .Add(tablaContenedora));



                    campoCard.AddCell(new Cell()
                        .Add(headerTable)
                        .SetPaddingBottom(8)
                        .SetBorder(Border.NO_BORDER));

                    // Tabla de lotes (código existente)
                    if (campo.Lotes != null && campo.Lotes.Any())
                    {
                        var tablaLotes = new Table(new float[] { 3, 2, 2, 2, 2, 2 })
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetMarginTop(3);

                        // Header de la tabla de lotes
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("LOTE"));
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("CULTIVO"));
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("SUPERFICIE"));
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("PRODUCCIÓN"));
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("RENDIMIENTO"));
                        tablaLotes.AddHeaderCell(CrearHeaderTablaLotes("INVERSIÓN"));

                        // Filas de lotes
                        foreach (var lote in campo.Lotes)
                        {
                            tablaLotes.AddCell(CrearCeldaLote(lote.NombreLote, TextAlignment.LEFT));
                            tablaLotes.AddCell(CrearCeldaLote(lote.Cultivo ?? "Sin cultivo", TextAlignment.LEFT));
                            tablaLotes.AddCell(CrearCeldaLote($"{lote.SuperficieHa:N1} ha", TextAlignment.RIGHT));
                            tablaLotes.AddCell(CrearCeldaLote($"{lote.ToneladasProducidas:N1} Tn", TextAlignment.RIGHT));
                            tablaLotes.AddCell(CrearCeldaLote($"{lote.RendimientoHa:N1} Tn/ha", TextAlignment.RIGHT));
                            tablaLotes.AddCell(CrearCeldaLote($"${lote.CostoTotalArs:N0}", TextAlignment.RIGHT));
                        }

                        campoCard.AddCell(new Cell().Add(tablaLotes).SetBorder(Border.NO_BORDER));
                    }
                    else
                    {
                        var sinLotes = new Paragraph("No hay lotes registrados")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                            .SetFontSize(8)
                            .SetFontColor(_colorSecundario)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(10);
                        campoCard.AddCell(new Cell().Add(sinLotes).SetBorder(Border.NO_BORDER));
                    }

                    // NUEVO: Tabla horizontal de lluvias mensuales
                    if (lluviasCampo.Any())
                    {
                        AgregarTablaLluviasHorizontal(campoCard, lluviasCampo, promedioMensual);
                    }

                    document.Add(campoCard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AgregarDatosPorCampoYLotes: {ex.Message}");
            }
        }

        private void AgregarTablaLluviasHorizontal(Table campoCard, List<ResumenClima> lluviasCampo, decimal promedioMensual)
        {
            // Espacio antes de la sección climática
            campoCard.AddCell(new Cell()
                .SetHeight(10)
                .SetBorder(Border.NO_BORDER));

            // Título con línea separadora
            var tituloCell = new Cell(1, 2) // Span completo
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetTextAlignment(TextAlignment.CENTER);

            var titulo = new Paragraph("PRECIPITACIÓN MENSUAL")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(8)
                .SetFontColor(_colorSecundario)
                .SetMarginBottom(3);

            tituloCell.Add(titulo);

            //// Línea separadora sutil
            //var lineaSeparadora = new Paragraph()
            //    .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
            //    .SetMarginBottom(5)
            //    .SetHeight(5);

            //tituloCell.Add(lineaSeparadora);

            campoCard.AddCell(tituloCell);

            // Ordenar lluvias por mes
            var lluviasOrdenadas = lluviasCampo.OrderBy(l => l.Mes).ToList();

            // Crear tabla horizontal
            int cantidadMeses = lluviasOrdenadas.Count;
            var tablaLluvias = new Table(cantidadMeses + 1)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(0);

            // Headers: Nombres de los meses
            foreach (var lluvia in lluviasOrdenadas)
            {
                tablaLluvias.AddHeaderCell(CrearHeaderLluviaHorizontal(ObtenerNombreMesCorto(lluvia.Mes)));
            }
            // Header para el promedio (más corto)
            tablaLluvias.AddHeaderCell(CrearHeaderLluviaHorizontal("PROM"));

            // Fila de datos: Valores de lluvia
            foreach (var lluvia in lluviasOrdenadas)
            {
                tablaLluvias.AddCell(CrearCeldaLluviaHorizontal($"{lluvia.Lluvia:N0}"));
            }
            // Celda del promedio
            tablaLluvias.AddCell(CrearCeldaTotal($"{promedioMensual:N0}", TextAlignment.CENTER));

            campoCard.AddCell(new Cell().Add(tablaLluvias).SetBorder(Border.NO_BORDER));

            var leyenda = new Paragraph("Valores en milímetros (mm)")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                    .SetFontSize(6)
                    .SetFontColor(_colorSecundario)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(2);

            campoCard.AddCell(new Cell()
                .Add(leyenda)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingTop(0));

        }
        private Cell CrearHeaderLluviaHorizontal(string texto)
        {
            return new Cell()
                .SetBackgroundColor(_colorPrimario)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .SetFontSize(7)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .Add(new Paragraph(texto));
        }

        private Cell CrearCeldaLluviaHorizontal(string texto)
        {
            return new Cell()
                .SetPadding(5)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(_colorPrimario));
        }

        private string ObtenerNombreMesCorto(string mesAnio)
        {
            try
            {
                var partes = mesAnio.Split('-');
                if (partes.Length == 2)
                {
                    var mes = int.Parse(partes[0]);
                    var nombres = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                    return nombres[mes - 1];
                }
            }
            catch { }
            return mesAnio.Length > 3 ? mesAnio.Substring(0, 3) : mesAnio;
        }
       
        private Dictionary<int, List<ResumenClima>> ObtenerLluviasPorCampo(ReporteCierreCampania reporte)
        {
            if (string.IsNullOrEmpty(reporte.LluviasPorMesJson))
                return new Dictionary<int, List<ResumenClima>>();

            try
            {
                var lluvias = JsonSerializer.Deserialize<List<ResumenClima>>(reporte.LluviasPorMesJson)
                             ?? new List<ResumenClima>();

                return lluvias.GroupBy(l => l.IdCampo)
                             .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch
            {
                return new Dictionary<int, List<ResumenClima>>();
            }
        }

        private Cell CrearCeldaHeaderClima(string texto)
        {
            return new Cell()
                .SetBackgroundColor(_colorSecundario)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .SetFontSize(8)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .Add(new Paragraph(texto));
        }

        private Cell CrearCeldaDatosClima(string texto, TextAlignment alineacion)
        {
            return new Cell()
                .SetPadding(5)
                .SetFontSize(8)
                .SetTextAlignment(alineacion)
                .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)));
        }

        private Cell CrearCeldaLluviaMensual(string texto, TextAlignment alineacion = TextAlignment.LEFT)
        {
            return new Cell()
                .SetPadding(3)
                .SetFontSize(7)
                .SetTextAlignment(alineacion)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)));
        }

        private string ObtenerNombreMes(string mesAnio)
        {
            try
            {
                var partes = mesAnio.Split('-');
                if (partes.Length == 2)
                {
                    var mes = int.Parse(partes[0]);
                    var anio = partes[1];
                    var nombres = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                    return $"{nombres[mes - 1]} {anio}";
                }
            }
            catch { }
            return mesAnio;
        }

        private void AgregarDistribucionGastos(Document document, ReporteCierreCampania reporte)
        {
            try
            {

                var gastosPorCategoria = JsonSerializer.Deserialize<List<ResumenGasto>>(reporte.GastosPorCategoriaJson);
                if (gastosPorCategoria == null || !gastosPorCategoria.Any()) return;

                var sectionTitle = CrearTituloSeccion("DISTRIBUCIÓN DE GASTOS");
                document.Add(sectionTitle);

                var gastosTable = new Table(4)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(5);

                // Header de la tabla
                gastosTable.AddCell(CrearCeldaHeader("CATEGORÍA"));
                gastosTable.AddCell(CrearCeldaHeader("GASTO ARS"));
                gastosTable.AddCell(CrearCeldaHeader("GASTO USD"));
                gastosTable.AddCell(CrearCeldaHeader("PARTICIPACIÓN"));

                // Ordenar por monto descendente
                gastosPorCategoria = gastosPorCategoria
                    .OrderByDescending(g => g.CostoArs) // Asumiendo un tipo de cambio
                    .ToList();

                // Datos de gastos por categoría
                foreach (var gasto in gastosPorCategoria)
                {
                    if (gasto.CostoArs > 0 || gasto.CostoUsd > 0)
                    {
                        var porcentaje = reporte.GastosTotalesArs > 0 ?
                            (gasto.CostoArs / reporte.GastosTotalesArs) * 100 : 0;

                        gastosTable.AddCell(CrearCeldaDatos(gasto.Categoria));
                        gastosTable.AddCell(CrearCeldaDatos(gasto.CostoArs > 0 ? $"${gasto.CostoArs:N0}" : "-", TextAlignment.RIGHT));
                        gastosTable.AddCell(CrearCeldaDatos(gasto.CostoUsd > 0 ? $"${gasto.CostoUsd:N2}" : "-", TextAlignment.RIGHT));
                        gastosTable.AddCell(CrearCeldaDatos($"{porcentaje:N1}%", TextAlignment.RIGHT));
                    }
                }

                // Si no hay gastos, mostrar mensaje
                if (!gastosPorCategoria.Any(g => g.CostoArs > 0 || g.CostoUsd > 0))
                {
                    gastosTable.AddCell(new Cell(1, 4)
                        .Add(new Paragraph("No se registraron gastos administrativos")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                            .SetFontSize(9)
                            .SetFontColor(_colorSecundario)
                            .SetTextAlignment(TextAlignment.CENTER))
                        .SetPadding(10)
                        .SetBorder(Border.NO_BORDER));
                }
                else
                {
                    // Total general
                    gastosTable.AddCell(CrearCeldaTotal("TOTAL GASTOS"));
                    gastosTable.AddCell(CrearCeldaTotal($"${reporte.GastosTotalesArs:N0}", TextAlignment.RIGHT));
                    gastosTable.AddCell(CrearCeldaTotal($"${reporte.GastosTotalesUsd:N2}", TextAlignment.RIGHT));
                    gastosTable.AddCell(CrearCeldaTotal("100%", TextAlignment.RIGHT));
                }

                document.Add(gastosTable);

            }
            catch (Exception ex)
            {

            }
        }


        private Cell CrearHeaderCampo(string label, string valor)
        {
            return new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(valor)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10)
                    .SetFontColor(_colorPrimario)
                    .SetMarginBottom(2))
                .Add(new Paragraph(label)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(_colorSecundario));
        }

        private Cell CrearHeaderTablaLotes(string texto)
        {
            return new Cell()
                .SetBackgroundColor(_colorPrimario)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6)
                .SetFontSize(8)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .Add(new Paragraph(texto));
        }

        private Cell CrearCeldaLote(string texto, TextAlignment alineacion)
        {
            return new Cell()
                .SetPadding(5)
                .SetFontSize(8)
                .SetTextAlignment(alineacion)
                .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)));
        }

        private Cell CrearCeldaTotal(string texto, TextAlignment alineacion, int colSpan = 1)
        {
            var cell = new Cell(1, colSpan)
                .SetPadding(5)
                .SetFontSize(8)
                .SetTextAlignment(alineacion)
                .SetBackgroundColor(_colorFondo)
                .SetBorderTop(new SolidBorder(_colorPrimario, 1))
                .SetBorderBottom(Border.NO_BORDER)
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

            return cell;
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
        private void AgregarFooter(Document document)
        {
            var footer = new Paragraph($"Generado por el Sistema de Gestión AgroLab el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(8)
                .SetFontColor(_colorSecundario)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30)
                .SetPaddingTop(10)
                .SetBorderTop(new SolidBorder(_colorBorde, 0.5f));
            document.Add(footer);
        }

        #region Métodos Auxiliares es
        private Paragraph CrearTituloSeccion(string titulo)
        {
            return new Paragraph(titulo)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetFontColor(_colorPrimario)
                .SetMarginTop(20)
                .SetMarginBottom(5)
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

        private Cell CrearTarjetaMetrica(string titulo, string valor)
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

        private Cell CrearCeldaHeader(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(_colorPrimario)
                .SetPadding(8)
                .SetTextAlignment(TextAlignment.CENTER);
        }

        private Cell CrearCeldaDatos(string texto, TextAlignment alignment = TextAlignment.LEFT)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(8))
                .SetPadding(6)
                .SetBorderBottom(new SolidBorder(_colorBorde, 0.5f))
                .SetTextAlignment(alignment);
        }

        private Cell CrearCeldaTotal(string texto, TextAlignment alignment = TextAlignment.LEFT)
        {
            return new Cell()
                .Add(new Paragraph(texto)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(232, 244, 248))
                .SetPadding(8)
                .SetBorderBottom(new SolidBorder(_colorPrimario, 1))
                .SetTextAlignment(alignment);
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