using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class LicenciaService : ServiceBase<Licencia>, ILicenciaService
    {

        public readonly IGenericRepository<PagoLicencia> _pagoLicenciaRepository;

        public LicenciaService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Licencia>> logger, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
            : base(contextFactory, logger, httpContextAccessor)
        {
            _pagoLicenciaRepository = unitOfWork.Repository<PagoLicencia>();

        }

        public override async Task<OperationResult<Licencia>> GetByIdAsync(int id)
        {
            try
            {
                var query = base.GetQuery().Include(_ => _.PagoLicencias).FirstAsync();

                return OperationResult<Licencia>.SuccessResult(await query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer todos los registros con detalles de Licencia");
                return OperationResult<Licencia>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult> CreatePagarLicencia(PagoLicencia pagoLicencia)
        {
            try
            {
                pagoLicencia.RegistrationDate = TimeHelper.GetArgentinaTime();
                pagoLicencia.RegistrationUser = _userAuth.UserName;

                await _pagoLicenciaRepository.AddAsync(pagoLicencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar la licencia");
                return OperationResult.Failure($"Ocurrio un error al pagar la licencia: {ex.Message}", "DATABASE_ERROR");
            }
            return OperationResult.SuccessResult();
        }

        public async Task<OperationResult> DeletePagoLicencia(int idPagoLicencia)
        {
            try
            {
                await _pagoLicenciaRepository.DeleteByIdAsync(idPagoLicencia);

                return OperationResult.SuccessResult();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error al pagar la licencia");
                return OperationResult.Failure($"Ocurrio un error al pagar la licencia: {ex.Message}", "DATABASE_ERROR");
            }
        }

    }
}
