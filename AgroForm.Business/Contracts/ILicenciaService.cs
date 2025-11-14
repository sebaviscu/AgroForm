using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface ILicenciaService : IServiceBase<Licencia>
    {
        Task<OperationResult> CreatePagarLicencia(PagoLicencia pagoLicencia);
        Task<OperationResult> DeletePagoLicencia(int idPagoLicencia);
    }
}
