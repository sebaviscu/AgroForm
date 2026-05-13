using AgroForm.Model.Actividades;

namespace AgroForm.Business.Contracts
{
    public class ReporteComparativaCampoDto
    {
        public int IdCampo { get; set; }
        public string Campo { get; set; } = string.Empty;
        public int IdLote { get; set; }
        public string Lote { get; set; } = string.Empty;
        public decimal? SuperficieHa { get; set; }

        // Cultivo
        public string? Cultivo { get; set; }
        public int? IdCultivo { get; set; }

        // Fechas
        public DateTime? FechaSiembra { get; set; }
        public DateTime? FechaCosecha { get; set; }

        // Rendimiento
        public decimal? RendimientoTonHa { get; set; }
        public decimal? RendimientoTotalTon { get; set; }

        // Insumos / Fertilizantes
        public decimal? TotalFertilizantesKgHa { get; set; }
        public decimal? TotalPulverizacionesLtsHa { get; set; }

        // Costos
        public decimal? CostoTotalARS { get; set; }
        public decimal? CostoTotalUSD { get; set; }
        public decimal? CostoPorHaARS { get; set; }
        public decimal? CostoPorHaUSD { get; set; }

        // Margen bruto (estimado)
        public decimal? MargenBrutoARS { get; set; }
        public decimal? MargenBrutoUSD { get; set; }

        // Cantidad de labores
        public int CantidadLabores { get; set; }

        // Para ranking
        public int RankingRendimiento { get; set; }
    }

    // ============================================================
    // DTOs para el Informe Integral del Campo
    // ============================================================

    /// <summary>
    /// DTO principal que contiene todas las secciones del informe integral
    /// </summary>
    public class ReporteCampoIntegralDto
    {
        public ResumenEjecutivoDto ResumenEjecutivo { get; set; } = new();
        public List<TimelineEventoDto> Timeline { get; set; } = new();
        public EvolucionCultivoDto EvolucionCultivo { get; set; } = new();
        public AnalisisClimaticoDto AnalisisClimatico { get; set; } = new();
        public AnalisisSueloDto AnalisisSuelo { get; set; } = new();
        public CostosRentabilidadDto CostosRentabilidad { get; set; } = new();
        public RendimientoCosechaDto RendimientoCosecha { get; set; } = new();
        public List<AlertaDto> Alertas { get; set; } = new();
        public List<HistorialCampaniaDto> HistorialMultiCampania { get; set; } = new();

        /// <summary>
        /// ID de la campaña seleccionada para el reporte (null = Todas)
        /// </summary>
        public int? IdCampaniaSeleccionada { get; set; }

        /// <summary>
        /// Nombre de la campaña seleccionada
        /// </summary>
        public string? NombreCampaniaSeleccionada { get; set; }

        /// <summary>
        /// Indica si la campaña seleccionada es la actual del sistema
        /// </summary>
        public bool EsCampaniaActual { get; set; }
    }

    /// <summary>
    /// Resumen Ejecutivo - KPIs principales del campo/lote
    /// </summary>
    public class ResumenEjecutivoDto
    {
        public string Campo { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public decimal SuperficieHa { get; set; }
        public string? Campania { get; set; }
        public int? DiasDesdeSiembra { get; set; }
        public DateTime? FechaSiembra { get; set; }
        public DateTime? FechaCosechaEstimada { get; set; }
        public string? UltimaLluvia { get; set; }
        public decimal? NDVIPromedio { get; set; }
        public string EstadoGeneral { get; set; } = "Sin datos";
        public string RiesgoActual { get; set; } = "Bajo";
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string? CoordenadasPoligono { get; set; }

        // Multi-crop support: each lot can have different crops
        public List<CultivoResumenDto> Cultivos { get; set; } = new();

        // Scores
        public int ScoreProductividad { get; set; }
        public int ScoreSaludCultivo { get; set; }
        public int ScoreHumedad { get; set; }
        public int ScoreHidrico { get; set; }
        public int ScoreRiesgo { get; set; }
    }

    /// <summary>
    /// Resumen de un cultivo en un lote específico
    /// </summary>
    public class CultivoResumenDto
    {
        public string Lote { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal SuperficieHa { get; set; }
        public int CantidadCiclos { get; set; }
        public int CantidadInactivos { get; set; }
        public bool IsActivo { get; set; }
    }

