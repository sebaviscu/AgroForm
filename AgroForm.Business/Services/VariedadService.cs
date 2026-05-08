using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class VariedadService : ServiceBase<Variedad>, IVariedadService
    {
        public VariedadService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Variedad>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<List<Variedad>>> GetAllAsync()
        {
            try
            {
                var list = await _repository.Query()
                    .Include(v => v.Cultivo)
                    .ToListAsync();
                return OperationResult<List<Variedad>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer variedades");
                return OperationResult<List<Variedad>>.Failure("Error al leer variedades");
            }
        }

        public override async Task<OperationResult<List<Variedad>>> GetAllWithDetailsAsync()
        {
            try
            {
                var list = await _repository.Query()
                    .Include(v => v.Cultivo)
                    .ToListAsync();
                return OperationResult<List<Variedad>>.SuccessResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer variedades con cultivo");
                return OperationResult<List<Variedad>>.Failure("Error al leer variedades");
            }
        }

        public async Task<OperationResult<List<Variedad>>> GetByCultivo(int idCultivo)
        {
            try
            {
                var entities = await _repository.GetAllAsync(e => e.IdCultivo == idCultivo);

                if (!entities.Any())
                    return OperationResult<List<Variedad>>.Failure("Variedad por idCultivo no encontrado");

                return OperationResult<List<Variedad>>.SuccessResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Variedades por idCultivo {idCultivo}", idCultivo);
                return OperationResult<List<Variedad>>.Failure("Error al obtener Variedades por idCultivo");
            }
        }
    }
}
