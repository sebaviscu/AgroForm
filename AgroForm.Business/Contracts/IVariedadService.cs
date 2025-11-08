using AgroForm.Model;
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
    public interface IVariedadService : IServiceBase<Variedad>
    {
        Task<OperationResult<List<Variedad>>> GetByCultivo(int idCultivo);
    }
}
