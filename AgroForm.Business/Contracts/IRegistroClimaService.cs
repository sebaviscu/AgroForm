using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IRegistroClimaService : IServiceBase<RegistroClima>
    {
        Task<OperationResult<List<RegistroClima>>> GetByCampaniaAsync(int idCampania);
        Task<OperationResult<List<RegistroClima>>> GetRegistroClimasAsync(int meses = 6, int idCampo = 0);
    }
}
