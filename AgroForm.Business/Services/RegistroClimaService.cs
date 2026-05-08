using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class RegistroClimaService : ServiceBase<RegistroClima>, IRegistroClimaService
    {
        public RegistroClimaService(IUnitOfWork unitOfWork, ILogger<ServiceBase<RegistroClima>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<List<RegistroClima>>> GetAllWithDetailsAsync()
        {
            var list = await GetQuery()
                     .Include(c => c.Campo)
                     .AsNoTracking()
                     .ToListAsync();

            return OperationResult<List<RegistroClima>>.SuccessResult(list);
        }

        public async Task<OperationResult<List<RegistroClima>>> GetByCampaniaAsync(int? idCampania)
        {
            var list = await GetQuery()
                     .Where(_ => _.IdCampania == idCampania)
                     .Include(c => c.Campo)
                     .AsNoTracking()
                     .ToListAsync();

            return OperationResult<List<RegistroClima>>.SuccessResult(list);
        }

        public async Task<OperationResult<List<RegistroClima>>> GetRegistroClimasAsync(int meses = 6, int idCampo = 0)
        {
            try
            {
                var fechaInicio = TimeHelper.GetArgentinaTime().AddMonths(-meses);

                var query = GetQuery()
                    .Where(_ => _.IdCampania == _userContext.IdCampaña)
                    .Where(_ => _.Fecha >= fechaInicio)
                    .Where(_ => _.TipoClima == TipoClima.Lluvia || _.TipoClima == TipoClima.Granizo)
                    .AsNoTracking();

                if (idCampo > 0)
                {
                    query = query.Where(rc => rc.IdCampo == idCampo);
                }
                
                var lista = await query.ToListAsync();
                return OperationResult<List<RegistroClima>>.SuccessResult(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Registro de clima {meses}, {idCampo}", meses, idCampo);
                return OperationResult<List<RegistroClima>>.Failure($"Error al obtener Registro de clima: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
