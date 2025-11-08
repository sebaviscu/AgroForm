using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class ActividadService : EntityBase, IActividadService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<ActividadService> _logger;
        private UserAuth _userAuth;

        private readonly IGenericRepository<AnalisisSuelo> _repositoryAnalisisSuelo;
        private readonly IGenericRepository<Cosecha> _repositoryCosecha;
        private readonly IGenericRepository<Fertilizacion> _repositoryFertilizacion;
        private readonly IGenericRepository<Monitoreo> _repositoryMonitoreo;
        private readonly IGenericRepository<OtraLabor> _repositoryOtrasLabores;
        private readonly IGenericRepository<Pulverizacion> _repositoryPulverizacion;
        private readonly IGenericRepository<Riego> _repositoryRiego;
        private readonly IGenericRepository<Siembra> _repositorySiembra;


        public ActividadService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ActividadService> logger, IHttpContextAccessor _httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _contextFactory = contextFactory;
            _logger = logger;

            _repositoryAnalisisSuelo = unitOfWork.Repository<AnalisisSuelo>();
            _repositoryCosecha = unitOfWork.Repository<Cosecha>();
            _repositoryFertilizacion = unitOfWork.Repository<Fertilizacion>();
            _repositoryMonitoreo = unitOfWork.Repository<Monitoreo>();
            _repositoryOtrasLabores = unitOfWork.Repository<OtraLabor>();
            _repositoryPulverizacion = unitOfWork.Repository<Pulverizacion>();
            _repositoryRiego = unitOfWork.Repository<Riego>();
            _repositorySiembra = unitOfWork.Repository<Siembra>();

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                var claimUser = httpContext.User;

                _userAuth = new UserAuth
                {
                    UserName = UtilidadService.GetClaimValue<string>(claimUser, ClaimTypes.Name),
                    IdLicencia = UtilidadService.GetClaimValue<int>(claimUser, "Licencia"),
                    IdCampaña = UtilidadService.GetClaimValue<int>(claimUser, "Campania"),
                    IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                    IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, ClaimTypes.Role)
                };
            }
        }



        //public async Task<List<Actividad>> GetByidCampoAsync(List<int> lotesId)
        //{
        //    try
        //    {
        //        using var context = await _contextFactory.CreateDbContextAsync();
        //        var query = context.Set<Actividad>()
        //            .Include(a => a.Lote)
        //            .ThenInclude(l => l.Campo)
        //            .AsNoTracking()
        //            .AsQueryable();

        //        if (lotesId.Any())
        //        {
        //            query = query.Where(_ => lotesId.Contains(_.IdLote));
        //        }

        //        return await query.ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener actividades por LotesId {LotesId}", string.Join(",", lotesId));
        //        throw;
        //    }
        //}

        //public override async Task<OperationResult<List<Actividad>>> GetAllWithDetailsAsync()
        //{
        //    try
        //    {
        //        using var context = await _contextFactory.CreateDbContextAsync();
        //        var entities = await context.Set<Actividad>()
        //            .Include(a => a.Insumo)
        //            .Include(a => a.TipoActividad)
        //            .Include(a => a.Lote)
        //            .ThenInclude(l => l.Campo)
        //            .AsNoTracking()
        //            .ToListAsync();

        //        return OperationResult<List<Actividad>>.SuccessResult(entities);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener todas las actividades con detalles");
        //        return OperationResult<List<Actividad>>.Failure("Error al obtener actividades");
        //    }
        //}

        //public override async Task<OperationResult<Actividad>> GetByIdWithDetailsAsync(long id)
        //{
        //    try
        //    {
        //        using var context = await _contextFactory.CreateDbContextAsync();
        //        var entity = await context.Set<Actividad>()
        //            .Include(a => a.Lote)
        //            .ThenInclude(l => l.Campo)
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(e => e.Id == id);

        //        if (entity == null)
        //            return OperationResult<Actividad>.Failure("Actividad no encontrada");

        //        return OperationResult<Actividad>.SuccessResult((Actividad)(object)entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener actividad con id {Id}", id);
        //        return OperationResult<Actividad>.Failure("Error al obtener actividad");
        //    }
        //}

        //public async Task<OperationResult<List<Actividad>>> GetRecentAsync()
        //{
        //    try
        //    {
        //        var fechaLimite = TimeHelper.GetArgentinaTime(); // Últimos 7 días

        //        using var context = await _contextFactory.CreateDbContextAsync();

        //        //var actividades = await context.Actividades
        //        //    .Where(a => a.IdLicencia == _userAuth.IdLicencia)
        //        //    .Include(a => a.Lote)
        //        //        .ThenInclude(l => l.Campo)
        //        //    .Include(a => a.TipoActividad)
        //        //    .Include(a => a.Usuario)
        //        //    .Include(a => a.Insumo)
        //        //    .OrderByDescending(a => a.Fecha)
        //        //    .Take(15)
        //        //    .AsNoTracking()
        //        //    .ToListAsync();

        //        return default;

        //        //return OperationResult<List<Actividad>>.SuccessResult(actividades);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener actividades recientes para licencia {IdLicencia}", _userAuth.IdLicencia);
        //        return OperationResult<List<Actividad>>.Failure("Error al obtener actividades recientes");
        //    }
        //}

        public async Task<OperationResult<List<LaborDTO>>> GetLaboresByAsync(int? idCampania = null, int? idLote = null, List<int> idsLotes = null)
        {
            var labores = new List<LaborDTO>();

            // Filtros dinámicos
            IQueryable<T> aplicarFiltros<T>(IQueryable<T> query)
                where T : class, ILabor
            {
                if (idCampania.HasValue)
                    query = query.Where(x => x.IdCampania == idCampania.Value);
                if (idLote.HasValue)
                    query = query.Where(x => x.IdLote == idLote.Value);
                if (idsLotes != null && idsLotes.Any())
                    query = query.Where(x => idsLotes.Contains(x.IdLote));
                return query.Include(_ => _.TipoActividad).Include(_ => _.Lote);
            }


            // Siembra
            var siembras = await aplicarFiltros(_repositorySiembra.Query()).Select(s => new LaborDTO
            {
                Id = s.Id,
                TipoActividad = s.TipoActividad.Nombre,
                IconoTipoActividad = s.TipoActividad.Icono,
                IconoColorTipoActividad = s.TipoActividad.ColorIcono,
                Fecha = s.Fecha,
                Responsable = s.RegistrationUser,
                RegistrationDate = s.RegistrationDate,
                Detalle = $"Cultivo: {s.Cultivo.Nombre}, Superficie: {s.SuperficieHa} ha, Densidad: {s.DensidadSemillaKgHa} kg/ha",
                Costo = s.Costo,
                IdCampania = s.IdCampania,
                Observacion = s.Observacion,
                IdLote = s.IdLote,
                Lote = s.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(siembras);

            // Riego
            var riegos = await aplicarFiltros(_repositoryRiego.Query()).Select(r => new LaborDTO
            {
                Id = r.Id,
                TipoActividad =r.TipoActividad.Nombre,
                IconoTipoActividad = r.TipoActividad.Icono,
                IconoColorTipoActividad = r.TipoActividad.ColorIcono,
                Fecha = r.Fecha,
                Responsable = r.RegistrationUser,
                RegistrationDate = r.RegistrationDate,
                Detalle = $"Horas: {r.HorasRiego}, Volumen: {r.VolumenAguaM3} m³",
                Costo = null,
                IdCampania = r.IdCampania,
                Observacion = r.Observacion,
                IdLote = r.IdLote,
                Lote = r.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(riegos);

            // Fertilización
            var fertilizaciones = await aplicarFiltros(_repositoryFertilizacion.Query()).Select(f => new LaborDTO
            {
                Id = f.Id,
                TipoActividad = f.TipoActividad.Nombre,
                IconoTipoActividad = f.TipoActividad.Icono,
                IconoColorTipoActividad = f.TipoActividad.ColorIcono,
                Fecha = f.Fecha,
                Responsable = f.RegistrationUser,
                RegistrationDate = f.RegistrationDate,
                Detalle = $"Nutriente: {f.Nutriente.Nombre}, Dosis: {f.DosisKgHa} kg/ha",
                Costo = f.Costo,
                IdCampania = f.IdCampania,
                Observacion = f.Observacion,
                IdLote = f.IdLote,
                Lote = f.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(fertilizaciones);

            // Pulverización
            var pulverizaciones = await aplicarFiltros(_repositoryPulverizacion.Query()).Select(p => new LaborDTO
            {
                Id = p.Id,
                TipoActividad = p.TipoActividad.Nombre,
                IconoTipoActividad = p.TipoActividad.Icono,
                IconoColorTipoActividad = p.TipoActividad.ColorIcono,
                Fecha = p.Fecha,
                RegistrationDate = p.RegistrationDate,
                Responsable = p.RegistrationUser,
                Detalle = $"Producto: {p.ProductoAgroquimico.Nombre}, Volumen: {p.VolumenLitrosHa} L/ha",
                Costo = null,
                IdCampania = p.IdCampania,
                Observacion = p.Observacion,
                IdLote = p.IdLote,
                Lote = p.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(pulverizaciones);

            // Monitoreo
            var monitoreos = await aplicarFiltros(_repositoryMonitoreo.Query()).Select(m => new LaborDTO
            {
                Id = m.Id,
                TipoActividad = m.TipoActividad.Nombre,
                IconoTipoActividad = m.TipoActividad.Icono,
                IconoColorTipoActividad = m.TipoActividad.ColorIcono,
                Fecha = m.Fecha,
                Responsable = m.RegistrationUser,
                RegistrationDate = m.RegistrationDate,
                Detalle = $"Tipo: {m.TipoMonitoreo.Nombre}, Estado: {m.EstadoFenologico.Nombre ?? "N/A"}",
                Costo = null,
                IdCampania = m.IdCampania,
                Observacion = m.Observacion,
                IdLote = m.IdLote,
                Lote = m.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(monitoreos);

            // Cosecha
            var cosechas = await aplicarFiltros(_repositoryCosecha.Query()).Select(c => new LaborDTO
            {
                Id = c.Id,
                TipoActividad = c.TipoActividad.Nombre,
                IconoTipoActividad = c.TipoActividad.Icono,
                IconoColorTipoActividad = c.TipoActividad.ColorIcono,
                Fecha = c.Fecha,
                Responsable = c.RegistrationUser,
                RegistrationDate = c.RegistrationDate,
                Detalle = $"Cultivo: {c.Cultivo.Nombre}, Rendimiento: {c.RendimientoTonHa} ton/ha",
                Costo = null,
                IdCampania = c.IdCampania,
                Observacion = c.Observacion,
                IdLote = c.IdLote,
                Lote = c.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(cosechas);

            // Análisis de Suelo
            var analisis = await aplicarFiltros(_repositoryAnalisisSuelo.Query()).Select(a => new LaborDTO
            {
                Id = a.Id,
                TipoActividad = a.TipoActividad.Nombre,
                IconoTipoActividad = a.TipoActividad.Icono,
                IconoColorTipoActividad = a.TipoActividad.ColorIcono,
                Fecha = a.Fecha,
                Responsable = a.RegistrationUser,
                RegistrationDate = a.RegistrationDate,
                Detalle = $"pH: {a.PH}, MO: {a.MateriaOrganica}%",
                Costo = null,
                IdCampania = a.IdCampania,
                Observacion = a.Observacion,
                IdLote = a.IdLote,
                Lote = a.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(analisis);

            // Otra labor
            var otras = await aplicarFiltros(_repositoryOtrasLabores.Query()).Select(o => new LaborDTO
            {
                Id = o.Id,
                TipoActividad = o.TipoActividad.Nombre,
                IconoTipoActividad = o.TipoActividad.Icono,
                IconoColorTipoActividad = o.TipoActividad.ColorIcono,
                Fecha = o.Fecha,
                Responsable = o.RegistrationUser,
                RegistrationDate = o.RegistrationDate,
                Detalle = $"Descripción: {o.Observacion}",
                Costo = null,
                IdCampania = o.IdCampania,
                Observacion = o.Observacion,
                IdLote = o.IdLote,
                Lote = o.Lote.Nombre
            }).ToListAsync();
            labores.AddRange(otras);

            return OperationResult<List<LaborDTO>>.SuccessResult(labores.OrderByDescending(l => l.RegistrationDate).ToList());
        }

        public async Task SaveActividadAsync(List<ILabor> actividades)
        {
            foreach (var actividad in actividades)
            {
                switch (actividad)
                {
                    case Siembra siembra:
                        await GuardarAsync(_repositorySiembra, siembra);
                        break;
                    case Riego riego:
                        await GuardarAsync(_repositoryRiego, riego);
                        break;
                    case Fertilizacion fertilizacion:
                        await GuardarAsync(_repositoryFertilizacion, fertilizacion);
                        break;
                    case Pulverizacion pulverizacion:
                        await GuardarAsync(_repositoryPulverizacion, pulverizacion);
                        break;
                    case Monitoreo monitoreo:
                        await GuardarAsync(_repositoryMonitoreo, monitoreo);
                        break;
                    case Cosecha cosecha:
                        await GuardarAsync(_repositoryCosecha, cosecha);
                        break;
                    case AnalisisSuelo analisis:
                        await GuardarAsync(_repositoryAnalisisSuelo, analisis);
                        break;
                    case OtraLabor otra:
                        await GuardarAsync(_repositoryOtrasLabores, otra);
                        break;
                    default:
                        throw new InvalidOperationException($"Tipo de actividad no soportado: {actividad.GetType().Name}");
                }

            }
        }

        private async Task GuardarAsync<T>(IGenericRepository<T> repo, T entidad) where T : EntityBaseWithLicencia
        {
            try
            {
                if (entidad.Id == 0)
                    await repo.AddAsync(entidad);
                else
                    await repo.UpdateAsync(entidad);

                using var context = await _contextFactory.CreateDbContextAsync();
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw;
            }

        }

    }
}
