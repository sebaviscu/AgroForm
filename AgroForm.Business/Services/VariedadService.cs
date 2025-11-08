using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class VariedadService : ServiceBase<Variedad>, IVariedadService
    {
        public VariedadService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Variedad>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public async Task<OperationResult<List<Variedad>>> GetByCultivo(int idCultivo)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entities = await context.Set<Variedad>()
                    .AsNoTracking()
                    .Where(_ => _.IdCultivo == idCultivo)
                    .ToListAsync();

                if (!entities.Any())
                    return OperationResult<List<Variedad>>.Failure("Variedad por idCultivo no encontrado");

                return OperationResult<List<Variedad>>.SuccessResult((List<Variedad>)(object)entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Variedads por idCultivo {idCultivo}", idCultivo);
                return OperationResult<List<Variedad>>.Failure("Error al obtener Variedads por idCultivo");
            }
        }

    }
}
