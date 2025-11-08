using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class EstadoFenologicoService : ServiceBase<EstadoFenologico>, IEstadoFenologicoService
    {
        public EstadoFenologicoService(IDbContextFactory<AppDbContext> contextFactory, ILogger<EstadoFenologicoService> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {

        }
    }
}
