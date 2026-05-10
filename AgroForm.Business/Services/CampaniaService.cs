using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class CampaniaService : ServiceBase<Campania>, ICampaniaService
    {
        public CampaniaService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Campania>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<Campania>> CreateAsync(Campania entity)
        {
            try
            {
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<Campania>.Failure(validationResult.ErrorMessage);

                foreach (var item in entity.Lotes)
                {
                    item.IdLicencia = _userContext.IdLicencia;
                }

                return await base.CreateAsync(entity);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar el registro Campania");
                return OperationResult<Campania>.Failure($"Ocurrió un problema al insertar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<Campania>> GetCurrent()
        {
            try
            {
                var campania = await GetQuery().SingleOrDefaultAsync(_ => _.Id == _userContext.IdCampaña);

                if (campania == null)
                {
                    return OperationResult<Campania>.Failure("No existe una Campaña en curso", "NOT_FOUND");
                }

                return OperationResult<Campania>.SuccessResult(campania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recuperar campaña en curso");
                return OperationResult<Campania>.Failure($"Ocurrió un problema al recuperar campaña en curso: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<Campania>> GetCurrentByLicencia(int? idLicencia)
        {
            try
            {
                if (idLicencia == null)
                    return OperationResult<Campania>.Failure("ID de licencia no proporcionado", "BAD_REQUEST");

                var campania = await GetQuery()
                    .Where(_ => _.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso || _.EstadosCampania == EnumClass.EstadosCamapaña.Iniciada)
                    .FirstOrDefaultAsync(_ => _.IdLicencia == idLicencia);

                if (campania == null)
                {
                    return OperationResult<Campania>.Failure("No existe una Campaña en curso para esta licencia", "NOT_FOUND");
                }

                return OperationResult<Campania>.SuccessResult(campania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recuperar campaña en curso por licencia {IdLicencia}", idLicencia);
                return OperationResult<Campania>.Failure($"Ocurrió un problema al recuperar campaña en curso: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public override async Task<OperationResult<Campania>> GetByIdAsync(int id)
        {
            try
            {
                var campania = await GetQuery().Include(_ => _.Lotes).ThenInclude(_ => _.Campo).SingleOrDefaultAsync(_ => _.Id == id);

                if (campania == null)
                {
                    return OperationResult<Campania>.Failure("No se encontró la campaña", "NOT_FOUND");
                }

                return OperationResult<Campania>.SuccessResult(campania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer el registro con ID {Id}", id);
                return OperationResult<Campania>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<bool>> FinalizarCampaña(int id)
        {
            try
            {
                var response = await GetByIdAsync(id);

                if (!response.Success || response.Data == null)
                {
                    return OperationResult<bool>.Failure("No se encontró la campaña", "NOT_FOUND");
                }

                var campania = response.Data;
                campania.EstadosCampania = EnumClass.EstadosCamapaña.Finalizada;
                campania.FechaFin = TimeHelper.GetArgentinaTime();

                await UpdateAsync(campania);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar la campaña con ID {Id}", id);
                return OperationResult<bool>.Failure($"Ocurrió un problema al finalizar la campaña: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
