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
    internal class CampaniaService : ServiceBase<Campania>, ICampaniaService
    {
        public CampaniaService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Campania>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {

        }
    }
}
