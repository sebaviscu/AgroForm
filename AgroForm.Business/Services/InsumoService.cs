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
    internal class InsumoService : ServiceBase<Insumo>, IInsumoService
    {
        public InsumoService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Insumo>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public async Task<OperationResult<List<Insumo>>> GetByTipoInsumo(int idTipoInsumo)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Insumos.Where(_=>_.IdTipoInsumo == idTipoInsumo)
                    .AsNoTracking()
                    .AsQueryable();

                var list = await query.ToListAsync();
                return OperationResult<List<Insumo>>.SuccessResult(list);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener insumos por Tipo de Insumo {idTipoInsumo}", idTipoInsumo);
                return OperationResult<List<Insumo>>.Failure($"\"Error al obtener insumos por Tipo de Insumo {idTipoInsumo}\": {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
