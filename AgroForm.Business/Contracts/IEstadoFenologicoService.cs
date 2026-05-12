using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
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

        /// <summary>
        /// Toggles the Activo field of a phenological state.
        /// </summary>
        Task<OperationResult> ToggleActivoAsync(int id);

        /// <summary>
        /// Creates a phenological state with validation that the crop exists and belongs to the current license.
        /// </summary>
        Task<OperationResult<EstadoFenologico>> CreateWithValidationAsync(EstadoFenologico entity);
    }
}
