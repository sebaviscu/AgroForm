using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IEstadoFenologicoService : IServiceBase<EstadoFenologico>
    {
        Task<OperationResult<List<EstadoFenologico>>> GetFenologicosByCultivoAsync(int idCultivo);
    }
}
