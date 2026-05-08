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

namespace AgroForm.Business.Services
{
    public class LoteService : ServiceBase<Lote>, ILoteService
    {
        public LoteService(IUnitOfWork unitOfWork, ILogger<LoteService> logger, IUserContext userContext)
           : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<List<Lote>>> GetAllWithDetailsAsync()
        {
            try
            {
                var list = await GetQuery()
                    .Where(e => e.IdCampania == _userContext.IdCampaña)
                    .Include(_ => _.Campo)
                    .Include(_ => _.Cosechas)
                    .Include(_ => _.Siembras)
                        .ThenInclude(_ => _.Cultivo)
                    .AsNoTracking()
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
            var query = GetQuery()
                .Include(_ => _.Campo)
                .Where(_ => _.IdCampo == idCampo && _.IdCampania == _userContext.IdCampaña);
            
            var list = await query.ToListAsync();
            return OperationResult<List<Lote>>.SuccessResult(list);
        }

        public async Task<OperationResult<List<Lote>>> GetByIds(List<int> idsLotes)
        {
            var query = GetQuery()
                .Include(_ => _.Campo)
                .Where(_ => idsLotes.Contains(_.Id) && _.IdCampania == _userContext.IdCampaña);

            var list = await query.ToListAsync();
            return OperationResult<List<Lote>>.SuccessResult(list);
        }
    }
}