    /// <summary>
    /// Evento para el Timeline Agronómico
    /// </summary>
    public class TimelineEventoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoActividad { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Lote { get; set; }
        public string? CicloCultivo { get; set; }
        public string? Responsable { get; set; }
    }

    /// <summary>
    /// Evolución del Cultivo - datos para gráficos
    /// </summary>
    public class EvolucionCultivoDto
    {
        public List<DatoEvolucion> Evolucion { get; set; } = new();
        public ComparativaEvolucionDto? Comparativa { get; set; }
    }

    public class DatoEvolucion
    {
        public DateTime Fecha { get; set; }
        public decimal? NDVI { get; set; }
        public decimal? Humedad { get; set; }
        public decimal? Temperatura { get; set; }
        public decimal? Precipitacion { get; set; }
    }

    public class ComparativaEvolucionDto
    {
        public string CampaniaAnterior { get; set; } = string.Empty;
        public decimal? NDVIPromedioAnterior { get; set; }
        public decimal? NDVIPromedioActual { get; set; }
        public decimal? RendimientoAnterior { get; set; }
        public decimal? RendimientoActual { get; set; }
    }

    /// <summary>
    /// Análisis Climático
    /// </summary>
    public class AnalisisClimaticoDto
    {
        public decimal? LluviaAcumulada { get; set; }
        public int? DiasSinLluvia { get; set; }
        public decimal? TempMinima { get; set; }
        public decimal? TempMaxima { get; set; }
        public decimal? TempPromedio { get; set; }
        public decimal? HumedadRelativa { get; set; }
        public decimal? SensacionTermica { get; set; }
        public string? DescripcionClima { get; set; }
        public string? IconoClima { get; set; }
        public int? CantidadHeladas { get; set; }
        public string BalanceHidrico { get; set; } = "Normal";
        public string EstresHidrico { get; set; } = "Sin estrés";
        public List<DatoClimatico> Registros { get; set; } = new();
        public decimal? ProbabilidadLluvia { get; set; }

        /// <summary>
        /// Indica si los datos meteorológicos actuales (Open-Meteo) no corresponden a la campaña seleccionada
        /// </summary>
        public bool EsHistorico { get; set; }

        /// <summary>
        /// Precipitación total acumulada en el período de la campaña seleccionada
        /// </summary>
        public decimal? LluviaTotalCampania { get; set; }
    }

    public class DatoClimatico
    {
        public DateTime Fecha { get; set; }
        public decimal? Temperatura { get; set; }
        public decimal? Precipitacion { get; set; }
        public decimal? Humedad { get; set; }
        public string? TipoClima { get; set; }
    }

    /// <summary>
    /// Análisis de Suelo con interpretación automática
    /// </summary>
    public class AnalisisSueloDto
    {
        public DateTime? FechaAnalisis { get; set; }
        public ParametroSueloDto? PH { get; set; }
        public ParametroSueloDto? MateriaOrganica { get; set; }
        public ParametroSueloDto? Nitrogeno { get; set; }
        public ParametroSueloDto? Fosforo { get; set; }
        public ParametroSueloDto? Potasio { get; set; }
        public ParametroSueloDto? ConductividadElectrica { get; set; }
        public ParametroSueloDto? CIC { get; set; }
        public string? Textura { get; set; }
        public decimal? ProfundidadCm { get; set; }
        public List<string> Recomendaciones { get; set; } = new();
    }

    public class ParametroSueloDto
    {
        public decimal? Valor { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public string Interpretacion { get; set; } = "Sin datos";
        public string Nivel { get; set; } = "Medio"; // Bajo, Medio, Alto
        public string? Advertencia { get; set; }
    }

    /// <summary>
    /// Costos y Rentabilidad
    /// </summary>
    public class CostosRentabilidadDto
    {
        public decimal CostoTotalARS { get; set; }
        public decimal CostoTotalUSD { get; set; }
        public decimal CostoPorHaARS { get; set; }
        public decimal CostoPorHaUSD { get; set; }
        public decimal? MargenEstimadoARS { get; set; }
        public decimal? MargenEstimadoUSD { get; set; }
        public decimal? RentabilidadProyectada { get; set; }
        public CostosDesglosadosDto Desglose { get; set; } = new();
    }

