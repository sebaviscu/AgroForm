using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class ServiceBase<T> : IServiceBase<T> where T : EntityBase
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger _logger;
        protected readonly IUserContext _userContext;
        protected readonly IGenericRepository<T> _repository;

        public ServiceBase(IUnitOfWork unitOfWork, ILogger logger, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userContext = userContext;
            _repository = _unitOfWork.Repository<T>();
        }

        public virtual async Task<OperationResult<List<T>>> GetAllAsync()
        {
            try
            {
                var list = await _repository.GetAllAsync();
                return OperationResult<List<T>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros de {TypeName}", typeof(T).Name);
                return OperationResult<List<T>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<List<T>>> GetAllByCamapniaAsync()
        {
            // Nota: El filtro de IdLicencia ya es global en el DbContext.
            // Si se requiere filtrar por campaña específicamente (y no es global), se hace aquí.
            try
            {
                IQueryable<T> query = _repository.Query().AsNoTracking();

                if (typeof(IEntityBaseWithCampania).IsAssignableFrom(typeof(T)))
                {
                    query = query.Where(e => EF.Property<int>(e, "IdCampania") == _userContext.IdCampaña);
                }

                var list = await query.ToListAsync();
                return OperationResult<List<T>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer los registros de {TypeName} por campaña", typeof(T).Name);
                return OperationResult<List<T>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<List<T>>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync();
        }

        public virtual async Task<OperationResult<T>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetAsync(e => e.Id == id);

                if (entity == null)
                    return OperationResult<T>.Failure("No se encontró el registro", "NOT_FOUND");

                return OperationResult<T>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con ID {Id}", id);
                return OperationResult<T>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<T>> GetByIdWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public virtual async Task<OperationResult<T>> CreateAsync(T entity)
        {
            try
            {
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<T>.Failure(validationResult.ErrorMessage);

                // Multi-tenancy automático para IdLicencia vía Global Filter y SaveChanges
                // Asignamos explícitamente los campos que NO son automáticos o que dependen de la lógica
                if (entity is EntityBaseWithLicencia entidadConLicencia)
                    entidadConLicencia.IdLicencia = _userContext.IdLicencia;

                if (entity is IEntityBaseWithCampania entidadConCampania && _userContext.IdCampaña.HasValue)
                    entidadConCampania.IdCampania = _userContext.IdCampaña.Value;

                if (entity is IEntityBaseWithMoneda entidadConMoneda)
                    entidadConMoneda.IdMoneda = (int)_userContext.User.Moneda;

                await _repository.AddAsync(entity);
                int result = await _unitOfWork.SaveAsync();

                if (result > 0)
                    return OperationResult<T>.SuccessResult(entity);

                return OperationResult<T>.Failure("No se pudo insertar el registro en la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar el registro {TypeName}", typeof(T).Name);
                return OperationResult<T>.Failure($"Ocurrió un problema al insertar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<List<T>>> CreateRangeAsync(List<T> entityList)
        {
            try
            {
                foreach (var entity in entityList)
                {
                    var validationResult = await ValidateAsync(entity);
                    if (!validationResult.Success)
                        return OperationResult<List<T>>.Failure($"Error de validacion en el registro: {validationResult.ErrorMessage}", "VALIDATION_ERROR");

                    if (entity is EntityBaseWithLicencia entidadConLicencia)
                        entidadConLicencia.IdLicencia = _userContext.IdLicencia;

                    if (entity is IEntityBaseWithCampania entidadConCampania && _userContext.IdCampaña.HasValue)
                        entidadConCampania.IdCampania = _userContext.IdCampaña.Value;
                }

                await _repository.AddRangeAsync(entityList);
                int result = await _unitOfWork.SaveAsync();

                if (result > 0)
                    return OperationResult<List<T>>.SuccessResult(entityList);

                return OperationResult<List<T>>.Failure("No se pudieron insertar los registros.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar {Count} registros", entityList.Count);
                return OperationResult<List<T>>.Failure($"Ocurrio un problema al insertar los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<T>> UpdateAsync(T entity)
        {
            try
            {
                var original = await _repository.GetAsync(x => x.Id == entity.Id);
                if (original == null)
                    return OperationResult<T>.Failure("El registro que intenta actualizar no existe.", "NOT_FOUND");

                if (entity is EntityBaseWithLicencia entidadConLicencia)
                    entidadConLicencia.IdLicencia = _userContext.IdLicencia;

                if (entity is IEntityBaseWithCampania entidadConCampania && _userContext.IdCampaña.HasValue)
                    entidadConCampania.IdCampania = _userContext.IdCampaña.Value;

                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<T>.Failure(validationResult.ErrorMessage, "VALIDATION_ERROR");

                // El context tracker se encarga de los cambios. Usamos el repositorio para marcarlo como modificado.
                await _repository.UpdateAsync(entity);
                int result = await _unitOfWork.SaveAsync();

                if (result > 0)
                    return OperationResult<T>.SuccessResult(entity);

                return OperationResult<T>.Failure("No se pudo actualizar el registro en la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el registro con ID {Id}", entity.Id);
                return OperationResult<T>.Failure($"Ocurrio un problema al actualizar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<List<T>>> UpdateRangeAsync(List<T> entityList)
        {
            try
            {
                await _repository.UpdateRangeAsync(entityList);
                int result = await _unitOfWork.SaveAsync();

                if (result > 0)
                    return OperationResult<List<T>>.SuccessResult(entityList);

                return OperationResult<List<T>>.Failure("No se pudieron actualizar los registros.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar {Count} registros", entityList.Count);
                return OperationResult<List<T>>.Failure($"Ocurrio un problema al actualizar los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await _repository.GetAsync(x => x.Id == id);

                if (entity == null)
                    return OperationResult.Failure("El registro que intenta eliminar no existe.", "NOT_FOUND");

                await _repository.DeleteAsync(entity);
                int result = await _unitOfWork.SaveAsync();

                return result > 0 ? OperationResult.SuccessResult() : OperationResult.Failure("No se pudo eliminar el registro de la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el registro con ID {Id}", id);
                return OperationResult.Failure($"Ocurrió un problema al eliminar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult> DeleteRangeAsync(List<int> ids)
        {
            try
            {
                var entities = await _repository.GetAllAsync(x => ids.Contains(x.Id));
                await _repository.DeleteRangeAsync(entities);
                int result = await _unitOfWork.SaveAsync();

                return result > 0 ? OperationResult.SuccessResult() : OperationResult.Failure("No se pudieron eliminar los registros.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar los registros");
                return OperationResult.Failure($"Ocurrió un problema al eliminar los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await _repository.GetAsync(x => x.Id == id);
            return entity != null;
        }

        public virtual async Task<OperationResult> ValidateAsync(T entity)
        {
            return await Task.FromResult(OperationResult.SuccessResult());
        }

        public virtual IQueryable<T> GetQuery()
        {
            return _repository.Query();
        }
    }

    public abstract class ClaimBase
    {
    }
        

    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static OperationResult<T> SuccessResult(T data) => new() { Success = true, Data = data };
        public static OperationResult<T> Failure(string errorMessage, string errorCode = "") => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };

        public override string ToString()
        {
            var result = $"OperationResult<{typeof(T).Name}>: Success={Success}";
            
            if (!string.IsNullOrEmpty(ErrorMessage))
                result += $", ErrorMessage=\"{ErrorMessage}\"";
            
            if (!string.IsNullOrEmpty(ErrorCode))
                result += $", ErrorCode=\"{ErrorCode}\"";
            
            if (Data != null)
                result += $", Data={Data}";
            
            return result;
        }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static OperationResult SuccessResult() => new() { Success = true };
        public static OperationResult Failure(string errorMessage, string errorCode = "") => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };

        public override string ToString()
        {
            var result = $"OperationResult: Success={Success}";
            
            if (!string.IsNullOrEmpty(ErrorMessage))
                result += $", ErrorMessage=\"{ErrorMessage}\"";
            
            if (!string.IsNullOrEmpty(ErrorCode))
                result += $", ErrorCode=\"{ErrorCode}\"";
            
            return result;
        }
    }
}