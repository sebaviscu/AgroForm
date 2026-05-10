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
    }
}
