using AgroForm.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IDolarApiService
    {
        Task<List<DolarInfo>> ObtenerDolaresAsync();
    }
}
