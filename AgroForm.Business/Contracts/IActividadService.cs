using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IActividadService : IServiceBase<Actividad>
    {
        Task<List<Actividad>> GetByidCampoAsync(List<int> lotesId);

        Task<OperationResult<List<Actividad>>> GetRecentAsync();
    }
}
