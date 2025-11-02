using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IInsumoService : IServiceBase<Insumo>
    {
        Task<OperationResult<List<Insumo>>> GetByTipoInsumo(int idTipoInsumo);

    }
}
