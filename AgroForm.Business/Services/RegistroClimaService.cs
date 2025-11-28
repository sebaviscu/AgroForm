using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class RegistroClimaService : ServiceBase<RegistroClima>, IRegistroClimaService
    {
        public RegistroClimaService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<RegistroClima>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public override async Task<OperationResult<List<RegistroClima>>> GetAllWithDetailsAsync()
        {
            var campos = await base.GetQuery()
                     .Where(c => c.IdLicencia == _userAuth.IdLicencia)
                     .Include(c => c.Campo)
                     .AsNoTracking()
                     .ToListAsync();

            return OperationResult<List<RegistroClima>>.SuccessResult(campos);
        }

        public async Task<OperationResult<List<RegistroClima>>> GetByCampaniaAsync(int idCampania)
        {
            var campos = await base.GetQuery(). Where(_=>_.IdCampania == idCampania)
                     .Where(c => c.IdLicencia == _userAuth.IdLicencia)
                     .Include(c => c.Campo)
                     .AsNoTracking()
                     .ToListAsync();

            return OperationResult<List<RegistroClima>>.SuccessResult(campos);
        }

        public async Task<OperationResult<List<RegistroClima>>> GetRegistroClimasAsync(int meses = 6, int idCampo = 0)
        {
            try
            {
                var fechaInicio = TimeHelper.GetArgentinaTime().AddMonths(-meses);

                using var context = await _contextFactory.CreateDbContextAsync();

                var query = context.RegistrosClima
                    .Where(_ => _.IdLicencia == _userAuth.IdLicencia && _.IdCampania == _userAuth.IdCampaña)
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
                _logger.LogError(ex, "Error al obtener Registro de lluvia {meses}, {idCampo}", meses, idCampo); 
                return OperationResult<List<RegistroClima>>.Failure($"Error al obtener Registro de lluvia: {ex.Message}", "DATABASE_ERROR");
            }
        }

    }
}
