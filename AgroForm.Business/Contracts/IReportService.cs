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
    }
}
