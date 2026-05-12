using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Contracts
{
    public interface ICatalogoService : IServiceBase<Catalogo>
    {

        Task<OperationResult<List<Catalogo>>> GetByType(TipoCatalogoEnum tipo);
        Task<OperationResult<List<Catalogo>>> GetAllActive();

        /// <summary>
        /// Gets all visible catalog items for the current license, applying the side-table visibility filter.
        /// </summary>
        Task<OperationResult<List<Catalogo>>> GetVisibleByLicenseAsync();

        /// <summary>
        /// Gets all catalog items for the current license's configuration view.
        /// Returns both global and own items with visibility info.
        /// </summary>
        Task<OperationResult<List<Catalogo>>> GetAllForLicenseConfigAsync();

        /// <summary>
        /// Gets catalog items by type for the current license, applying visibility filter.
        /// </summary>
        Task<OperationResult<List<Catalogo>>> GetByTipoForLicenseAsync(TipoCatalogoEnum tipo);

        /// <summary>
        /// Sets visibility of a global catalog item for the current license.
        /// </summary>
        Task<OperationResult> SetVisibilityAsync(int idCatalogo, bool visible);

        /// <summary>
        /// Toggles the Activo field of a license-owned catalog item directly.
        /// </summary>
        Task<OperationResult> ToggleActivoAsync(int idCatalogo);

        /// <summary>
        /// Checks if a global catalog item is visible for the given license.
        /// Returns true if no LicenciasCatalogos entry hides it, or if entry has Activo=true.
        /// </summary>
        Task<bool> CheckVisibilityAsync(int idCatalogo, int idLicencia);
    }
}