    public class CostosDesglosadosDto
    {
        public decimal SiembraARS { get; set; }
        public decimal SiembraUSD { get; set; }
        public decimal FertilizacionARS { get; set; }
        public decimal FertilizacionUSD { get; set; }
        public decimal PulverizacionARS { get; set; }
        public decimal PulverizacionUSD { get; set; }
        public decimal RiegoARS { get; set; }
        public decimal RiegoUSD { get; set; }
        public decimal CosechaARS { get; set; }
        public decimal CosechaUSD { get; set; }
        public decimal MonitoreoARS { get; set; }
        public decimal MonitoreoUSD { get; set; }
        public decimal AnalisisSueloARS { get; set; }
        public decimal AnalisisSueloUSD { get; set; }
        public decimal OtrasLaboresARS { get; set; }
        public decimal OtrasLaboresUSD { get; set; }
        public decimal SiloBolsasARS { get; set; }
        public decimal SiloBolsasUSD { get; set; }
    }

    /// <summary>
    /// Rendimiento y Cosecha
    /// </summary>
    public class RendimientoCosechaDto
    {
        public decimal? RendimientoTonHa { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public decimal? HumedadCosecha { get; set; }
        public decimal? SuperficieCosechadaHa { get; set; }
        public DateTime? FechaCosecha { get; set; }
        public List<DatoRendimientoHistorico> Historico { get; set; } = new();
    }

    public class DatoRendimientoHistorico
    {
        public string Campania { get; set; } = string.Empty;
        public string? Cultivo { get; set; }
        public decimal? RendimientoTonHa { get; set; }
    }

    /// <summary>
    /// Alerta Inteligente
    /// </summary>
    public class AlertaDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Severidad { get; set; } = "Media"; // Baja, Media, Alta
        public string Mensaje { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string? Recomendacion { get; set; }
        public string Icono { get; set; } = "ph-warning-circle";
    }

    /// <summary>
    /// Historial Multi-Campaña
    /// </summary>
    public class HistorialCampaniaDto
    {
        public string Campania { get; set; } = string.Empty;
        public int IdCampania { get; set; }
        public string? Cultivo { get; set; }
        public List<LaborDTO> Labores { get; set; } = new();
        public decimal? RendimientoTonHa { get; set; }
        public decimal CostoTotalARS { get; set; }
        public decimal CostoTotalUSD { get; set; }
        public int CantidadLabores { get; set; }
    }

    /// <summary>
    /// Request para obtener el reporte integral
    /// </summary>
    public class ReporteCampoRequest
    {
        public int IdCampo { get; set; }
        public int? IdCampania { get; set; }
    }

    /// <summary>
    /// DTO para la comparativa entre dos campos (tab Comparativa)
    /// </summary>
    public class ComparativaCamposDto
    {
        public CampoComparativaDto CampoPrincipal { get; set; } = new();
        public CampoComparativaDto? CampoSecundario { get; set; }
    }

    public class CampoComparativaDto
    {
        public int IdCampo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal SuperficieHa { get; set; }
        public string? CultivoPrincipal { get; set; }
        public decimal CostoTotalARS { get; set; }
        public decimal CostoPorHaARS { get; set; }
        public decimal? RendimientoTonHa { get; set; }
        public decimal? RentabilidadProyectada { get; set; }
        public int CantidadLabores { get; set; }
        public int CantidadAlertas { get; set; }
        public string EstadoGeneral { get; set; } = string.Empty;
        // Desglose de costos por tipo de labor
        public CostosDesglosadosDto DesgloseCostos { get; set; } = new();
        // Rendimiento histórico por campaña
        public List<DatoRendimientoHistorico> RendimientoHistorico { get; set; } = new();
    }

    /// <summary>
    /// Request para la comparativa entre campos
    /// </summary>
    public class ComparativaRequest
    {
        public int IdCampoPrincipal { get; set; }
        public int? IdCampoSecundario { get; set; }
        public int? IdCampania { get; set; }
    }

    /// <summary>
    /// Request para obtener datos comparativos de campos/lotes
    /// </summary>
    public class ReporteComparativaRequest
    {
        public int? IdCampania { get; set; }
        public int? IdCampo { get; set; }
        public int? IdLote { get; set; }
        public int? IdCultivo { get; set; }
    }

    // ============================================================
    // DTOs para el Reporte de Rendimiento de Cosecha
    // ============================================================

    /// <summary>
    /// Request para el reporte de rendimiento de cosecha
    /// </summary>
    public class RendimientoCosechaRequest
    {
        public int? IdCampania { get; set; }
        public int? IdCampo { get; set; }
        public int? IdLote { get; set; }
        public int? IdCultivo { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string OrdenarPor { get; set; } = "RendimientoTonHa";
        public string OrdenDireccion { get; set; } = "desc";
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
    }

