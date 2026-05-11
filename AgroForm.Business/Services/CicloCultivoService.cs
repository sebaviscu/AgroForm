using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class CicloCultivoService : ServiceBase<CicloCultivo>, ICicloCultivoService
    {
        public CicloCultivoService(IUnitOfWork unitOfWork, ILogger<CicloCultivoService> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        /// <summary>
        /// Override of CreateAsync that adds cycle-specific logic:
        /// checks for an existing active cycle for the same lot+epoca,
        /// and sets FechaInicio/FechaFin defaults.
        /// </summary>
        public override async Task<OperationResult<CicloCultivo>> CreateAsync(CicloCultivo entity)
        {
            try
            {
                // Check if there's already an active (non-closed) cycle for this lot + epoca
                var existingActivo = await _repository.Query()
                    .FirstOrDefaultAsync(c =>
                        c.IdLote == entity.IdLote &&
                        c.Epoca == entity.Epoca &&
                        c.FechaFin == null &&
                        c.IdCampania == _userContext.IdCampaña);

                if (existingActivo != null)
                {
                    _logger.LogInformation("Active cycle already exists for Lote={IdLote}, returning existing", entity.IdLote);
                    return OperationResult<CicloCultivo>.SuccessResult(existingActivo);
                }

                // Set default dates for new cycles
                entity.FechaInicio = TimeHelper.GetArgentinaTime();
                entity.FechaFin = null;

                // CicloCultivo does not implement IEntityBaseWithCampania,
                // so base.CreateAsync won't auto-assign IdCampania.
                // We must set it explicitly here.
                if (_userContext.IdCampaña.HasValue)
                    entity.IdCampania = _userContext.IdCampaña.Value;

                return await base.CreateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CicloCultivo for Lote={IdLote}", entity.IdLote);
                return OperationResult<CicloCultivo>.Failure($"Error al crear el ciclo de cultivo: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Convenience method that builds a CicloCultivo entity and delegates to CreateAsync.
        /// </summary>
        public async Task<OperationResult<CicloCultivo>> CrearCicloAsync(int idLote, int idCultivo, int? idVariedad, EpocaSiembra? epoca)
        {
            var ciclo = new CicloCultivo
            {
                IdLote = idLote,
                IdCultivo = idCultivo,
                IdVariedad = idVariedad,
                Epoca = epoca
            };
            return await CreateAsync(ciclo);
        }

        /// <summary>
        /// Closes a crop cycle by setting FechaFin.
        /// </summary>
        public async Task<OperationResult<CicloCultivo>> CerrarCicloAsync(int idCicloCultivo)
        {
            try
            {
                var ciclo = await _repository.GetAsync(c => c.Id == idCicloCultivo);
                if (ciclo == null)
                {
                    return OperationResult<CicloCultivo>.Failure("No se encontró el ciclo de cultivo.", "NOT_FOUND");
                }

                ciclo.FechaFin = TimeHelper.GetArgentinaTime();
                return await UpdateAsync(ciclo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing CicloCultivo Id={IdCicloCultivo}", idCicloCultivo);
                return OperationResult<CicloCultivo>.Failure($"Error al cerrar el ciclo de cultivo: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets the active (non-closed) cycle for a given lot and optional epoca.
        /// </summary>
        public async Task<OperationResult<CicloCultivo?>> ObtenerCicloActivoAsync(int idLote, EpocaSiembra? epoca = null)
        {
            try
            {
                var query = _repository.Query()
                    .Where(c =>
                        c.IdLote == idLote &&
                        c.FechaFin == null &&
                        c.IdCampania == _userContext.IdCampaña);

                if (epoca.HasValue)
                {
                    query = query.Where(c => c.Epoca == epoca.Value);
                }

                var ciclo = await query
                    .Include(c => c.Cultivo)
                    .Include(c => c.Variedad)
                    .FirstOrDefaultAsync();

                return OperationResult<CicloCultivo?>.SuccessResult(ciclo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active CicloCultivo for Lote={IdLote}", idLote);
                return OperationResult<CicloCultivo?>.Failure($"Error al obtener el ciclo activo: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Gets all cycles for a given lot, ordered by FechaInicio descending.
        /// </summary>
        public async Task<OperationResult<List<CicloCultivo>>> ObtenerCiclosPorLoteAsync(int idLote)
        {
            try
            {
                var ciclos = await _repository.Query()
                    .Where(c => c.IdLote == idLote && c.IdCampania == _userContext.IdCampaña)
                    .Include(c => c.Cultivo)
                    .Include(c => c.Variedad)
                    .OrderByDescending(c => c.FechaInicio)
                    .ToListAsync();

                return OperationResult<List<CicloCultivo>>.SuccessResult(ciclos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CicloCultivos for Lote={IdLote}", idLote);
                return OperationResult<List<CicloCultivo>>.Failure($"Error al obtener los ciclos: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Override of GetAllByCamapniaAsync that includes related entities and adds ordering.
        /// Replaces the former ObtenerCiclosCampaniaActualAsync.
        /// </summary>
        public override async Task<OperationResult<List<CicloCultivo>>> GetAllByCamapniaAsync()
        {
            try
            {
                var ciclos = await _repository.Query()
                    .Where(c => c.IdCampania == _userContext.IdCampaña)
                    .Include(c => c.Cultivo)
                    .Include(c => c.Variedad)
                    .Include(c => c.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(c => c.Siembras)
                    .Include(c => c.Riegos)
                    .Include(c => c.Fertilizaciones)
                    .Include(c => c.Pulverizaciones)
                    .Include(c => c.Monitoreos)
                    .Include(c => c.AnalisisSuelos)
                    .Include(c => c.Cosechas)
                    .Include(c => c.OtrasLabores)
                    .Include(c => c.SiloBolsas)
                    .OrderByDescending(c => c.FechaInicio)
                    .ToListAsync();

                return OperationResult<List<CicloCultivo>>.SuccessResult(ciclos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CicloCultivos for current campaign");
                return OperationResult<List<CicloCultivo>>.Failure($"Error al obtener los ciclos: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Override of GetByIdWithDetailsAsync that includes all related labor collections.
        /// Replaces the former ObtenerCicloConDetallesAsync.
        /// </summary>
        public override async Task<OperationResult<CicloCultivo>> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                var ciclo = await _repository.Query()
                    .Where(c => c.Id == id)
                    .Include(c => c.Cultivo)
                    .Include(c => c.Variedad)
                    .Include(c => c.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(c => c.Campania)
                    .Include(c => c.Siembras)
                        .ThenInclude(s => s.Cultivo)
                    .Include(c => c.Riegos)
                    .Include(c => c.Fertilizaciones)
                    .Include(c => c.Pulverizaciones)
                    .Include(c => c.Monitoreos)
                    .Include(c => c.AnalisisSuelos)
                    .Include(c => c.Cosechas)
                    .Include(c => c.OtrasLabores)
                    .Include(c => c.SiloBolsas)
                    .FirstOrDefaultAsync();

                if (ciclo == null)
                {
                    return OperationResult<CicloCultivo>.Failure("No se encontró el ciclo de cultivo.", "NOT_FOUND");
                }

                return OperationResult<CicloCultivo>.SuccessResult(ciclo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CicloCultivo details Id={Id}", id);
                return OperationResult<CicloCultivo>.Failure($"Error al obtener los detalles del ciclo: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
