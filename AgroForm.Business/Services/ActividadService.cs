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
        private readonly IDictionary<TipoActividadEnum, object> _reposPorTipo;
        private readonly IDictionary<Type, object> _reposPorTipoClr;

        public ActividadService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ActividadService> logger, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _contextFactory = contextFactory;
            _logger = logger;

            var repoAnalisis = unitOfWork.Repository<AnalisisSuelo>();
            var repoCosecha = unitOfWork.Repository<Cosecha>();
            var repoFert = unitOfWork.Repository<Fertilizacion>();
            var repoMonit = unitOfWork.Repository<Monitoreo>();
            var repoOtra = unitOfWork.Repository<OtraLabor>();
            var repoPulv = unitOfWork.Repository<Pulverizacion>();
            var repoRiego = unitOfWork.Repository<Riego>();
            var repoSiembra = unitOfWork.Repository<Siembra>();

            _reposPorTipo = new Dictionary<TipoActividadEnum, object>
            {
                { TipoActividadEnum.AnalisisSuelo, repoAnalisis },
                { TipoActividadEnum.Cosecha, repoCosecha },
                { TipoActividadEnum.Fertilizado, repoFert },
                { TipoActividadEnum.Monitoreo, repoMonit },
                { TipoActividadEnum.OtrasLabores, repoOtra },
                { TipoActividadEnum.Pulverizacion, repoPulv },
                { TipoActividadEnum.Riego, repoRiego },
                { TipoActividadEnum.Siembra, repoSiembra }
            };

            _reposPorTipoClr = new Dictionary<Type, object>
            {
                { typeof(AnalisisSuelo), repoAnalisis },
                { typeof(Cosecha), repoCosecha },
                { typeof(Fertilizacion), repoFert },
                { typeof(Monitoreo), repoMonit },
                { typeof(OtraLabor), repoOtra },
                { typeof(Pulverizacion), repoPulv },
                { typeof(Riego), repoRiego },
                { typeof(Siembra), repoSiembra }
            };

            var httpContext = httpContextAccessor.HttpContext;
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

        public async Task<OperationResult<List<LaborDTO>>> GetLaboresByAsync(int? idCampania = null, int? idLote = null, List<int> idsLotes = null)
        {
            var labores = new List<LaborDTO>();

            IQueryable<T> aplicarFiltros<T>(IQueryable<T> query) where T : class, ILabor
            {
                if (idCampania.HasValue)
                    query = query.Where(x => x.IdCampania == idCampania.Value);
                if (idLote.HasValue)
                    query = query.Where(x => x.IdLote == idLote.Value);
                if (idsLotes != null && idsLotes.Any())
                    query = query.Where(x => idsLotes.Contains(x.IdLote));
                return query.Include(_ => _.TipoActividad)
                            .Include(_ => _.Lote)
                                .ThenInclude(_ => _.Campo);
            }

            try
            {
                var repoSiembra = _reposPorTipo[TipoActividadEnum.Siembra] as IGenericRepository<Siembra>;
                var siembras = await aplicarFiltros(repoSiembra.Query())
                    .Select(s => new LaborDTO
                    {
                        Id = s.Id,
                        TipoActividad = s.TipoActividad.Nombre,
                        IdTipoActividad = s.TipoActividad.Id,
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
                        Lote = s.Lote.Nombre,
                        Campo = s.Lote.Campo.Nombre,
                        EsDolar = s.IdMoneda == (int)Monedas.Dolar
                    }).ToListAsync();
                labores.AddRange(siembras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando siembras");
            }

            try
            {
                var riegos = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.Riego] as IGenericRepository<Riego>).Query()).Select(r => new LaborDTO
                {
                    Id = r.Id,
                    TipoActividad = r.TipoActividad.Nombre,
                    IdTipoActividad = r.TipoActividad.Id,
                    IconoTipoActividad = r.TipoActividad.Icono,
                    IconoColorTipoActividad = r.TipoActividad.ColorIcono,
                    Fecha = r.Fecha,
                    Responsable = r.RegistrationUser,
                    RegistrationDate = r.RegistrationDate,
                    Detalle = $"Horas: {r.HorasRiego}, Volumen: {r.VolumenAguaM3} m³",
                    Costo = r.Costo,
                    IdCampania = r.IdCampania,
                    Observacion = r.Observacion,
                    IdLote = r.IdLote,
                    Lote = r.Lote.Nombre,
                    Campo = r.Lote.Campo.Nombre,
                    EsDolar = r.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(riegos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando riegos");
            }

            try
            {
                var fertilizaciones = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.Fertilizado] as IGenericRepository<Fertilizacion>).Query()).Select(f => new LaborDTO
                {
                    Id = f.Id,
                    IdTipoActividad = f.TipoActividad.Id,
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
                    Lote = f.Lote.Nombre,
                    Campo = f.Lote.Campo.Nombre,
                    EsDolar = f.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(fertilizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando fertilizaciones");
            }

            try
            {
                var pulverizaciones = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.Pulverizacion] as IGenericRepository<Pulverizacion>).Query()).Select(p => new LaborDTO
                {
                    Id = p.Id,
                    IdTipoActividad = p.TipoActividad.Id,
                    TipoActividad = p.TipoActividad.Nombre,
                    IconoTipoActividad = p.TipoActividad.Icono,
                    IconoColorTipoActividad = p.TipoActividad.ColorIcono,
                    Fecha = p.Fecha,
                    RegistrationDate = p.RegistrationDate,
                    Responsable = p.RegistrationUser,
                    Detalle = $"Producto: {p.ProductoAgroquimico.Nombre}, Volumen: {p.VolumenLitrosHa} L/ha",
                    Costo = p.Costo,
                    IdCampania = p.IdCampania,
                    Observacion = p.Observacion,
                    IdLote = p.IdLote,
                    Lote = p.Lote.Nombre,
                    Campo = p.Lote.Campo.Nombre,
                    EsDolar = p.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(pulverizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando pulverizaciones");
            }

            try
            {
                var monitoreos = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.Monitoreo] as IGenericRepository<Monitoreo>).Query()).Select(m => new LaborDTO
                {
                    Id = m.Id,
                    IdTipoActividad = m.TipoActividad.Id,
                    TipoActividad = m.TipoActividad.Nombre,
                    IconoTipoActividad = m.TipoActividad.Icono,
                    IconoColorTipoActividad = m.TipoActividad.ColorIcono,
                    Fecha = m.Fecha,
                    Responsable = m.RegistrationUser,
                    RegistrationDate = m.RegistrationDate,
                    Detalle = $"{m.TipoMonitoreo.Tipo}: {m.TipoMonitoreo.Nombre}",
                    Costo = m.Costo,
                    IdCampania = m.IdCampania,
                    Observacion = m.Observacion,
                    IdLote = m.IdLote,
                    Lote = m.Lote.Nombre,
                    Campo = m.Lote.Campo.Nombre,
                    EsDolar = m.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(monitoreos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando monitoreos");
            }

            try
            {
                var cosechas = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.Cosecha] as IGenericRepository<Cosecha>).Query()).Select(c => new LaborDTO
                {
                    Id = c.Id,
                    IdTipoActividad = c.TipoActividad.Id,
                    TipoActividad = c.TipoActividad.Nombre,
                    IconoTipoActividad = c.TipoActividad.Icono,
                    IconoColorTipoActividad = c.TipoActividad.ColorIcono,
                    Fecha = c.Fecha,
                    Responsable = c.RegistrationUser,
                    RegistrationDate = c.RegistrationDate,
                    Detalle = $"Cultivo: {c.Cultivo.Nombre}, Rendimiento: {c.RendimientoTonHa} ton/ha",
                    Costo = c.Costo,
                    IdCampania = c.IdCampania,
                    Observacion = c.Observacion,
                    IdLote = c.IdLote,
                    Lote = c.Lote.Nombre,
                    Campo = c.Lote.Campo.Nombre,
                    EsDolar = c.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(cosechas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando cosechas");
            }

            try
            {
                var analisis = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.AnalisisSuelo] as IGenericRepository<AnalisisSuelo>).Query()).Select(a => new LaborDTO
                {
                    Id = a.Id,
                    IdTipoActividad = a.TipoActividad.Id,
                    TipoActividad = a.TipoActividad.Nombre,
                    IconoTipoActividad = a.TipoActividad.Icono,
                    IconoColorTipoActividad = a.TipoActividad.ColorIcono,
                    Fecha = a.Fecha,
                    Costo = a.Costo,
                    Responsable = a.RegistrationUser,
                    RegistrationDate = a.RegistrationDate,
                    Detalle = $"pH: {a.PH}, MO: {a.MateriaOrganica}%",
                    IdCampania = a.IdCampania,
                    Observacion = a.Observacion,
                    IdLote = a.IdLote,
                    Lote = a.Lote.Nombre,
                    Campo = a.Lote.Campo.Nombre,
                    EsDolar = a.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(analisis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando analisis");
            }

            try
            {
                var otras = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.OtrasLabores] as IGenericRepository<OtraLabor>).Query()).Select(o => new LaborDTO
                {
                    Id = o.Id,
                    IdTipoActividad = o.TipoActividad.Id,
                    TipoActividad = o.TipoActividad.Nombre,
                    IconoTipoActividad = o.TipoActividad.Icono,
                    IconoColorTipoActividad = o.TipoActividad.ColorIcono,
                    Fecha = o.Fecha,
                    Responsable = o.RegistrationUser,
                    RegistrationDate = o.RegistrationDate,
                    Detalle = $"Descripción: {o.Observacion}",
                    Costo = o.Costo,
                    IdCampania = o.IdCampania,
                    Observacion = o.Observacion,
                    IdLote = o.IdLote,
                    Lote = o.Lote.Nombre,
                    Campo = o.Lote.Campo.Nombre,
                    EsDolar = o.IdMoneda == (int)Monedas.Dolar
                }).ToListAsync();
                labores.AddRange(otras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando otras labores");
            }

            return OperationResult<List<LaborDTO>>.SuccessResult(labores.OrderByDescending(l => l.RegistrationDate).ToList());
        }

        public async Task SaveActividadAsync(List<ILabor> actividades)
        {
            if (actividades == null || !actividades.Any()) return;

            foreach (var actividad in actividades)
            {
                try
                {
                    var tipoClr = actividad.GetType();
                    if (!_reposPorTipoClr.TryGetValue(tipoClr, out var repoObj))
                    {
                        _logger.LogWarning("No existe repositorio para el tipo CLR {Tipo}", tipoClr.Name);
                        continue;
                    }

                    dynamic repo = repoObj;
                    await repo.AddAsync((dynamic)actividad);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error guardando actividad tipo {Tipo}", actividad.GetType().Name);
                }
            }
        }

        public async Task<object> GetLaboresByAsync(int idActividad, TipoActividadEnum idTipoActividad)
        {
            if (!_reposPorTipo.TryGetValue(idTipoActividad, out var repoObj))
                return null;

            try
            {
                switch (idTipoActividad)
                {
                    case TipoActividadEnum.Siembra:
                        var repoSembra = repoObj as IGenericRepository<Siembra>;
                        return await repoSembra.Query()
                            .Include(s => s.TipoActividad)
                            .Include(s => s.Cultivo)
                            .Include(s => s.Variedad)
                            .Include(s => s.MetodoSiembra)
                            .Include(s => s.Moneda)
                            .Include(s => s.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(s => s.Id == idActividad);

                    case TipoActividadEnum.Riego:
                        var repoRiego = repoObj as IGenericRepository<Riego>;
                        return await repoRiego.Query()
                            .Include(s => s.Moneda)
                            .Include(r => r.TipoActividad)
                            .Include(r => r.MetodoRiego)
                            .Include(r => r.FuenteAgua)
                            .Include(r => r.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(r => r.Id == idActividad);

                    case TipoActividadEnum.Fertilizado:
                        var repoFertilizacion = repoObj as IGenericRepository<Fertilizacion>;
                        return await repoFertilizacion.Query()
                            .Include(s => s.Moneda)
                            .Include(f => f.TipoActividad)
                            .Include(f => f.Nutriente)
                            .Include(f => f.TipoFertilizante)
                            .Include(f => f.MetodoAplicacion)
                            .Include(f => f.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(f => f.Id == idActividad);

                    case TipoActividadEnum.Pulverizacion:
                        var repoPulverizacion = repoObj as IGenericRepository<Pulverizacion>;
                        return await repoPulverizacion.Query()
                            .Include(s => s.Moneda)
                            .Include(p => p.TipoActividad)
                            .Include(p => p.ProductoAgroquimico)
                            .Include(p => p.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(p => p.Id == idActividad);

                    case TipoActividadEnum.Monitoreo:
                        var repoMonitoreo = repoObj as IGenericRepository<Monitoreo>;
                        return await repoMonitoreo.Query()
                            .Include(s => s.Moneda)
                            .Include(m => m.TipoActividad)
                            .Include(m => m.TipoMonitoreo)
                            .Include(m => m.EstadoFenologico)
                            .Include(m => m.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(m => m.Id == idActividad);

                    case TipoActividadEnum.Cosecha:
                        var repoCosecha = repoObj as IGenericRepository<Cosecha>;
                        return await repoCosecha.Query()
                            .Include(s => s.Moneda)
                            .Include(c => c.TipoActividad)
                            .Include(c => c.Cultivo)
                            .Include(c => c.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(c => c.Id == idActividad);

                    case TipoActividadEnum.AnalisisSuelo:
                        var repoAnalisis = repoObj as IGenericRepository<AnalisisSuelo>;
                        return await repoAnalisis.Query()
                            .Include(s => s.Moneda)
                            .Include(a => a.TipoActividad)
                            .Include(a => a.Laboratorio)
                            .Include(a => a.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(a => a.Id == idActividad);

                    case TipoActividadEnum.OtrasLabores:
                        var repoOtra = repoObj as IGenericRepository<OtraLabor>;
                        return await repoOtra.Query()
                            .Include(s => s.Moneda)
                            .Include(o => o.TipoActividad)
                            .Include(o => o.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(o => o.Id == idActividad);

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la entidad con ID {IdActividad}", idActividad);
                return null;
            }
        }

        public async Task DeteleActividadAsync(int idActividad, TipoActividadEnum idTipoActividad)
        {
            if (!_reposPorTipo.TryGetValue(idTipoActividad, out var repoObj)) return;

            try
            {
                dynamic repo = repoObj;
                await repo.DeleteByIdAsync(idActividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al borrar la entidad con ID {IdActividad}", idActividad);
            }
        }

        public async Task<OperationResult<ILabor>> UpdateActividadAsync(ILabor actividad)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                switch (actividad)
                {
                    case Siembra siembra:
                        return await UpdateEntityAsync(context, siembra);
                    case Riego riego:
                        return await UpdateEntityAsync(context, riego);
                    case Fertilizacion fertilizacion:
                        return await UpdateEntityAsync(context, fertilizacion);
                    case Pulverizacion pulverizacion:
                        return await UpdateEntityAsync(context, pulverizacion);
                    case Monitoreo monitoreo:
                        return await UpdateEntityAsync(context, monitoreo);
                    case Cosecha cosecha:
                        return await UpdateEntityAsync(context, cosecha);
                    case AnalisisSuelo analisisSuelo:
                        return await UpdateEntityAsync(context, analisisSuelo);
                    case OtraLabor otraLabor:
                        return await UpdateEntityAsync(context, otraLabor);
                    default:
                        return OperationResult<ILabor>.Failure($"Tipo de actividad no soportado: {actividad.GetType().Name}", "UNSUPPORTED_TYPE");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editar actividad tipo {Tipo}", actividad.GetType().Name);
                return OperationResult<ILabor>.Failure($"Ocurrió un problema al actualizar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // Método genérico auxiliar
        private async Task<OperationResult<ILabor>> UpdateEntityAsync<T>(AppDbContext context, T entity) where T : class, ILabor
        {
            var original = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (original == null)
                return OperationResult<ILabor>.Failure("El registro que intenta actualizar no existe.", "NOT_FOUND");

            if (entity is EntityBase entityBase)
            {
                entityBase.ModificationDate = TimeHelper.GetArgentinaTime();
                entityBase.ModificationUser = _userAuth.UserName;
            }

            // Copiar valores
            context.Entry(original).CurrentValues.SetValues(entity);

            int result = await context.SaveChangesAsync();

            if (result > 0)
                return OperationResult<ILabor>.SuccessResult(entity);

            return OperationResult<ILabor>.Failure("No se pudo actualizar el registro en la base de datos.", "SAVE_FAILED");
        }
    }
}
