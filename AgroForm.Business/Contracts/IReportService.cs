using AgroForm.Business.Services;
using AgroForm.Model;

namespace AgroForm.Business.Contracts
{
    public interface IReportService
    {
        /// <summary>
        /// Obtiene datos comparativos de campos/lotes para el reporte.
        /// </summary>
        Task<OperationResult<List<ReporteComparativaCampoDto>>> GetComparativaCamposAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null);

        /// <summary>
        /// Obtiene el reporte integral de un campo/lote con todas las secciones:
        /// Resumen Ejecutivo, Timeline, Evolución, Clima, Suelo, Costos, Rendimiento, Alertas, Historial Multi-Campaña.
        /// </summary>
        Task<OperationResult<ReporteCampoIntegralDto>> GetReporteCampoIntegralAsync(
            int idCampo,
            int? idCampania = null);

        /// <summary>
        /// Obtiene la comparativa entre dos campos para el tab Comparativa
        /// </summary>
        Task<OperationResult<ComparativaCamposDto>> GetComparativaCamposIntegralAsync(
            int idCampoPrincipal,
            int? idCampoSecundario,
            int? idCampania = null);
        /// <summary>
        /// Obtiene el reporte de rendimiento de cosecha con KPIs, tabla, rankings, gráficos e indicadores.
        /// </summary>
        Task<OperationResult<RendimientoCosechaReporteDto>> GetRendimientoCosechaAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string ordenarPor = "RendimientoTonHa",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamanoPagina = 20);

        /// <summary>
        /// Obtiene el reporte de aplicaciones agrícolas (Pulverización + Fertilización)
        /// con KPIs, tabla principal, timeline, análisis de insumos, gráficos y trazabilidad.
        /// </summary>
        Task<OperationResult<AplicacionReporteDto>> GetAplicacionesAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null,
            int? idTipoAplicacion = null,
            int? idProducto = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string ordenarPor = "Fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamanoPagina = 20);
    }
}
