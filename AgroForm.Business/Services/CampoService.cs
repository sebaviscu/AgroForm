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
            var campos =  await base.GetQuery()
                     .Where(c => c.IdLicencia == _userAuth.IdLicencia)
                     .Include(c => c.Lotes)
                     .AsNoTracking()
                     .ToListAsync();

            return OperationResult<List<Campo>>.SuccessResult(campos);
        }

        public async Task<OperationResult<List<Campo>>> GetCamposConLotesYActividades()
        {
            try
            {
                var campos = await base.GetQuery()
                         .Where(c => c.IdLicencia == _userAuth.IdLicencia)
                         .Include(c => c.Lotes)
                         .AsNoTracking()
                         .ToListAsync();

                // Cargar actividades por separado
                var lotesIds = campos.SelectMany(c => c.Lotes).Select(l => l.Id).ToList();

                using var context = await _contextFactory.CreateDbContextAsync();
                //var actividades = context.Set<Actividad>()
                //    .Where(a => lotesIds.Contains(a.IdLote))
                //    .Include(a => a.TipoActividad)
                //    .Include(a => a.Insumo)
                //    .AsNoTracking();

                // Asignar actividades a sus lotes manualmente
                foreach (var campo in campos)
                {
                    foreach (var lote in campo.Lotes)
                    {
                        //lote.Actividades = actividades.Where(a => a.IdLote == lote.Id).ToList();
                    }
                }

                return OperationResult<List<Campo>>.SuccessResult(campos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetCamposConLotesYActividades");
                return OperationResult<List<Campo>>.Failure($"Ocurrió un problema al recuperar actividades de lotes: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
