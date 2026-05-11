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
        public int ScoreRiesgo { get; set; }
    }

    /// <summary>
    /// Resumen de un cultivo en un lote específico
    /// </summary>
    public class CultivoResumenDto
    {
        public string Lote { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Variedad { get; set; }
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
        public int? CantidadHeladas { get; set; }
        public string BalanceHidrico { get; set; } = "Normal";
        public string EstresHidrico { get; set; } = "Sin estrés";
        public List<DatoClimatico> Registros { get; set; } = new();
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
}
