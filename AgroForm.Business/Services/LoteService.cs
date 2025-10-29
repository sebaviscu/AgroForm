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

        public Task<List<Lote>> GetByCampoIdAsync(int campoId)
        {
            var query = GetQuery().AsQueryable().Where(_=>_.CampoId == campoId);
            return query.ToListAsync();
        }
    }
}