    /// <summary>
    /// DTO principal del reporte de rendimiento de cosecha
    /// </summary>
    public class RendimientoCosechaReporteDto
    {
        public RendimientoCosechaKpiDto Kpis { get; set; } = new();
        public List<DatoRendimientoLoteDto> DatosLotes { get; set; } = new();
        public List<RankingLoteDto> RankingLotes { get; set; } = new();
        public List<DatoRendimientoPorCultivo> RendimientoPorCultivo { get; set; } = new();
        public List<DatoRendimientoPorCampania> RendimientoPorCampania { get; set; } = new();
        public List<DatoRendimientoPorCampo> RendimientoPorCampo { get; set; } = new();
        public List<DatoEvolucionRendimiento> EvolucionRendimiento { get; set; } = new();
        public List<IndicadorInteligenteDto> Indicadores { get; set; } = new();
        public PaginacionDto Paginacion { get; set; } = new();
    }

    /// <summary>
    /// KPIs principales del reporte de rendimiento
    /// </summary>
    public class RendimientoCosechaKpiDto
    {
        public decimal? RendimientoPromedioTonHa { get; set; }
        public decimal? RendimientoMaximoTonHa { get; set; }
        public string? LoteMejorRendimiento { get; set; }
        public decimal? RendimientoMinimoTonHa { get; set; }
        public string? LotePeorRendimiento { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public decimal? SuperficieTotalCosechadaHa { get; set; }
        public decimal? HumedadPromedio { get; set; }
        public decimal? VariacionVsCampaniaAnterior { get; set; }
        public string? CampaniaAnterior { get; set; }
        public string? CampaniaActual { get; set; }
        public int TotalLotes { get; set; }
        public int LotesConRendimiento { get; set; }
        public decimal? CostoPromedioPorHa { get; set; }
        public string Moneda { get; set; } = "ARS";
    }

