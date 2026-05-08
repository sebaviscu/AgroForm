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
    public class AjusteService : ServiceBase<Ajuste>, IAjusteService
    {
        public AjusteService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Ajuste>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }
    }
}
