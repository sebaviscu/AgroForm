using AgroForm.Business.Services;
using AgroForm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface ICultivoService : IServiceBase<Cultivo>
    {
        /// <summary>
        /// Gets all visible crops for the current license, applying the side-table visibility filter.
        /// Returns global crops (IdLicencia = NULL) that are NOT hidden + own crops.
        /// </summary>
        Task<OperationResult<List<Cultivo>>> GetVisibleByLicenseAsync();

        /// <summary>
        /// Gets all crops for the current license's configuration view.
        /// Returns both global and own crops with visibility info.
        /// Each crop includes: IsGlobal (IdLicencia == null), IsVisibleForLicense (visibility state).
        /// </summary>
        Task<OperationResult<List<Cultivo>>> GetAllForLicenseConfigAsync();

        /// <summary>
        /// Sets visibility of a global crop for the current license.
        /// Creates/updates an entry in LicenciasCultivos side table.
        /// </summary>
        /// <param name="idCultivo">The global crop ID</param>
        /// <param name="visible">false to hide, true to show</param>
        Task<OperationResult> SetVisibilityAsync(int idCultivo, bool visible);

        /// <summary>
        /// Toggles the Activo field of a license-owned crop directly.
        /// </summary>
        /// <param name="idCultivo">The crop ID (must be owned by the current license)</param>
        Task<OperationResult> ToggleActivoAsync(int idCultivo);

        /// <summary>
        /// Checks if a global crop is visible for the given license.
        /// Returns true if no LicenciasCultivos entry hides it, or if entry has Activo=true.
        /// </summary>
        Task<bool> CheckVisibilityAsync(int idCultivo, int idLicencia);

        /// <summary>
        /// Creates a new crop with automatic order shifting.
        /// If another crop already has the requested orden, shifts existing items up by 1.
        /// </summary>
        Task<OperationResult<Cultivo>> CreateWithOrderShiftAsync(Cultivo entity);

        /// <summary>
        /// Updates a crop with automatic order re-shifting.
        /// If the orden changed, shifts other items accordingly to maintain ordering integrity.
        /// </summary>
        Task<OperationResult<Cultivo>> UpdateCultivoAsync(Cultivo entity);
    }
}