    /// <summary>
    /// Dato de rendimiento por lote (para la tabla principal)
    /// </summary>
    public class DatoRendimientoLoteDto
    {
        public int IdCosecha { get; set; }
        public int IdLote { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public string? Cultivo { get; set; }
        public string? Campania { get; set; }
        public decimal? RendimientoTonHa { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public decimal? SuperficieCosechadaHa { get; set; }
        public decimal? HumedadGrano { get; set; }
        public DateTime? FechaCosecha { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public string? Moneda { get; set; }
        public int RankingRendimiento { get; set; }
        public string? Tendencia { get; set; } // "subiendo", "estable", "bajando"
    }

    /// <summary>
    /// Ranking de lotes por rendimiento
    /// </summary>
    public class RankingLoteDto
    {
        public int Posicion { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public string? Cultivo { get; set; }
        public decimal RendimientoTonHa { get; set; }
        public string? Campania { get; set; }
    }

    /// <summary>
    /// Rendimiento agregado por cultivo
    /// </summary>
    public class DatoRendimientoPorCultivo
    {
        public string Cultivo { get; set; } = string.Empty;
        public decimal RendimientoPromedioTonHa { get; set; }
        public decimal? SuperficieTotalHa { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public int CantidadLotes { get; set; }
        public string Color { get; set; } = "#4CAF50";
    }

    /// <summary>
    /// Rendimiento agregado por campaña
    /// </summary>
    public class DatoRendimientoPorCampania
    {
        public string Campania { get; set; } = string.Empty;
        public decimal RendimientoPromedioTonHa { get; set; }
        public decimal? SuperficieTotalHa { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public int CantidadCosechas { get; set; }
    }

    /// <summary>
    /// Rendimiento agregado por campo
    /// </summary>
    public class DatoRendimientoPorCampo
    {
        public string Campo { get; set; } = string.Empty;
        public decimal RendimientoPromedioTonHa { get; set; }
        public decimal? SuperficieTotalHa { get; set; }
        public decimal? ProduccionTotalTon { get; set; }
        public int CantidadLotes { get; set; }
    }

    /// <summary>
    /// Dato para la evolución histórica del rendimiento (gráfico de líneas)
    /// </summary>
    public class DatoEvolucionRendimiento
    {
        public string Periodo { get; set; } = string.Empty; // "2023/24 - Soja"
        public string? Campania { get; set; }
        public string? Cultivo { get; set; }
        public decimal RendimientoTonHa { get; set; }
        public decimal? HumedadPromedio { get; set; }
        public decimal? SuperficieHa { get; set; }
    }

    /// <summary>
    /// Indicador inteligente con alertas y recomendaciones
    /// </summary>
    public class IndicadorInteligenteDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Severidad { get; set; } = "Media"; // Baja, Media, Alta
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Recomendacion { get; set; }
        public string Icono { get; set; } = "ph-info";
        public string Color { get; set; } = "#6c757d";
        public decimal? Valor { get; set; }
        public decimal? Umbral { get; set; }
    }

    /// <summary>
    /// Información de paginación
    /// </summary>
    public class PaginacionDto
    {
        public int PaginaActual { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public bool TieneAnterior => PaginaActual > 1;
        public bool TieneSiguiente => PaginaActual < TotalPaginas;
    }

    // ============================================================
    // DTOs para el Reporte de Aplicaciones (Pulverización + Fertilización)
    // ============================================================

    /// <summary>
    /// Request para el reporte de aplicaciones agrícolas
    /// </summary>
    public class AplicacionRequest
    {
        public int? IdCampania { get; set; }
        public int? IdCampo { get; set; }
        public int? IdLote { get; set; }
        public int? IdCultivo { get; set; }
        public int? IdTipoAplicacion { get; set; } // 3=Pulverizacion, 4=Fertilizado
        public int? IdProducto { get; set; } // ProductoAgroquimico (pulv) or Nutriente (fert)
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string OrdenarPor { get; set; } = "Fecha";
        public string OrdenDireccion { get; set; } = "desc";
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
    }

    /// <summary>
    /// DTO principal del reporte de aplicaciones agrícolas
    /// </summary>
    public class AplicacionReporteDto
    {
        public AplicacionKpiDto Kpis { get; set; } = new();
        public List<AplicacionLoteDto> DatosAplicaciones { get; set; } = new();
        public List<AplicacionTimelineDto> Timeline { get; set; } = new();
        public List<InsumoConsumoDto> AnalisisInsumos { get; set; } = new();
        public List<AplicacionTraceDto> Trazabilidad { get; set; } = new();
        public List<DatoAplicacionPorProducto> CostosPorProducto { get; set; } = new();
        public List<DatoAplicacionPorTipo> DistribucionPorTipo { get; set; } = new();
        public List<DatoAplicacionTimeline> AplicacionesTimeline { get; set; } = new();
        public List<DatoAplicacionPorCampania> ComparativaPorCampania { get; set; } = new();
        public List<DatoAplicacionPorCampo> ComparativaPorCampo { get; set; } = new();
        public PaginacionDto Paginacion { get; set; } = new();
        public List<IndicadorInteligenteDto> Indicadores { get; set; } = new();
    }

    /// <summary>
    /// KPIs principales del reporte de aplicaciones
    /// </summary>
    public class AplicacionKpiDto
    {
        public int TotalAplicaciones { get; set; }
        public int TotalPulverizaciones { get; set; }
        public int TotalFertilizaciones { get; set; }
        public decimal TotalLitrosAplicados { get; set; }
        public decimal TotalKgAplicados { get; set; }
        public decimal? CostoTotalARS { get; set; }
        public decimal? CostoTotalUSD { get; set; }
        public decimal? CostoPromedioPorHaARS { get; set; }
        public decimal? CostoPromedioPorHaUSD { get; set; }
        public string ProductoMasAplicado { get; set; } = string.Empty;
        public int ProductoMasAplicadoCantidad { get; set; }
        public string NutrienteMasAplicado { get; set; } = string.Empty;
        public int NutrienteMasAplicadoCantidad { get; set; }
        public decimal PromedioAplicacionesPorLote { get; set; }
        public decimal SuperficieTotalTratadaHa { get; set; }
        public int TotalLotes { get; set; }
        public string Moneda { get; set; } = "ARS";
    }

    /// <summary>
    /// Fila de la tabla principal de aplicaciones (unifica Pulverizacion y Fertilizacion)
    /// </summary>
    public class AplicacionLoteDto
    {
        public int Id { get; set; }
        public int IdTipoActividad { get; set; }
        public string TipoActividad { get; set; } = string.Empty; // "Pulverización" o "Fertilización"
        public string TipoActividadIcono { get; set; } = string.Empty;
        public string TipoActividadColor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Lote { get; set; } = string.Empty;
        public int IdLote { get; set; }
        public string Campo { get; set; } = string.Empty;
        public int IdCampo { get; set; }
        public string? Campania { get; set; }
        public string? Cultivo { get; set; }
        public string? ProductoAplicado { get; set; }
        public string? TipoProducto { get; set; } // "Agroquímico", "Fertilizante", "Nutriente"
        public decimal? Dosis { get; set; }
        public string? UnidadDosis { get; set; } // "Lts/Ha" o "Kg/Ha"
        public decimal? CantidadTotal { get; set; }
        public string? UnidadCantidad { get; set; } // "Litros" o "Kg"
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public decimal? Costo { get; set; }
        public string? Moneda { get; set; }
        public string? Responsable { get; set; }
        public string? Observacion { get; set; }
        public string? ObservacionCortada { get; set; }
        public decimal? SuperficieHa { get; set; }
        public decimal? CostoPorHa { get; set; }
    }

    /// <summary>
    /// Evento de timeline para el historial del lote
    /// </summary>
    public class AplicacionTimelineDto
    {
        public int Id { get; set; }
        public int IdTipoActividad { get; set; }
        public string TipoActividad { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string? Campania { get; set; }
        public string? Cultivo { get; set; }
        public string? Descripcion { get; set; }
        public string? ProductoAplicado { get; set; }
        public decimal? Dosis { get; set; }
        public string? Unidad { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public string? Responsable { get; set; }
    }

    /// <summary>
    /// Análisis de consumo de insumos agrupado
    /// </summary>
    public class InsumoConsumoDto
    {
        public string Producto { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty; // "Agroquímico", "Fertilizante", "Nutriente"
        public decimal CantidadTotal { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public decimal? CostoTotalARS { get; set; }
        public decimal? CostoTotalUSD { get; set; }
        public int CantidadAplicaciones { get; set; }
        public int CantidadLotes { get; set; }
        public string? CultivoPrincipal { get; set; }
        public string? CampaniaPrincipal { get; set; }
    }

    /// <summary>
    /// Trazabilidad de cada aplicación (auditoría)
    /// </summary>
    public class AplicacionTraceDto
    {
        public int Id { get; set; }
        public int IdTipoActividad { get; set; }
        public string TipoActividad { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public string? Campania { get; set; }
        public string? ProductoAplicado { get; set; }
        public decimal? Dosis { get; set; }
        public decimal? CantidadTotal { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public string? Responsable { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }

    /// <summary>
    /// Costos agregados por producto (para gráfico de barras)
    /// </summary>
    public class DatoAplicacionPorProducto
    {
        public string Producto { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public decimal CostoARS { get; set; }
        public decimal CostoUSD { get; set; }
        public decimal CantidadTotal { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public int CantidadAplicaciones { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// Distribución por tipo de aplicación (para gráfico de torta/donut)
    /// </summary>
    public class DatoAplicacionPorTipo
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aplicaciones en el tiempo (para gráfico de líneas/área)
    /// </summary>
    public class DatoAplicacionTimeline
    {
        public string Periodo { get; set; } = string.Empty; // "2025-01", "2025-02", etc.
        public DateTime FechaInicio { get; set; }
        public int CantidadPulverizaciones { get; set; }
        public int CantidadFertilizaciones { get; set; }
        public int TotalAplicaciones { get; set; }
        public decimal? CostoTotalARS { get; set; }
    }

    /// <summary>
    /// Comparativa por campaña (para gráfico de barras agrupadas)
    /// </summary>
    public class DatoAplicacionPorCampania
    {
        public string Campania { get; set; } = string.Empty;
        public int TotalAplicaciones { get; set; }
        public int Pulverizaciones { get; set; }
        public int Fertilizaciones { get; set; }
        public decimal CostoTotalARS { get; set; }
        public decimal CostoTotalUSD { get; set; }
        public decimal TotalLitros { get; set; }
        public decimal TotalKg { get; set; }
    }

    /// <summary>
    /// Comparativa por campo (para gráfico de barras horizontal)
    /// </summary>
    public class DatoAplicacionPorCampo
    {
        public string Campo { get; set; } = string.Empty;
        public int TotalAplicaciones { get; set; }
        public int Pulverizaciones { get; set; }
        public int Fertilizaciones { get; set; }
        public decimal CostoTotalARS { get; set; }
        public decimal SuperficieHa { get; set; }
        public int CantidadLotes { get; set; }
    }

    // ============================================================
    // DTOs existentes de otros reportes se mantienen arriba
    // ============================================================
}
