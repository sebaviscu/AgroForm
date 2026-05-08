using AgroForm.Model;
using AgroForm.Business.Services;
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
        Task<OperationResult<Licencia>> CreateLicenseWithAdminAsync(Licencia licencia, string adminName, string adminEmail, string adminPassword);
    }
}
