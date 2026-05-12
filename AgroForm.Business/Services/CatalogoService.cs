using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class CatalogoService : ServiceBase<Catalogo>, ICatalogoService
    {
        public CatalogoService(IUnitOfWork unitOfWork, ILogger<CatalogoService> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public async Task<OperationResult<List<Catalogo>>> GetByType(TipoCatalogoEnum tipo)
        {
            try
            {
                // Get globals + own records filtered by type
                var query = _repository.Query().AsNoTracking()
                    .Where(e => e.Tipo == tipo && e.Activo);

                // Apply multitenant OR filter: globals (NULL) + own records
                if (_userContext.IdLicencia.HasValue)
                {
                    query = query.Where(e => e.IdLicencia == null || e.IdLicencia == _userContext.IdLicencia);
                }

                var entities = await query.OrderBy(e => e.Nombre).ToListAsync();

                if (!entities.Any())
                    return OperationResult<List<Catalogo>>.Failure("Catálogo por tipo no encontrado");

                return OperationResult<List<Catalogo>>.SuccessResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catálogos por tipo {tipo}", tipo);
                return OperationResult<List<Catalogo>>.Failure("Error al obtener Catálogos por tipo");
            }
        }

        public async Task<OperationResult<List<Catalogo>>> GetAllActive()
        {
            try
            {
                var query = _repository.Query().AsNoTracking()
                    .Where(e => e.Activo);

                // Apply multitenant OR filter: globals (NULL) + own records
                if (_userContext.IdLicencia.HasValue)
                {
                    query = query.Where(e => e.IdLicencia == null || e.IdLicencia == _userContext.IdLicencia);
                }

                var entities = await query.ToListAsync();

                if (!entities.Any())
                    return OperationResult<List<Catalogo>>.Failure("Catálogos activos no encontrados");

                return OperationResult<List<Catalogo>>.SuccessResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los Catálogos activos");
                return OperationResult<List<Catalogo>>.Failure("Error al obtener todos los Catálogos activos");
            }
        }

        /// <summary>
        /// Gets all visible catalog items for the current license.
        /// Returns own items + global items not hidden in LicenciasCatalogos side table.
        /// </summary>
        public async Task<OperationResult<List<Catalogo>>> GetVisibleByLicenseAsync()
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;

                var query = _repository.Query().AsNoTracking()
                    .Where(c => c.IdLicencia == licenciaId
                        || (c.IdLicencia == null
                            && !_unitOfWork.Repository<LicenciasCatalogos>().Query()
                                .Any(lc => lc.IdCatalogo == c.Id && lc.IdLicencia == licenciaId && !lc.Activo)));

                var list = await query.OrderBy(c => c.Nombre).ToListAsync();
                return OperationResult<List<Catalogo>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catálogos visibles para la licencia");
                return OperationResult<List<Catalogo>>.Failure("Ocurrió un problema al obtener los catálogos visibles.", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets all catalog items for configuration: global items + own items.
        /// </summary>
        public async Task<OperationResult<List<Catalogo>>> GetAllForLicenseConfigAsync()
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;

                var query = _repository.Query().AsNoTracking()
                    .Where(c => c.IdLicencia == licenciaId
                        || c.IdLicencia == null);

                var list = await query.OrderBy(c => c.Tipo).ThenBy(c => c.Nombre).ToListAsync();
                return OperationResult<List<Catalogo>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catálogos para configuración");
                return OperationResult<List<Catalogo>>.Failure("Ocurrió un problema al obtener los catálogos.", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets catalog items by type for the current license, applying visibility filter.
        /// </summary>
        public async Task<OperationResult<List<Catalogo>>> GetByTipoForLicenseAsync(TipoCatalogoEnum tipo)
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;

                var query = _repository.Query().AsNoTracking()
                    .Where(c => c.Tipo == tipo
                        && (c.IdLicencia == licenciaId
                            || (c.IdLicencia == null
                                && !_unitOfWork.Repository<LicenciasCatalogos>().Query()
                                    .Any(lc => lc.IdCatalogo == c.Id && lc.IdLicencia == licenciaId && !lc.Activo))));

                var list = await query.OrderBy(c => c.Nombre).ToListAsync();
                return OperationResult<List<Catalogo>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catálogos por tipo para licencia");
                return OperationResult<List<Catalogo>>.Failure("Error al obtener Catálogos por tipo", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Toggles the Activo field of a license-owned catalog item.
        /// </summary>
        public async Task<OperationResult> ToggleActivoAsync(int idCatalogo)
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;
                if (!licenciaId.HasValue)
                    return OperationResult.Failure("No hay una licencia activa.", "NO_LICENSE");

                var catalogo = await _repository.GetAsync(c => c.Id == idCatalogo && c.IdLicencia == licenciaId.Value);
                if (catalogo == null)
                    return OperationResult.Failure("El catálogo propio no existe.", "NOT_FOUND");

                catalogo.Activo = !catalogo.Activo;
                catalogo.ModificationDate = TimeHelper.GetArgentinaTime();
                catalogo.ModificationUser = _userContext.UserName;
                await _repository.UpdateAsync(catalogo);
                await _unitOfWork.SaveAsync();

                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al alternar Activo del catálogo propio {IdCatalogo}", idCatalogo);
                return OperationResult.Failure($"Ocurrió un problema al alternar el estado: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Checks if a global catalog item is visible for the given license.
        /// Returns true if no LicenciasCatalogos entry hides it, or if entry has Activo=true.
        /// </summary>
        public async Task<bool> CheckVisibilityAsync(int idCatalogo, int idLicencia)
        {
            try
            {
                var entry = await _unitOfWork.Repository<LicenciasCatalogos>().Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(lc => lc.IdLicencia == idLicencia && lc.IdCatalogo == idCatalogo);

                // No entry means visible by default
                if (entry == null)
                    return true;

                return entry.Activo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar visibilidad del catálogo {IdCatalogo} para licencia {IdLicencia}", idCatalogo, idLicencia);
                return true; // Default to visible on error
            }
        }

        /// <summary>
        /// Sets visibility of a global catalog item for the current license.
        /// Creates or updates an entry in LicenciasCatalogos side table.
        /// </summary>
        public async Task<OperationResult> SetVisibilityAsync(int idCatalogo, bool visible)
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;
                if (!licenciaId.HasValue)
                    return OperationResult.Failure("No hay una licencia activa.", "NO_LICENSE");

                // Verify the catalog item exists and is a global record
                var catalogo = await _repository.GetAsync(c => c.Id == idCatalogo && c.IdLicencia == null);
                if (catalogo == null)
                    return OperationResult.Failure("El catálogo global no existe.", "NOT_FOUND");

                // Check if there's already a visibility entry
                var lcRepo = _unitOfWork.Repository<LicenciasCatalogos>();
                var existingEntry = await lcRepo.GetAsync(lc => lc.IdLicencia == licenciaId.Value && lc.IdCatalogo == idCatalogo);

                if (existingEntry != null)
                {
                    existingEntry.Activo = visible;
                    existingEntry.ModificationDate = TimeHelper.GetArgentinaTime();
                    existingEntry.ModificationUser = _userContext.UserName;
                    await lcRepo.UpdateAsync(existingEntry);
                }
                else
                {
                    var newEntry = new LicenciasCatalogos
                    {
                        IdLicencia = licenciaId.Value,
                        IdCatalogo = idCatalogo,
                        Activo = visible,
                        RegistrationDate = TimeHelper.GetArgentinaTime(),
                        RegistrationUser = _userContext.UserName
                    };
                    await lcRepo.AddAsync(newEntry);
                }

                await _unitOfWork.SaveAsync();
                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer visibilidad del catálogo {IdCatalogo}", idCatalogo);
                return OperationResult.Failure($"Ocurrió un problema al establecer la visibilidad: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
