using AgroForm.Model;
using AgroForm.Business.Services;

namespace AgroForm.Business.Contracts
{
    public interface ILicenciaService : IServiceBase<Licencia>
    {
        Task<OperationResult> CreatePagarLicencia(PagoLicencia pagoLicencia);
        Task<OperationResult> DeletePagoLicencia(int idPagoLicencia);
        Task<OperationResult<Licencia>> CreateLicenseWithAdminAsync(Licencia licencia, string adminName, string adminEmail, string adminPassword);
    }
}
