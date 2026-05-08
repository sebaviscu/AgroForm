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
    }
}
