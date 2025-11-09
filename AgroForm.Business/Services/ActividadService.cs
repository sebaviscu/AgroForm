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

        public async Task<OperationResult<List<LaborDTO>>> GetLaboresByAsync(int? idCampania = null, int? idLote = null, List<int> idsLotes = null)
        {
            var labores = new List<LaborDTO>();

            IQueryable<T> aplicarFiltros<T>(IQueryable<T> query)
                where T : class, ILabor
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


            // Siembra
            var siembras = await aplicarFiltros(_repositorySiembra.Query()).Select(s => new LaborDTO
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

            // Riego
            var riegos = await aplicarFiltros(_repositoryRiego.Query()).Select(r => new LaborDTO
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

            // Fertilización
            var fertilizaciones = await aplicarFiltros(_repositoryFertilizacion.Query()).Select(f => new LaborDTO
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

            // Pulverización
            var pulverizaciones = await aplicarFiltros(_repositoryPulverizacion.Query()).Select(p => new LaborDTO
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

            // Monitoreo
            var monitoreos = await aplicarFiltros(_repositoryMonitoreo.Query()).Select(m => new LaborDTO
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

            // Cosecha
            var cosechas = await aplicarFiltros(_repositoryCosecha.Query()).Select(c => new LaborDTO
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

            // Análisis de Suelo
            var analisis = await aplicarFiltros(_repositoryAnalisisSuelo.Query()).Select(a => new LaborDTO
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

            // Otra labor
            var otras = await aplicarFiltros(_repositoryOtrasLabores.Query()).Select(o => new LaborDTO
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

            }

        }

        public async Task DeteleActividadAsync(int idActividad, TipoActividadEnum IdTipoActividad)
        {
            switch(IdTipoActividad)
                {
                    case TipoActividadEnum.Siembra:
                        await BorrarAsync(_repositorySiembra, idActividad);
                        break;
                    case TipoActividadEnum.Riego:
                        await BorrarAsync(_repositoryRiego, idActividad);
                        break;
                    case TipoActividadEnum.Fertilizado:
                        await BorrarAsync(_repositoryFertilizacion, idActividad);
                        break;
                    case TipoActividadEnum.Pulverizacion:
                        await BorrarAsync(_repositoryPulverizacion, idActividad);
                        break;
                    case TipoActividadEnum.Monitoreo:
                        await BorrarAsync(_repositoryMonitoreo, idActividad);
                        break;
                    case TipoActividadEnum.Cosecha:
                        await BorrarAsync(_repositoryCosecha, idActividad);
                        break;
                    case TipoActividadEnum.AnalisisSuelo:
                        await BorrarAsync(_repositoryAnalisisSuelo, idActividad);
                        break;
                    case TipoActividadEnum.OtrasLabores :
                        await BorrarAsync(_repositoryOtrasLabores, idActividad);
                        break;
                    }
            }


        private async Task BorrarAsync<T>(IGenericRepository<T> repo, int idActividad) where T : EntityBaseWithLicencia
        {
            try
            {
                await repo.DeleteByIdAsync(idActividad);

            }
            catch (Exception e)
            {

            }

        }
    }
}
