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
        public ActividadService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ActividadService> logger, IHttpContextAccessor httpContextAccessor) 
            : base(contextFactory, logger, httpContextAccessor)
        {
            
        }

        public async Task<List<Actividad>> GetByidCampoAsync(List<int> lotesId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Set<Actividad>()
                    .Include(a => a.Lote)
                    .ThenInclude(l => l.Campo)
                    .AsNoTracking()
                    .AsQueryable();

                if (lotesId.Any())
                {
                    query = query.Where(_ => lotesId.Contains(_.IdLote));
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades por LotesId {LotesId}", string.Join(",", lotesId));
                throw;
            }
        }

        public override async Task<OperationResult<List<Actividad>>> GetAllWithDetailsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entities = await context.Set<Actividad>()
                    .Include(a => a.Insumo)
                    .Include(a => a.TipoActividad)
                    .Include(a => a.Lote)
                    .ThenInclude(l => l.Campo)
                    .AsNoTracking()
                    .ToListAsync();

                return OperationResult<List<Actividad>>.SuccessResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las actividades con detalles");
                return OperationResult<List<Actividad>>.Failure("Error al obtener actividades");
            }
        }

        public override async Task<OperationResult<Actividad>> GetByIdWithDetailsAsync(long id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.Set<Actividad>()
                    .Include(a => a.Lote)
                    .ThenInclude(l => l.Campo)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null)
                    return OperationResult<Actividad>.Failure("Actividad no encontrada");

                return OperationResult<Actividad>.SuccessResult((Actividad)(object)entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad con id {Id}", id);
                return OperationResult<Actividad>.Failure("Error al obtener actividad");
            }
        }
        public async Task<OperationResult<List<Actividad>>> GetRecentAsync()
        {
            try
            {
                var fechaLimite = TimeHelper.GetArgentinaTime(); // Últimos 7 días

                using var context = await _contextFactory.CreateDbContextAsync();

                var actividades = await context.Actividades
                    .Where(a => a.IdLicencia == _userAuth.IdLicencia)
                    .Include(a => a.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(a => a.TipoActividad)
                    .Include(a => a.Usuario)
                    .Include(a => a.Insumo)
                    .OrderByDescending(a => a.Fecha)
                    .Take(15)
                    .AsNoTracking()
                    .ToListAsync();


                return OperationResult<List<Actividad>>.SuccessResult(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades recientes para licencia {IdLicencia}", _userAuth.IdLicencia);
                return OperationResult<List<Actividad>>.Failure("Error al obtener actividades recientes");
            }
        }

    }
}
