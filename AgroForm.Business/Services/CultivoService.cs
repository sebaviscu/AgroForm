using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AgroForm.Business.Services
{
    public class CultivoService : ServiceBase<Cultivo>, ICultivoService
    {
        public CultivoService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Cultivo>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<Cultivo>> CreateAsync(Cultivo entity)
        {
            try
            {
                // Check if user wants to create a global record (SuperAdmin) or owned record (regular user)
                // This logic is handled by ServiceBase.CreateAsync for IOptionalLicenciaEntity
                var result = await base.CreateAsync(entity);
                if (!result.Success)
                    return result;

                // Auto-create default phenological states ONLY for owned crops (license user created it)
                // Not for global crops created by SuperAdmin, unless they explicitly provided EstadosFenologicos
                if (entity.IdLicencia.HasValue && (entity.EstadosFenologicos == null || entity.EstadosFenologicos.Count == 0))
                {
                    var estadosDefault = new List<EstadoFenologico>
                    {
                        new() { Codigo = "E", Nombre = "Emergencia", Descripcion = "Plántula emerge del suelo.", IdCultivo = entity.Id, Activo = true, Orden = 1 },
                        new() { Codigo = "V6", Nombre = "Desarrollo Vegetativo", Descripcion = "Crecimiento de hojas.", IdCultivo = entity.Id, Activo = true, Orden = 2 },
                        new() { Codigo = "R", Nombre = "Reproductivo", Descripcion = "Transición a etapa reproductiva.", IdCultivo = entity.Id, Activo = true, Orden = 3 },
                        new() { Codigo = "F", Nombre = "Floración", Descripcion = "Inicio de floración.", IdCultivo = entity.Id, Activo = true, Orden = 4 },
                        new() { Codigo = "M", Nombre = "Madurez", Descripcion = "Frutos secos, semillas maduras.", IdCultivo = entity.Id, Activo = true, Orden = 5 }
                    };

                    await _unitOfWork.Repository<EstadoFenologico>().AddRangeAsync(estadosDefault);
                    await _unitOfWork.SaveAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar el Cultivo y sus Estados Fenológicos");
                return OperationResult<Cultivo>.Failure($"Ocurrió un problema al insertar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets all visible crops for the current license.
        /// Returns: own crops (IdLicencia = licenseId) + global crops (IdLicencia = NULL)
        /// that are NOT hidden in the LicenciasCultivos side table.
        /// </summary>
        public async Task<OperationResult<List<Cultivo>>> GetVisibleByLicenseAsync()
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;

                // Get all crops visible to this license:
                // 1. Own crops (IdLicencia = licenseId)
                // 2. Global crops (IdLicencia = NULL) that don't have a visibility entry hiding them
                var query = _repository.Query().AsNoTracking()
                    .Where(c => c.IdLicencia == licenciaId
                        || (c.IdLicencia == null
                            && !_unitOfWork.Repository<LicenciasCultivos>().Query()
                                .Any(lc => lc.IdCultivo == c.Id && lc.IdLicencia == licenciaId && !lc.Activo)));

                var list = await query.OrderBy(c => c.Nombre).ToListAsync();
                return OperationResult<List<Cultivo>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Cultivos visibles para la licencia");
                return OperationResult<List<Cultivo>>.Failure("Ocurrió un problema al obtener los cultivos visibles.", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets all crops for configuration: global crops + own crops with visibility/active state.
        /// </summary>
        public async Task<OperationResult<List<Cultivo>>> GetAllForLicenseConfigAsync()
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;

                var query = _repository.Query().AsNoTracking()
                    .Where(c => c.IdLicencia == licenciaId
                        || c.IdLicencia == null);

                var list = await query.OrderBy(c => c.Nombre).ToListAsync();
                return OperationResult<List<Cultivo>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Cultivos para configuración");
                return OperationResult<List<Cultivo>>.Failure("Ocurrió un problema al obtener los cultivos.", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Toggles the Activo field of a license-owned crop.
        /// </summary>
        public async Task<OperationResult> ToggleActivoAsync(int idCultivo)
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;
                if (!licenciaId.HasValue)
                    return OperationResult.Failure("No hay una licencia activa.", "NO_LICENSE");

                var cultivo = await _repository.GetAsync(c => c.Id == idCultivo && c.IdLicencia == licenciaId.Value);
                if (cultivo == null)
                    return OperationResult.Failure("El cultivo propio no existe.", "NOT_FOUND");

                cultivo.Activo = !cultivo.Activo;
                cultivo.ModificationDate = TimeHelper.GetArgentinaTime();
                cultivo.ModificationUser = _userContext.UserName;
                await _repository.UpdateAsync(cultivo);
                await _unitOfWork.SaveAsync();

                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al alternar Activo del cultivo propio {IdCultivo}", idCultivo);
                return OperationResult.Failure($"Ocurrió un problema al alternar el estado: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Checks if a global crop is visible for the given license.
        /// Returns true if no LicenciasCultivos entry hides it, or if entry has Activo=true.
        /// </summary>
        public async Task<bool> CheckVisibilityAsync(int idCultivo, int idLicencia)
        {
            try
            {
                var entry = await _unitOfWork.Repository<LicenciasCultivos>().Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(lc => lc.IdLicencia == idLicencia && lc.IdCultivo == idCultivo);

                // No entry means visible by default
                if (entry == null)
                    return true;

                return entry.Activo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar visibilidad del cultivo {IdCultivo} para licencia {IdLicencia}", idCultivo, idLicencia);
                return true; // Default to visible on error
            }
        }

        /// <summary>
        /// Sets visibility of a global crop for the current license.
        /// Creates or updates an entry in LicenciasCultivos side table.
        /// </summary>
        public async Task<OperationResult> SetVisibilityAsync(int idCultivo, bool visible)
        {
            try
            {
                var licenciaId = _userContext.IdLicencia;
                if (!licenciaId.HasValue)
                    return OperationResult.Failure("No hay una licencia activa.", "NO_LICENSE");

                // Verify the crop exists and is a global record
                var cultivo = await _repository.GetAsync(c => c.Id == idCultivo && c.IdLicencia == null);
                if (cultivo == null)
                    return OperationResult.Failure("El cultivo global no existe.", "NOT_FOUND");

                // Check if there's already a visibility entry
                var lcRepo = _unitOfWork.Repository<LicenciasCultivos>();
                var existingEntry = await lcRepo.GetAsync(lc => lc.IdLicencia == licenciaId.Value && lc.IdCultivo == idCultivo);

                if (existingEntry != null)
                {
                    // Update existing entry
                    existingEntry.Activo = visible;
                    existingEntry.ModificationDate = TimeHelper.GetArgentinaTime();
                    existingEntry.ModificationUser = _userContext.UserName;
                    await lcRepo.UpdateAsync(existingEntry);
                }
                else
                {
                    // Create new entry (visible=true means no hiding needed, but we create it anyway for tracking)
                    var newEntry = new LicenciasCultivos
                    {
                        IdLicencia = licenciaId.Value,
                        IdCultivo = idCultivo,
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
                _logger.LogError(ex, "Error al establecer visibilidad del cultivo {IdCultivo}", idCultivo);
                return OperationResult.Failure($"Ocurrió un problema al establecer la visibilidad: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Creates a new crop with automatic order shifting.
        /// If another crop already has the requested orden, shifts existing items up by 1.
        /// Only shifts items visible to this license (global + own).
        /// </summary>
        public async Task<OperationResult<Cultivo>> CreateWithOrderShiftAsync(Cultivo entity)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                // Shift existing items that have orden >= entity.Orden
                var repo = _unitOfWork.Repository<Cultivo>();
                var itemsToShift = await repo.Query()
                    .Where(c => c.Orden >= entity.Orden
                        && (c.IdLicencia == null || c.IdLicencia == _userContext.IdLicencia))
                    .OrderByDescending(c => c.Orden)
                    .ToListAsync();

                foreach (var item in itemsToShift)
                {
                    item.Orden++;
                }

                // Now create the new entity
                var result = await base.CreateAsync(entity);
                if (!result.Success)
                    return result;

                scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cultivo con desplazamiento de orden");
                return OperationResult<Cultivo>.Failure($"Ocurrió un problema al crear el cultivo: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Updates a crop with automatic order re-shifting.
        /// If the orden changed, shifts other items accordingly to maintain ordering integrity.
        /// Only shifts items visible to this license (global + own).
        /// </summary>
        public async Task<OperationResult<Cultivo>> UpdateCultivoAsync(Cultivo entity)
        {
            try
            {
                var repo = _unitOfWork.Repository<Cultivo>();
                var existing = await repo.GetAsync(c => c.Id == entity.Id);
                if (existing == null)
                    return OperationResult<Cultivo>.Failure("El cultivo no existe.", "NOT_FOUND");

                var oldOrden = existing.Orden;
                var newOrden = entity.Orden;

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                // If orden changed, handle shifting
                if (oldOrden != newOrden)
                {
                    if (newOrden < oldOrden)
                    {
                        // Moving up: shift items between newOrden and oldOrden-1 up by 1
                        var itemsToShift = await repo.Query()
                            .Where(c => c.Orden >= newOrden && c.Orden < oldOrden && c.Id != entity.Id
                                && (c.IdLicencia == null || c.IdLicencia == _userContext.IdLicencia))
                            .OrderByDescending(c => c.Orden)
                            .ToListAsync();

                        foreach (var item in itemsToShift)
                        {
                            item.Orden++;
                        }
                    }
                    else
                    {
                        // Moving down: shift items between oldOrden+1 and newOrden down by 1
                        var itemsToShift = await repo.Query()
                            .Where(c => c.Orden > oldOrden && c.Orden <= newOrden && c.Id != entity.Id
                                && (c.IdLicencia == null || c.IdLicencia == _userContext.IdLicencia))
                            .OrderBy(c => c.Orden)
                            .ToListAsync();

                        foreach (var item in itemsToShift)
                        {
                            item.Orden--;
                        }
                    }
                }

                // Update fields on existing entity
                existing.Nombre = entity.Nombre;
                existing.Descripcion = entity.Descripcion;
                existing.Color = entity.Color;
                existing.Orden = newOrden;
                existing.ModificationDate = TimeHelper.GetArgentinaTime();
                existing.ModificationUser = _userContext.UserName;

                await repo.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();

                scope.Complete();

                return OperationResult<Cultivo>.SuccessResult(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cultivo con desplazamiento de orden");
                return OperationResult<Cultivo>.Failure($"Ocurrió un problema al actualizar el cultivo: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
