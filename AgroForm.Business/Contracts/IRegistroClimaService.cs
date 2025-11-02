using AgroForm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IRegistroClimaService : IServiceBase<RegistroClima>
    {
        Task<List<RegistroClima>> GetRegistroClimasAsync(int meses = 6, int idCampo = 0);
    }
}
