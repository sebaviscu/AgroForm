using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class EstadoFenologicoService : ServiceBase<EstadoFenologico>, IEstadoFenologicoService
    {
        public EstadoFenologicoService(IUnitOfWork unitOfWork, ILogger<EstadoFenologicoService> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public async Task<OperationResult<List<EstadoFenologico>>> GetFenologicosByCultivoAsync(int idCultivo)
        {
            try
            {
                var list = await base.GetQuery().Where(_ => _.IdCultivo == idCultivo).ToListAsync();

                return OperationResult<List<EstadoFenologico>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Fenologicos por Cultivo");
                return OperationResult<List<EstadoFenologico>>.Failure("Error al obtener Fenologicos por Cultivo");
            }
        }

        public async Task<OperationResult> ToggleActivoAsync(int id)
        {
            try
            {
                var estado = await _repository.GetAsync(e => e.Id == id);
                if (estado == null)
                    return OperationResult.Failure("El estado fenológico no existe.", "NOT_FOUND");

                estado.Activo = !estado.Activo;
                estado.ModificationDate = TimeHelper.GetArgentinaTime();
                estado.ModificationUser = _userContext.UserName;
                await _repository.UpdateAsync(estado);
                await _unitOfWork.SaveAsync();

                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al alternar Activo del estado fenológico {Id}", id);
                return OperationResult.Failure($"Ocurrió un problema al alternar el estado: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<EstadoFenologico>> CreateWithValidationAsync(EstadoFenologico entity)
        {
            try
            {
                // Verify the crop exists and belongs to the current license (or is global)
                var cultivoRepo = _unitOfWork.Repository<Cultivo>();
                var cultivo = await cultivoRepo.GetAsync(c => c.Id == entity.IdCultivo
                    && (c.IdLicencia == null || c.IdLicencia == _userContext.IdLicencia));

                if (cultivo == null)
                    return OperationResult<EstadoFenologico>.Failure("El cultivo no existe o no pertenece a su licencia.", "VALIDATION_ERROR");

                entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                entity.RegistrationUser = _userContext.UserName;

                await _repository.AddAsync(entity);
                await _unitOfWork.SaveAsync();

                return OperationResult<EstadoFenologico>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear estado fenológico");
                return OperationResult<EstadoFenologico>.Failure($"Ocurrió un problema al crear el estado fenológico: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
