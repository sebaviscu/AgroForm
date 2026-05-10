using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class LicenciaService : ServiceBase<Licencia>, ILicenciaService
    {

        public readonly IGenericRepository<PagoLicencia> _pagoLicenciaRepository;

        public LicenciaService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Licencia>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
            _pagoLicenciaRepository = _unitOfWork.Repository<PagoLicencia>();
        }

        public override async Task<OperationResult<Licencia>> GetByIdAsync(int id)
        {
            try
            {
                var query = GetQuery().Include(_ => _.PagoLicencias).FirstOrDefaultAsync(l => l.Id == id);
                var licencia = await query;

                if (licencia == null)
                    return OperationResult<Licencia>.Failure("No se encontró la licencia", "NOT_FOUND");

                return OperationResult<Licencia>.SuccessResult(licencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer la licencia con ID {Id}", id);
                return OperationResult<Licencia>.Failure($"Ocurrió un problema al leer la licencia: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult> CreatePagarLicencia(PagoLicencia pagoLicencia)
        {
            try
            {
                await _pagoLicenciaRepository.AddAsync(pagoLicencia);
                await _unitOfWork.SaveAsync();
                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar la licencia");
                return OperationResult.Failure($"Ocurrio un error al pagar la licencia: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult> DeletePagoLicencia(int idPagoLicencia)
        {
            try
            {
                await _pagoLicenciaRepository.DeleteByIdAsync(idPagoLicencia);
                await _unitOfWork.SaveAsync();
                return OperationResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el pago de la licencia");
                return OperationResult.Failure($"Ocurrio un error al eliminar el pago: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<Licencia>> CreateLicenseWithAdminAsync(Licencia licencia, string adminName, string adminEmail, string adminPassword)
        {
            try
            {
                // 1. Validar si ya existe un usuario con ese email
                var userRepo = _unitOfWork.Repository<Usuario>();
                var existingUser = await userRepo.Query().FirstOrDefaultAsync(u => u.Email == adminEmail);
                if (existingUser != null)
                {
                    return OperationResult<Licencia>.Failure("El email del administrador ya está en uso.", "EMAIL_ALREADY_EXISTS");
                }

                // 2. Crear la licencia
                await _repository.AddAsync(licencia);
                await _unitOfWork.SaveAsync(); // Guardamos para obtener el Id de la licencia

                // 3. Crear el usuario administrador asociado
                UsuarioService.CreatePasswordHash(adminPassword, out string passwordHash, out byte[] passwordSalt);

                var user = new Usuario
                {
                    IdLicencia = licencia.Id,
                    Nombre = adminName,
                    Email = adminEmail,
                    Rol = Roles.Administrador,
                    Activo = true,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    EmailConfirmed = true,
                    IdMonedaReferencia = (int)Monedas.DolarOficial
                };

                await userRepo.AddAsync(user);
                await _unitOfWork.SaveAsync();

                return OperationResult<Licencia>.SuccessResult(licencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la licencia con su administrador");
                return OperationResult<Licencia>.Failure($"Error al crear la licencia: {ex.Message}", "DATABASE_ERROR");
            }
        }

    }
}
