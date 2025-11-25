using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class CampoService : ServiceBase<Campo>, ICampoService
    {

        public CampoService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Campo>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public override async Task<OperationResult<List<Campo>>> GetAllWithDetailsAsync()
        {
            var campos = await base.GetQuery()
                .Where(c => c.IdLicencia == _userAuth.IdLicencia)
                .Include(c => c.Lotes
                    .Where(l => l.IdCampania == _userAuth.IdCampaña))
                .AsNoTracking()
                .ToListAsync();

            return OperationResult<List<Campo>>.SuccessResult(campos);
        }

        public override async Task<OperationResult<Campo?>> GetByIdWithDetailsAsync(long id)
        {
            try
            {
                var entity = await base.GetQuery()
                    .Include(c => c.Lotes
                    .Where(l => l.IdCampania == _userAuth.IdCampaña))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                return OperationResult<Campo?>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con detalles con ID {Id}", id);
                return OperationResult<Campo?>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
