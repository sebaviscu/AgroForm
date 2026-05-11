using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
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
                    .Where(_ => _.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso || _.EstadosCampania == EnumClass.EstadosCamapaña.Planificada)
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

                await CerrarCiclosActivosDeCampania(id);

                await UpdateAsync(campania);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar la campaña con ID {Id}", id);
                return OperationResult<bool>.Failure($"Ocurrió un problema al finalizar la campaña: {ex.Message}", "DATABASE_ERROR");
            }
        }

        private async Task CerrarCiclosActivosDeCampania(int idCampania)
        {
            try
            {
                var ciclosActivos = await _unitOfWork.Repository<CicloCultivo>()
                    .Query()
                    .Where(c => c.IdCampania == idCampania && c.FechaFin == null)
                    .ToListAsync();

                var fechaCierre = TimeHelper.GetArgentinaTime();

                foreach (var ciclo in ciclosActivos)
                {
                    ciclo.FechaFin = fechaCierre;
                    await _unitOfWork.Repository<CicloCultivo>().UpdateAsync(ciclo);
                }

                _logger.LogInformation("Se cerraron {Count} ciclos activos de la campaña {IdCampania}", 
                    ciclosActivos.Count, idCampania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar ciclos activos de la campaña {IdCampania}", idCampania);
            }
        }

        public async Task<OperationResult<Campania>> IniciarCampania(int id)
        {
            try
            {
                // 1. Obtener la campaña con sus lotes
                var response = await GetByIdAsync(id);
                if (!response.Success || response.Data == null)
                    return OperationResult<Campania>.Failure("No se encontró la campaña", "NOT_FOUND");

                var campania = response.Data;

                // 2. Validar estado actual (solo Planificada puede iniciarse)
                if (campania.EstadosCampania != EnumClass.EstadosCamapaña.Planificada)
                    return OperationResult<Campania>.Failure(
                        "Solo se pueden iniciar campañas en estado Planificada.",
                        "INVALID_STATE");

                // 3. Validar unicidad (solo una EnCurso por licencia)
                var existeEnCurso = await GetQuery()
                    .AnyAsync(c => c.IdLicencia == _userContext.IdLicencia
                                && c.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso
                                && c.Id != id);
                if (existeEnCurso)
                    return OperationResult<Campania>.Failure(
                        "Ya existe una campaña en curso. Debe finalizarla antes de iniciar una nueva.",
                        "CAMPAIGN_ALREADY_IN_PROGRESS");

                // 4. Validar existencia de lotes
                if (campania.Lotes == null || campania.Lotes.Count == 0)
                    return OperationResult<Campania>.Failure(
                        "La campaña debe tener al menos un lote asignado para poder iniciarse.",
                        "NO_LOTS");

                // 5. Validar conflictos de recursos (campos usados en otra campaña activa)
                var campoIds = campania.Lotes.Select(l => l.IdCampo).Distinct().ToList();
                var conflicto = await _unitOfWork.Repository<Lote>()
                    .Query()
                    .AnyAsync(l => campoIds.Contains(l.IdCampo)
                                && l.IdCampania != id
                                && l.Campania.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso);
                if (conflicto)
                    return OperationResult<Campania>.Failure(
                        "Uno o más campos ya están asignados a otra campaña en curso.",
                        "RESOURCE_CONFLICT");

                // 6. Validar fecha (permitir inicio hasta 7 días antes de la fecha planificada)
                //var fechaLimite = campania.FechaInicio.AddDays(-7);
                //if (TimeHelper.GetArgentinaTime() < fechaLimite)
                //    return OperationResult<Campania>.Failure(
                //        "La campaña no puede iniciarse antes de los 7 días previos a su fecha planificada.",
                //        "DATE_TOO_EARLY");

                // 7. Cambiar estado a EnCurso
                campania.EstadosCampania = EnumClass.EstadosCamapaña.EnCurso;
                await UpdateAsync(campania);

                return OperationResult<Campania>.SuccessResult(campania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar la campaña con ID {Id}", id);
                return OperationResult<Campania>.Failure(
                    $"Ocurrió un problema al iniciar la campaña: {ex.Message}",
                    "DATABASE_ERROR");
            }
        }
    }
}
