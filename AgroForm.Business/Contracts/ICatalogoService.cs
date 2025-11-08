using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Contracts
{
    public interface ICatalogoService : IServiceBase<Catalogo>
    {

        Task<OperationResult<List<Catalogo>>> GetByType(TipoCatalogo tipo);
    }
}
