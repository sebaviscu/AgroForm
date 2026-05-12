using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Contracts
{
    public interface ICicloCultivoService : IServiceBase<CicloCultivo>
    {
        /// <summary>
        /// Creates a new crop cycle for a lot when a Siembra is registered.
        /// Convenience method that builds a CicloCultivo entity and delegates to CreateAsync.
        /// </summary>
        Task<OperationResult<CicloCultivo>> CrearCicloAsync(int idLote, int idCultivo, EpocaSiembra? epoca);

        /// <summary>
        /// Closes a crop cycle (sets FechaFin) when a Cosecha is registered.
        /// </summary>
        Task<OperationResult<CicloCultivo>> CerrarCicloAsync(int idCicloCultivo);

        /// <summary>
        /// Gets the active (non-closed) cycle for a given lot and optional epoca.
        /// </summary>
        Task<OperationResult<CicloCultivo?>> ObtenerCicloActivoAsync(int idLote, EpocaSiembra? epoca = null);

        /// <summary>
        /// Gets all cycles for a given lot, ordered by FechaInicio descending.
        /// </summary>
        Task<OperationResult<List<CicloCultivo>>> ObtenerCiclosPorLoteAsync(int idLote);
    }
}
