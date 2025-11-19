using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Services
{
    public class LoteService : ServiceBase<Lote>, ILoteService
    {
        public LoteService(IDbContextFactory<AppDbContext> contextFactory, ILogger<LoteService> logger, IHttpContextAccessor httpContextAccessor)
           : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public async override Task<OperationResult<List<Lote>>> GetAllWithDetailsAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                IQueryable<Lote> query = context.Lotes.AsNoTracking();

                query = query.Where(e => e.IdLicencia == _userAuth.IdLicencia);
                var list = await query.Include(_ => _.Campo)
                                        .Include(_ => _.Cosechas)
                                        .Include(_ => _.Siembras)
                                            .ThenInclude(_=>_.Cultivo)
                                        .ToListAsync();

                return OperationResult<List<Lote>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros con detalles de Lotes");
                return OperationResult<List<Lote>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<List<Lote>>> GetByIdCampoAsync(int idCampo)
        {
            var query = GetQuery().Include(_=>_.Campo).AsQueryable().Where(_=>_.IdCampo == idCampo && _.IdCampania == _userAuth.IdCampaña);
            var list = await query.ToListAsync();
            return OperationResult<List<Lote>>.SuccessResult(list);
        }

        public async Task<OperationResult<List<Lote>>> GetByIds(List<int> idsLotes)
        {
            var query = GetQuery().Include(_=>_.Campo).AsQueryable().Where(_=> idsLotes.Contains(_.Id) && _.IdCampania == _userAuth.IdCampaña);
            var list = await query.ToListAsync();
            return OperationResult<List<Lote>>.SuccessResult(list);
        }
    }
}
