namespace AgroForm.Business.Contracts
{
    // ============================================================
    // DTOs para datos satelitales
    // ============================================================

    /// <summary>
    /// DTO con los índices satelitales recientes de un lote.
    /// Se usa para reemplazar el NDVI simulado en el reporte integral.
    /// </summary>
    public class IndicesSatelitalesLoteDto
    {
        /// <summary>NDVI promedio del período (últimos N días o campaña)</summary>
        public decimal? NDVIPromedio { get; set; }

        /// <summary>NDWI promedio del período</summary>
        public decimal? NDWIPromedio { get; set; }

        /// <summary>Último NDVI disponible (más reciente)</summary>
        public decimal? UltimoNDVI { get; set; }

        /// <summary>Último NDWI disponible (más reciente)</summary>
        public decimal? UltimoNDWI { get; set; }

        /// <summary>Serie temporal completa para el gráfico de evolución</summary>
        public List<DatoIndiceSatelitalDto> SerieTemporal { get; set; } = new();

        /// <summary>Indica si los datos son reales (satelitales) o simulados</summary>
        public bool EsSatelital { get; set; }

        /// <summary>Fecha del último dato disponible</summary>
        public DateTime? UltimaFecha { get; set; }

        /// <summary>Cloud cover promedio del período</summary>
        public decimal? CloudCoverPromedio { get; set; }
    }

    /// <summary>
    /// Un punto de la serie temporal de índices satelitales.
    /// Compatible con DatoEvolucion para integrarse en el gráfico existente.
    /// </summary>
    public class DatoIndiceSatelitalDto
    {
        public DateTime Fecha { get; set; }
        public decimal? NDVI { get; set; }
        public decimal? NDWI { get; set; }
        public decimal? CloudCover { get; set; }
        public bool EsValido { get; set; } = true;
    }

    /// <summary>
    /// Request para obtener índices satelitales de un lote
    /// </summary>
    public class IndicesSatelitalesRequest
    {
        public int IdLote { get; set; }
        public int? IdCampania { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    /// <summary>
    /// DTO con el resumen de índices de una campaña
    /// </summary>
    public class IndiceCampaniaDto
    {
        public decimal? NDVIPromedio { get; set; }
        public decimal? NDWIPromedio { get; set; }
        public decimal? NDVIMaximo { get; set; }
        public decimal? NDWIMaximo { get; set; }
        public decimal? NDVIMinimo { get; set; }
        public decimal? NDWIMinimo { get; set; }
        public int TotalObservaciones { get; set; }
        public int ObservacionesValidas { get; set; }
        public int IdCampania { get; set; }
        public string? Campania { get; set; }
    }

    /// <summary>
    /// DTO para el endpoint de lotes pendientes de actualización (usado por n8n)
    /// </summary>
    public class LotePendienteActualizacionDto
    {
        public int IdLote { get; set; }
        public string? NombreLote { get; set; }
        public int IdCampo { get; set; }
        public string? NombreCampo { get; set; }
        public DateTime? UltimaConsulta { get; set; }
        public int DiasSinActualizar { get; set; }
        public string? GeometriaSimplificada { get; set; }
    }
}
