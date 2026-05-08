using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class CampoService : ServiceBase<Campo>, ICampoService
    {
        public CampoService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Campo>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<List<Campo>>> GetAllWithDetailsAsync()
        {
            try
            {
                // El filtro de IdLicencia es global. Aquí solo aplicamos los detalles específicos.
                var campos = await GetQuery()
                    .Include(c => c.Lotes.Where(l => l.IdCampania == _userContext.IdCampaña))
                    .AsNoTracking()
                    .ToListAsync();

                var camposFiltrados = campos.Where(c => c.Lotes.Any()).ToList();

                return OperationResult<List<Campo>>.SuccessResult(camposFiltrados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros con detalles de Campo");
                return OperationResult<List<Campo>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public override async Task<OperationResult<Campo>> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                var entity = await GetQuery()
                    .Include(c => c.Lotes.Where(l => l.IdCampania == _userContext.IdCampaña))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return OperationResult<Campo>.Failure("No se encontró el registro", "NOT_FOUND");

                return OperationResult<Campo>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con detalles con ID {Id}", id);
                return OperationResult<Campo>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<Campo>> GetHistorialByIdAsync(int id)
        {
            try
            {
                var entity = await GetQuery()
                    .Include(c => c.Lotes)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return OperationResult<Campo>.Failure("No se encontró el registro", "NOT_FOUND");

                return OperationResult<Campo>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el historial del campo con ID {Id}", id);
                return OperationResult<Campo>.Failure($"Ocurrió un problema al leer el historial: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
