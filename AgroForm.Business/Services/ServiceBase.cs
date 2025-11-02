using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AlbaServicios.Services
{
    public class ServiceBase<T> : IServiceBase<T> where T : EntityBase
    {
        protected readonly IDbContextFactory<AppDbContext> _contextFactory;
        protected readonly ILogger<ServiceBase<T>> _logger;
        protected UserAuth _userAuth;

        public ServiceBase(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<T>> logger, IHttpContextAccessor _httpContextAccessor)
        {
            _contextFactory = contextFactory;
            _logger = logger;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                var claimUser = httpContext.User;

                _userAuth = new UserAuth
                {
                    UserName = GetClaimValue<string>(claimUser, ClaimTypes.Name),
                    IdLicencia = GetClaimValue<int>(claimUser, "Licencia"),
                    IdCampaña = GetClaimValue<int>(claimUser, "Campania"),
                    IdUsuario = GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                    IdRol = GetClaimValue<Roles>(claimUser, ClaimTypes.Role)
                };
            }
        }

        protected T GetClaimValue<T>(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim == null)
                return default;

            try
            {
                if (typeof(T).IsEnum)
                {
                    if (Enum.TryParse(typeof(T), claim.Value, out var result))
                    {
                        return (T)result;
                    }
                    return default;
                }

                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public virtual async Task<OperationResult<List<T>>> GetAllAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                IQueryable<T> query = context.Set<T>().AsNoTracking();

                if (typeof(EntityBaseWithLicencia).IsAssignableFrom(typeof(T)))
                {
                    query = query.Where(e => EF.Property<int>(e, "IdLicencia") == _userAuth.IdLicencia);
                }

                var list = await query.ToListAsync();

                return OperationResult<List<T>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros de {TypeName}", typeof(T).Name);
                return OperationResult<List<T>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }


        public virtual async Task<OperationResult<List<T>>> GetAllWithDetailsAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                IQueryable<T> query = context.Set<T>().AsNoTracking();

                if (typeof(EntityBaseWithLicencia).IsAssignableFrom(typeof(T)))
                {
                    query = query.Where(e => EF.Property<int>(e, "IdLicencia") == _userAuth.IdLicencia);
                }
                var list = await query.ToListAsync();

                return OperationResult<List<T>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros con detalles de {TypeName}", typeof(T).Name);
                return OperationResult<List<T>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<T>> GetByIdAsync(long id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.Set<T>()
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(x => x.Id == id);

                //if (entity == null)
                //    return OperationResult<T>.Failure($"No se encontró el registro con ID {id}.", "NOT_FOUND");

                return OperationResult<T>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con ID {Id}", id);
                return OperationResult<T>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<T>> GetByIdWithDetailsAsync(long id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.Set<T>()
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(x => x.Id == id);

                return OperationResult<T>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con detalles con ID {Id}", id);
                return OperationResult<T>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<T>> CreateAsync(T entity)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<T>.Failure(validationResult.ErrorMessage);

                if (entity is EntityBaseWithLicencia entidadConLicencia)
                    entidadConLicencia.IdLicencia = _userAuth.IdLicencia;

                entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                entity.ModificationDate = null;
                entity.RegistrationUser = _userAuth.UserName;

                context.Set<T>().Add(entity);
                int result = await context.SaveChangesAsync();

                if (result > 0)
                    return OperationResult<T>.SuccessResult(entity);

                return OperationResult<T>.Failure("No se pudo insertar el registro en la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al insertar el registro {typeof(T)}");
                return OperationResult<T>.Failure($"Ocurrió un problema al insertar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult<List<T>>> CreateRangeAsync(List<T> entityList)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                foreach (var entity in entityList)
                {
                    var validationResult = await ValidateAsync(entity);
                    if (!validationResult.Success)
                        return OperationResult<List<T>>.Failure($"Error de validacion en el registro: {validationResult.ErrorMessage}", "VALIDATION_ERROR");

                    if (entity is EntityBaseWithLicencia entidadConLicencia)
                        entidadConLicencia.IdLicencia = _userAuth.IdLicencia;

                    entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                    entity.ModificationDate = null;
                    entity.RegistrationUser = _userAuth.UserName;
                }

                context.Set<T>().AddRange(entityList);
                int result = await context.SaveChangesAsync();

                if (result == entityList.Count)
                    return OperationResult<List<T>>.SuccessResult(entityList);

                return OperationResult<List<T>>.Failure($"Solo se insertaron {result} de {entityList.Count} registros.", "PARTIAL_SAVE");
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
                await using var context = await _contextFactory.CreateDbContextAsync();

                var original = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == entity.Id);
                if (original == null)
                    return OperationResult<T>.Failure("El registro que intenta actualizar no existe.", "NOT_FOUND");

                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<T>.Failure(validationResult.ErrorMessage, "VALIDATION_ERROR");

                if (entity is EntityBaseWithLicencia entidadConLicencia && original is EntityBaseWithLicencia originalConLicencia)
                    entidadConLicencia.IdLicencia = originalConLicencia.IdLicencia;

                context.Entry(original).CurrentValues.SetValues(entity);

                original.ModificationDate = TimeHelper.GetArgentinaTime();
                original.ModificationUser = _userAuth.UserName;

                int result = await context.SaveChangesAsync();

                if (result > 0)
                    return OperationResult<T>.SuccessResult(original);

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
                await using var context = await _contextFactory.CreateDbContextAsync();

                foreach (var entity in entityList)
                {
                    var original = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == entity.Id);
                    if (original == null)
                        return OperationResult<List<T>>.Failure($"El registro con ID {entity.Id} que intenta actualizar no existe.", "NOT_FOUND");

                    var validationResult = await ValidateAsync(entity);
                    if (!validationResult.Success)
                        return OperationResult<List<T>>.Failure($"Error de validacion en el registro con ID {entity.Id}: {validationResult.ErrorMessage}", "VALIDATION_ERROR");

                    if (entity is EntityBaseWithLicencia entidadConLicencia && original is EntityBaseWithLicencia originalConLicencia)
                        entidadConLicencia.IdLicencia = originalConLicencia.IdLicencia;

                    context.Entry(original).CurrentValues.SetValues(entity);

                    original.ModificationDate = TimeHelper.GetArgentinaTime();
                    original.ModificationUser = _userAuth.UserName;
                }

                int result = await context.SaveChangesAsync();

                if (result == entityList.Count)
                    return OperationResult<List<T>>.SuccessResult(entityList);

                return OperationResult<List<T>>.Failure($"Solo se actualizaron {result} de {entityList.Count} registros.", "PARTIAL_SAVE");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar {Count} registros", entityList.Count);
                return OperationResult<List<T>>.Failure($"Ocurrio un problema al actualizar los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }


        public virtual async Task<OperationResult> DeleteAsync(long id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return OperationResult.Failure("El registro que intenta eliminar no existe.", "NOT_FOUND");

                context.Set<T>().Remove(entity);
                int result = await context.SaveChangesAsync();

                return result > 0 ? OperationResult.SuccessResult() : OperationResult.Failure("No se pudo eliminar el registro de la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el registro con ID {Id}", id);
                return OperationResult.Failure($"Ocurrió un problema al eliminar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<OperationResult> DeleteRangeAsync(List<long> ids)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var idList = ids.ToList();

                var entities = await context.Set<T>().Where(x => idList.Contains(x.Id)).ToListAsync();
                context.Set<T>().RemoveRange(entities);
                int result = await context.SaveChangesAsync();

                return result == idList.Count ? OperationResult.SuccessResult() : OperationResult.Failure($"Solo se eliminaron {result} de {idList.Count} registros.", "PARTIAL_DELETE");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar {Count} registros", ids.Count());
                return OperationResult.Failure($"Ocurrió un problema al eliminar los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public virtual async Task<bool> ExistsAsync(long id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>().AnyAsync(x => x.Id == id);
        }

        public virtual async Task<OperationResult> ValidateAsync(T entity)
        {
            return OperationResult.SuccessResult();
        }

        public virtual IQueryable<T> GetQuery()
        {
            var context = _contextFactory.CreateDbContext();
            return context.Set<T>().AsQueryable();
        }
    }


    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static OperationResult<T> SuccessResult(T data) => new() { Success = true, Data = data };
        public static OperationResult<T> Failure(string errorMessage, string errorCode = "") => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static OperationResult SuccessResult() => new() { Success = true };
        public static OperationResult Failure(string errorMessage, string errorCode = "") => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}