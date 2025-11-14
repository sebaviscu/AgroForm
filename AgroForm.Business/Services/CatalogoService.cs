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
    public class CatalogoService : ServiceBase<Catalogo>, ICatalogoService
    {
        public CatalogoService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CatalogoService> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {

        }

        public async Task<OperationResult<List<Catalogo>>> GetByType(TipoCatalogoEnum tipo)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entities = await context.Set<Catalogo>()
                    .AsNoTracking()
                    .Where(_=>_.Tipo == tipo)
                    .ToListAsync();

                if (!entities.Any())
                    return OperationResult<List<Catalogo>>.Failure("Catalogo por tipo no encontrados");

                return OperationResult<List<Catalogo>>.SuccessResult((List<Catalogo>)(object)entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catalogos por tipo {tipo}",tipo);
                return OperationResult<List<Catalogo>>.Failure("Error al obtener Catalogos por tipo");
            }
        }
    }
}
