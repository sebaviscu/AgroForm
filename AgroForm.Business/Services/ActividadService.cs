using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AgroForm.Business.Services
{
    public class ActividadService : ServiceBase<Actividad>, IActividadService
    {
        public ActividadService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Actividad>> logger, IHttpContextAccessor httpContextAccessor) 
            : base(contextFactory, logger, httpContextAccessor)
        {
            
        }

        public async Task<List<Actividad>> GetByCampoIdAsync(List<int> lotesId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Set<Actividad>().AsQueryable();
                if (lotesId.Any())
                {
                    query = query.Where(_ => lotesId.Contains(_.LoteId));
                }
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades por CampoId {CampoId}", lotesId);
                throw;
            }
        }

    }
}
