using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class ActividadService : IActividadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActividadService> _logger;
        private readonly IUserContext _userContext;
        private readonly AppDbContext _dbContext;
        private readonly IDictionary<TipoActividadEnum, object> _reposPorTipo;
        private readonly IDictionary<Type, object> _reposPorTipoClr;

        public ActividadService(ILogger<ActividadService> logger, IUserContext userContext, IUnitOfWork unitOfWork, AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userContext = userContext;
            _dbContext = dbContext;

            var repoAnalisis = unitOfWork.Repository<AnalisisSuelo>();
            var repoCosecha = unitOfWork.Repository<Cosecha>();
            var repoFert = unitOfWork.Repository<Fertilizacion>();
            var repoMonit = unitOfWork.Repository<Monitoreo>();
            var repoOtra = unitOfWork.Repository<OtraLabor>();
            var repoPulv = unitOfWork.Repository<Pulverizacion>();
            var repoRiego = unitOfWork.Repository<Riego>();
            var repoSiembra = unitOfWork.Repository<Siembra>();
            var repoSiloBolsa = unitOfWork.Repository<SiloBolsa>();

            _reposPorTipo = new Dictionary<TipoActividadEnum, object>
            {
                { TipoActividadEnum.AnalisisSuelo, repoAnalisis },
                { TipoActividadEnum.Cosecha, repoCosecha },
                { TipoActividadEnum.Fertilizado, repoFert },
                { TipoActividadEnum.Monitoreo, repoMonit },
                { TipoActividadEnum.OtrasLabores, repoOtra },
                { TipoActividadEnum.Pulverizacion, repoPulv },
                { TipoActividadEnum.Riego, repoRiego },
                { TipoActividadEnum.Siembra, repoSiembra },
                { TipoActividadEnum.SiloBolsa, repoSiloBolsa }
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
                { typeof(Siembra), repoSiembra },
                { typeof(SiloBolsa), repoSiloBolsa }
            };
        }

        [Obsolete("Este método será eliminado en una versión futura. Usar el nuevo GetLaboresByAsync que ejecuta el SP GetLaboresByAsync.")]
        public async Task<OperationResult<List<LaborDTO>>> GetLaboresByAsyncLegacy(int? idCampania = null, int? idLote = null, List<int> idsLotes = null)
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
                            .Include(_ => _.Campania)
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
                        CostoUSD = s.CostoUSD,
                        CostoARS = s.CostoARS,
                        IdCampania = s.IdCampania,
                        Campania = s.Campania.Nombre,
                        Observacion = s.Observacion,
                        IdLote = s.IdLote,
                        Lote = s.Lote.Nombre,
                        Campo = s.Lote.Campo.Nombre,
                        EsDolar = s.IdMoneda == (int)Monedas.DolarOficial
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
                    CostoUSD = r.CostoUSD,
                    CostoARS = r.CostoARS,
                    IdCampania = r.IdCampania,
                    Campania = r.Campania.Nombre,
                    Observacion = r.Observacion,
                    IdLote = r.IdLote,
                    Lote = r.Lote.Nombre,
                    Campo = r.Lote.Campo.Nombre,
                    EsDolar = r.IdMoneda == (int)Monedas.DolarOficial
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
                    CostoUSD = f.CostoUSD,
                    CostoARS = f.CostoARS,
                    IdCampania = f.IdCampania,
                    Campania = f.Campania.Nombre,
                    Observacion = f.Observacion,
                    IdLote = f.IdLote,
                    Lote = f.Lote.Nombre,
                    Campo = f.Lote.Campo.Nombre,
                    EsDolar = f.IdMoneda == (int)Monedas.DolarOficial
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
                    Detalle = $"Producto: {p.ProductoAgroquimico.Nombre}" +
                      $"{(p.VolumenLitrosHa.HasValue ? $", Volumen: {p.VolumenLitrosHa:N1} L/ha" : "")}" +
                      $"{(p.Dosis.HasValue ? $", Dosis: {p.Dosis:N1}" : "")}" +
                      $"{(string.IsNullOrEmpty(p.CondicionesClimaticas) ? "" : $", Cond: {p.CondicionesClimaticas}")}",
                    Costo = p.Costo,
                    CostoUSD = p.CostoUSD,
                    CostoARS = p.CostoARS,
                    IdCampania = p.IdCampania,
                    Campania = p.Campania.Nombre,
                    Observacion = p.Observacion,
                    IdLote = p.IdLote,
                    Lote = p.Lote.Nombre,
                    Campo = p.Lote.Campo.Nombre,
                    EsDolar = p.IdMoneda == (int)Monedas.DolarOficial
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
                    CostoUSD = m.CostoUSD,
                    CostoARS = m.CostoARS,
                    IdCampania = m.IdCampania,
                    Campania = m.Campania.Nombre,
                    Observacion = m.Observacion,
                    IdLote = m.IdLote,
                    Lote = m.Lote.Nombre,
                    Campo = m.Lote.Campo.Nombre,
                    EsDolar = m.IdMoneda == (int)Monedas.DolarOficial
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
                    Detalle = $"Cultivo: {c.Cultivo.Nombre}" +
                      $"{(c.RendimientoTonHa.HasValue ? $", Rendimiento: {c.RendimientoTonHa:N1} ton/ha" : "")}" +
                      $"{(c.HumedadGrano.HasValue ? $", Humedad: {c.HumedadGrano:N1}%" : "")}" +
                      $"{(c.SuperficieCosechadaHa.HasValue ? $", Sup: {c.SuperficieCosechadaHa:N1} ha" : "")}",
                    Costo = c.Costo,
                    CostoUSD = c.CostoUSD,
                    CostoARS = c.CostoARS,
                    IdCampania = c.IdCampania,
                    Campania = c.Campania.Nombre,
                    Observacion = c.Observacion,
                    IdLote = c.IdLote,
                    Lote = c.Lote.Nombre,
                    Campo = c.Lote.Campo.Nombre,
                    EsDolar = c.IdMoneda == (int)Monedas.DolarOficial
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
                    CostoUSD = a.CostoUSD,
                    CostoARS = a.CostoARS,
                    Responsable = a.RegistrationUser,
                    RegistrationDate = a.RegistrationDate,
                    Detalle = (a.PH.HasValue || a.MateriaOrganica.HasValue || a.Nitrogeno.HasValue || a.Fosforo.HasValue || a.Potasio.HasValue || a.ConductividadElectrica.HasValue || a.CIC.HasValue || !string.IsNullOrEmpty(a.Textura))
                        ? $"{(a.PH.HasValue ? $"pH: {a.PH:N1}, " : "")}" +
                          $"{(a.MateriaOrganica.HasValue ? $"MO: {a.MateriaOrganica:N1}%, " : "")}" +
                          $"{(a.Nitrogeno.HasValue ? $"N: {a.Nitrogeno:N1}, " : "")}" +
                          $"{(a.Fosforo.HasValue ? $"P: {a.Fosforo:N1}, " : "")}" +
                          $"{(a.Potasio.HasValue ? $"K: {a.Potasio:N1}, " : "")}" +
                          $"{(a.ConductividadElectrica.HasValue ? $"CE: {a.ConductividadElectrica:N1}, " : "")}" +
                          $"{(a.CIC.HasValue ? $"CIC: {a.CIC:N1}, " : "")}" +
                          $"{(string.IsNullOrEmpty(a.Textura) ? "" : $"Textura: {a.Textura}")}"
                            .TrimEnd(',', ' ')
                        : string.Empty,
                    IdCampania = a.IdCampania,
                    Campania = a.Campania.Nombre,
                    Observacion = a.Observacion,
                    IdLote = a.IdLote,
                    Lote = a.Lote.Nombre,
                    Campo = a.Lote.Campo.Nombre,
                    EsDolar = a.IdMoneda == (int)Monedas.DolarOficial
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
                    CostoUSD = o.CostoUSD,
                    CostoARS = o.CostoARS,
                    IdCampania = o.IdCampania,
                    Campania = o.Campania.Nombre,
                    Observacion = o.Observacion,
                    IdLote = o.IdLote,
                    Lote = o.Lote.Nombre,
                    Campo = o.Lote.Campo.Nombre,
                    EsDolar = o.IdMoneda == (int)Monedas.DolarOficial
                }).ToListAsync();
                labores.AddRange(otras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando otras labores");
            }

            try
            {
                var siloBolsas = await aplicarFiltros((_reposPorTipo[TipoActividadEnum.SiloBolsa] as IGenericRepository<SiloBolsa>).Query()).Select(sb => new LaborDTO
                {
                    Id = sb.Id,
                    IdTipoActividad = sb.TipoActividad.Id,
                    TipoActividad = sb.TipoActividad.Nombre,
                    IconoTipoActividad = sb.TipoActividad.Icono,
                    IconoColorTipoActividad = sb.TipoActividad.ColorIcono,
                    Fecha = sb.Fecha,
                    Responsable = sb.RegistrationUser,
                    RegistrationDate = sb.RegistrationDate,
                    Detalle = $"Código: {sb.Codigo}, Longitud: {sb.Longitud}m, Capacidad: {sb.CapacidadTotalTn}tn, Humedad: {sb.HumedadGrano}%",
                    Costo = sb.Costo,
                    CostoUSD = sb.CostoUSD,
                    CostoARS = sb.CostoARS,
                    IdCampania = sb.IdCampania,
                    Campania = sb.Campania.Nombre,
                    Observacion = sb.Observacion,
                    IdLote = sb.IdLote,
                    Lote = sb.Lote.Nombre,
                    Campo = sb.Lote.Campo.Nombre,
                    EsDolar = sb.IdMoneda == (int)Monedas.DolarOficial
                }).ToListAsync();
                labores.AddRange(siloBolsas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando silo bolsas");
            }

            return OperationResult<List<LaborDTO>>.SuccessResult(labores.OrderByDescending(l => l.RegistrationDate).ToList());
        }

        public async Task<OperationResult<List<LaborDTO>>> GetLaboresByAsync(int? idCampania = null, int? idLote = null, List<int> idsLotes = null)
        {
            try
            {
                // Si no se especifica campaña, usar la del usuario actual
                if (!idCampania.HasValue)
                {
                    idCampania = _userContext.IdCampaña;
                }

                // Si no hay campaña activa, devolver lista vacía
                if (!idCampania.HasValue)
                {
                    _logger.LogInformation("No hay campaña activa, devolviendo lista vacía de labores");
                    return OperationResult<List<LaborDTO>>.SuccessResult(new List<LaborDTO>());
                }

                var idLicencia = _userContext.IdLicencia ?? 0;
                var idsLotesStr = idsLotes != null && idsLotes.Any() ? string.Join(",", idsLotes) : null;

                _logger.LogInformation("Ejecutando SP GetLaboresByAsync: IdLicencia={IdLicencia}, IdCampania={IdCampania}, IdLote={IdLote}, IdsLotes={IdsLotes}",
                    idLicencia, idCampania, idLote, idsLotesStr);

                var labores = await _dbContext.Database
                    .SqlQueryRaw<LaborDTO>(
                        "EXEC GetLaboresByAsync @p0, @p1, @p2, @p3",
                        idLicencia,
                        idCampania,
                        idLote,
                        idsLotesStr)
                    .ToListAsync();

                return OperationResult<List<LaborDTO>>.SuccessResult(labores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar labores mediante SP GetLaboresByAsync");
                return OperationResult<List<LaborDTO>>.Failure(ex.Message, "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<bool>> SaveActividadAsync(List<ILabor> actividades)
        {
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
                    return OperationResult<bool>.Failure(ex.Message, "DATABASE_ERROR");
                }
            }
            await _unitOfWork.SaveAsync();
            return OperationResult<bool>.SuccessResult(true);

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

                    case TipoActividadEnum.SiloBolsa:
                        var repoSiloBolsa = repoObj as IGenericRepository<SiloBolsa>;
                        return await repoSiloBolsa.Query()
                            .Include(s => s.Moneda)
                            .Include(sb => sb.TipoActividad)
                            .Include(sb => sb.Lote)
                                .ThenInclude(l => l.Campo)
                            .FirstOrDefaultAsync(sb => sb.Id == idActividad);

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
                await _unitOfWork.SaveAsync();
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
                var tipoClr = actividad.GetType();
                if (!_reposPorTipoClr.TryGetValue(tipoClr, out var repoObj))
                {
                    return OperationResult<ILabor>.Failure($"Tipo de actividad no soportado: {tipoClr.Name}", "UNSUPPORTED_TYPE");
                }

                dynamic repo = repoObj;
                await repo.UpdateAsync((dynamic)actividad);
                await _unitOfWork.SaveAsync();

                return OperationResult<ILabor>.SuccessResult(actividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editar actividad tipo {Tipo}", actividad.GetType().Name);
                return OperationResult<ILabor>.Failure($"Ocurrió un problema al actualizar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // El método UpdateEntityAsync ya no es necesario ya que UpdateActividadAsync usa los repositorios directamente.

        public async Task<OperationResult<List<Siembra>>> GetSiembrasAsync()
        {
            try
            {
                var repo = _reposPorTipoClr[typeof(Siembra)] as IGenericRepository<Siembra>;
                
                // Solo filtrar por campaña si el claim tiene un valor
                if (_userContext.IdCampaña.HasValue)
                {
                    var list = await repo.Query()
                        .Where(_ => _.IdCampania == _userContext.IdCampaña.Value)
                        .Include(_ => _.Cultivo)
                        .ToListAsync();

                    return OperationResult<List<Siembra>>.SuccessResult(list);
                }
                else
                {
                    // Si no hay campaña activa, devolver lista vacía
                    return OperationResult<List<Siembra>>.SuccessResult(new List<Siembra>());
                }
            }
            catch (Exception e)
            {
                return OperationResult<List<Siembra>>.Failure(e.Message, "DATABASE_ERROR");
            }
        }
    }
}
