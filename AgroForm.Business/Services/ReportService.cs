using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;
        private readonly IUserContext _userContext;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<OperationResult<List<ReporteComparativaCampoDto>>> GetComparativaCamposAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null)
        {
            try
            {
                var lotesQuery = _unitOfWork.Repository<Lote>().Query()
                    .Include(l => l.Campo)
                    .Include(l => l.Campania)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Siembras)
                            .ThenInclude(s => s.Cultivo)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Cosechas)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Fertilizaciones)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Pulverizaciones)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Riegos)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.Monitoreos)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.AnalisisSuelos)
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.OtrasLabores)
                    .Where(l => l.IdLicencia == _userContext.IdLicencia)
                    .AsQueryable();

                if (idCampania.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampania == idCampania.Value);
                else if (_userContext.IdCampaña.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampania == _userContext.IdCampaña.Value);

                if (idCampo.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampo == idCampo.Value);

                if (idLote.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.Id == idLote.Value);

                var lotes = await lotesQuery.ToListAsync();

                var result = new List<ReporteComparativaCampoDto>();

                foreach (var lote in lotes)
                {
                    var todasLasSiembras = lote.Siembras.ToList();
                    var todasLasCosechas = lote.Cosechas.ToList();

                    var ultimaSiembra = todasLasSiembras
                        .OrderByDescending(s => s.Fecha)
                        .FirstOrDefault();

                    var ultimaCosecha = todasLasCosechas
                        .OrderByDescending(c => c.Fecha)
                        .FirstOrDefault();

                    if (idCultivo.HasValue && ultimaSiembra != null && ultimaSiembra.IdCultivo != idCultivo.Value)
                        continue;

                    var totalFertKgHa = lote.Fertilizaciones
                        .Where(f => f.CantidadKgHa.HasValue)
                        .Sum(f => f.CantidadKgHa!.Value);

                    var totalPulvLtsHa = lote.Pulverizaciones
                        .Where(p => p.VolumenLitrosHa.HasValue)
                        .Sum(p => p.VolumenLitrosHa!.Value);

                    var costosARS = ObtenerCostosARS(lote);
                    var costosUSD = ObtenerCostosUSD(lote);

                    var rendimientoTonHa = ultimaCosecha?.RendimientoTonHa;
                    var superficieHa = lote.SuperficieHectareas ?? ultimaSiembra?.SuperficieHa ?? 0;
                    decimal? rendimientoTotal = rendimientoTonHa.HasValue && superficieHa > 0
                        ? rendimientoTonHa.Value * superficieHa
                        : null;

                    var costoPorHaARS = superficieHa > 0 ? costosARS / superficieHa : costosARS;
                    var costoPorHaUSD = superficieHa > 0 ? costosUSD / superficieHa : costosUSD;

                    var cantidadLabores = todasLasSiembras.Count
                        + todasLasCosechas.Count
                        + lote.Fertilizaciones.Count
                        + lote.Pulverizaciones.Count
                        + lote.Riegos.Count
                        + lote.Monitoreos.Count
                        + lote.AnalisisSuelos.Count
                        + lote.OtrasLabores.Count;

                    decimal? margenBrutoARS = null;
                    decimal? margenBrutoUSD = null;

                    result.Add(new ReporteComparativaCampoDto
                    {
                        IdCampo = lote.Campo?.Id ?? 0,
                        Campo = lote.Campo?.Nombre ?? "Sin campo",
                        IdLote = lote.Id,
                        Lote = lote.Nombre,
                        SuperficieHa = lote.SuperficieHectareas,

                        Cultivo = ultimaSiembra?.Cultivo?.Nombre,
                        IdCultivo = ultimaSiembra?.IdCultivo,

                        FechaSiembra = ultimaSiembra?.Fecha,
                        FechaCosecha = ultimaCosecha?.Fecha,

                        RendimientoTonHa = rendimientoTonHa,
                        RendimientoTotalTon = rendimientoTotal,

                        TotalFertilizantesKgHa = totalFertKgHa > 0 ? totalFertKgHa : null,
                        TotalPulverizacionesLtsHa = totalPulvLtsHa > 0 ? totalPulvLtsHa : null,

                        CostoTotalARS = costosARS > 0 ? costosARS : null,
                        CostoTotalUSD = costosUSD > 0 ? costosUSD : null,
                        CostoPorHaARS = costoPorHaARS > 0 ? Math.Round(costoPorHaARS, 2) : null,
                        CostoPorHaUSD = costoPorHaUSD > 0 ? Math.Round(costoPorHaUSD, 2) : null,

                        MargenBrutoARS = margenBrutoARS,
                        MargenBrutoUSD = margenBrutoUSD,

                        CantidadLabores = cantidadLabores
                    });
                }

                var conRendimiento = result
                    .Where(r => r.RendimientoTonHa.HasValue)
                    .OrderByDescending(r => r.RendimientoTonHa)
                    .ToList();

                for (int i = 0; i < conRendimiento.Count; i++)
                {
                    var item = result.First(r => r.IdLote == conRendimiento[i].IdLote);
                    item.RankingRendimiento = i + 1;
                }

                return OperationResult<List<ReporteComparativaCampoDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte comparativa de campos/lotes");
                return OperationResult<List<ReporteComparativaCampoDto>>.Failure(
                    $"Error al generar reporte: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // ============================================================
        // NUEVO: Reporte Integral del Campo
        // ============================================================

        public async Task<OperationResult<ReporteCampoIntegralDto>> GetReporteCampoIntegralAsync(
            int idCampo,
            int? idCampania = null)
        {
            try
            {
                // 1. Obtener el Campo con sus lotes
                var campo = await _unitOfWork.Repository<Campo>().Query()
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.Campania)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Cultivo)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Variedad)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Campania)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Siembras)
                                .ThenInclude(s => s.Cultivo)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Cosechas)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Fertilizaciones)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Pulverizaciones)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Riegos)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.Monitoreos)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.AnalisisSuelos)
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.OtrasLabores)
                    .Include(c => c.RegistrosClima)
                    .FirstOrDefaultAsync(c => c.Id == idCampo && c.IdLicencia == _userContext.IdLicencia);

                if (campo == null)
                    return OperationResult<ReporteCampoIntegralDto>.Failure("Campo no encontrado", "NOT_FOUND");

                // Determinar campaña activa
                int? idCampaniaEfectiva = idCampania ?? _userContext.IdCampaña;

                // Obtener todos los lotes del campo
                var lotes = campo.Lotes.ToList();

                // Obtener datos climáticos
                var registrosClima = campo.RegistrosClima?
                    .OrderByDescending(r => r.Fecha)
                    .ToList() ?? new List<RegistroClima>();

                // Obtener gastos del campo (de todas las campañas)
                var gastos = await _unitOfWork.Repository<Gasto>().Query()
                    .Where(g => g.IdLicencia == _userContext.IdLicencia)
                    .ToListAsync();

                // Construir el reporte
                var reporte = new ReporteCampoIntegralDto();

                // --- RESULTADOS POR SECCIÓN ---
                var activosCiclos = lotes
                    .SelectMany(l => l.CicloCultivos)
                    .Where(cc => !idCampaniaEfectiva.HasValue || cc.IdCampania == idCampaniaEfectiva.Value)
                    .ToList();

                var historicosCiclos = lotes
                    .SelectMany(l => l.CicloCultivos)
                    .Where(cc => !idCampaniaEfectiva.HasValue || cc.IdCampania != idCampaniaEfectiva.Value)
                    .ToList();

                var todosCiclos = lotes
                    .SelectMany(l => l.CicloCultivos)
                    .ToList();

                // --- 2.1 Resumen Ejecutivo ---
                reporte.ResumenEjecutivo = BuildResumenEjecutivo(campo, lotes, activosCiclos, registrosClima);

                // --- 2.2 Timeline Agronómico ---
                reporte.Timeline = BuildTimeline(todosCiclos, activosCiclos, idCampaniaEfectiva);

                // --- 2.3 Evolución del Cultivo ---
                reporte.EvolucionCultivo = BuildEvolucionCultivo(activosCiclos, registrosClima, todosCiclos);

                // --- 2.4 Análisis Climático ---
                reporte.AnalisisClimatico = BuildAnalisisClimatico(registrosClima);

                // --- 2.5 Análisis de Suelo ---
                reporte.AnalisisSuelo = BuildAnalisisSuelo(activosCiclos);

                // --- 2.6 Costos y Rentabilidad ---
                reporte.CostosRentabilidad = BuildCostosRentabilidad(lotes, activosCiclos, gastos, idCampaniaEfectiva);

                // --- 2.7 Rendimiento y Cosecha ---
                reporte.RendimientoCosecha = BuildRendimientoCosecha(activosCiclos, todosCiclos);

                // --- 2.8 Alertas Inteligentes ---
                reporte.Alertas = BuildAlertas(reporte.ResumenEjecutivo, reporte.AnalisisClimatico,
                    reporte.AnalisisSuelo, reporte.RendimientoCosecha, registrosClima);

                // --- 2.9 Historial Multi-Campaña ---
                reporte.HistorialMultiCampania = BuildHistorialMultiCampania(todosCiclos, lotes);

                return OperationResult<ReporteCampoIntegralDto>.SuccessResult(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte integral del campo {IdCampo}", idCampo);
                return OperationResult<ReporteCampoIntegralDto>.Failure(
                    $"Error al generar reporte integral: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // ============================================================
        // MÉTODOS PRIVADOS PARA CONSTRUIR CADA SECCIÓN
        // ============================================================

        private ResumenEjecutivoDto BuildResumenEjecutivo(
            Campo campo,
            List<Lote> lotes,
            List<CicloCultivo> activosCiclos,
            List<RegistroClima> registrosClima)
        {
            var resumen = new ResumenEjecutivoDto
            {
                Campo = campo.Nombre,
                Lote = lotes.FirstOrDefault()?.Nombre ?? "N/A",
                SuperficieHa = lotes.Sum(l => l.SuperficieHectareas ?? 0),
                Latitud = (decimal?)campo.Latitud,
                Longitud = (decimal?)campo.Longitud,
                CoordenadasPoligono = campo.CoordenadasPoligono
            };

            // Último ciclo activo
            var ultimoCiclo = activosCiclos
                .OrderByDescending(cc => cc.FechaInicio)
                .FirstOrDefault();

            if (ultimoCiclo != null)
            {
                resumen.CultivoActual = ultimoCiclo.Cultivo?.Nombre;
                resumen.Variedad = ultimoCiclo.Variedad?.Nombre;
                resumen.Campania = ultimoCiclo.Campania?.Nombre;

                var ultimaSiembra = ultimoCiclo.Siembras
                    .OrderByDescending(s => s.Fecha)
                    .FirstOrDefault();

                if (ultimaSiembra != null)
                {
                    resumen.FechaSiembra = ultimaSiembra.Fecha;
                    resumen.DiasDesdeSiembra = (int)(DateTime.UtcNow.Date - ultimaSiembra.Fecha.Date).TotalDays;
                }
            }

            // Última lluvia
            var ultimaLluvia = registrosClima
                .Where(r => r.TipoClima == TipoClima.Lluvia)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();

            if (ultimaLluvia != null)
            {
                var diasSinLluvia = (int)(DateTime.UtcNow.Date - ultimaLluvia.Fecha.Date).TotalDays;
                resumen.UltimaLluvia = diasSinLluvia <= 1
                    ? "Hoy"
                    : $"Hace {diasSinLluvia} días ({ultimaLluvia.Milimetros}mm)";
            }
            else
            {
                resumen.UltimaLluvia = "Sin registros";
            }

            // NDVI estimado (simulado basado en días desde siembra y estado)
            if (resumen.DiasDesdeSiembra.HasValue)
            {
                var dias = resumen.DiasDesdeSiembra.Value;
                // Simular curva de NDVI: sube hasta ~60 días, se mantiene, baja después de 120
                if (dias < 30)
                    resumen.NDVIPromedio = Math.Round(0.2m + (dias / 30m) * 0.4m, 2);
                else if (dias < 90)
                    resumen.NDVIPromedio = Math.Round(0.6m + ((dias - 30) / 60m) * 0.2m, 2);
                else if (dias < 150)
                    resumen.NDVIPromedio = Math.Round(0.8m - ((dias - 90) / 60m) * 0.3m, 2);
                else
                    resumen.NDVIPromedio = Math.Round(0.3m, 2);
            }

            // Estado general
            if (resumen.NDVIPromedio.HasValue)
            {
                if (resumen.NDVIPromedio >= 0.7m)
                {
                    resumen.EstadoGeneral = "Excelente";
                    resumen.ScoreSaludCultivo = 90;
                }
                else if (resumen.NDVIPromedio >= 0.5m)
                {
                    resumen.EstadoGeneral = "Bueno";
                    resumen.ScoreSaludCultivo = 70;
                }
                else if (resumen.NDVIPromedio >= 0.3m)
                {
                    resumen.EstadoGeneral = "Regular";
                    resumen.ScoreSaludCultivo = 50;
                }
                else
                {
                    resumen.EstadoGeneral = "Crítico";
                    resumen.ScoreSaludCultivo = 25;
                }
            }

            // Scores
            resumen.ScoreProductividad = activosCiclos
                .SelectMany(cc => cc.Cosechas)
                .Where(c => c.RendimientoTonHa.HasValue)
            .DefaultIfEmpty()
                .Average(c => c?.RendimientoTonHa) switch
                {
                    null => 50,
                    < 2 => 30,
                    < 3.5m => 60,
                    < 5 => 80,
                    _ => 95
                };

            resumen.ScoreHumedad = registrosClima.Any()
                ? 100 - (int)Math.Min(
                    registrosClima.Count(r => r.TipoClima == TipoClima.Lluvia && r.Milimetros > 0) * 5, 70)
                : 50;

            resumen.RiesgoActual = resumen.ScoreSaludCultivo < 40 ? "Alto"
                : resumen.ScoreSaludCultivo < 65 ? "Medio"
                : "Bajo";
            resumen.ScoreRiesgo = resumen.ScoreSaludCultivo;

            return resumen;
        }

        private List<TimelineEventoDto> BuildTimeline(
            List<CicloCultivo> todosCiclos,
            List<CicloCultivo> activosCiclos,
            int? idCampaniaEfectiva)
        {
            var eventos = new List<TimelineEventoDto>();
            int eventId = 1;

            void AddEventosFromCiclo(CicloCultivo ciclo, List<Siembra> siembras, string tipo, string icono, string color)
            {
                foreach (var s in siembras)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = s.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {ciclo.Cultivo?.Nombre ?? "N/A"}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                        Responsable = (s as dynamic)?.Usuario?.Nombre
                    });
                }
            }

            void AddCosechas(CicloCultivo ciclo, string tipo, string icono, string color)
            {
                foreach (var c in ciclo.Cosechas)
                {
                    var rendimiento = c.RendimientoTonHa.HasValue
                        ? $" - {c.RendimientoTonHa} tn/ha"
                        : "";
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = c.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo}{rendimiento}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            void AddLabores(CicloCultivo ciclo, List<Fertilizacion> labores, string tipo, string icono, string color)
            {
                foreach (var l in labores)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = l.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {(l.TipoFertilizante?.Nombre ?? "Sin especificar")} {(l.CantidadKgHa.HasValue ? $"({l.CantidadKgHa} kg/ha)" : "")}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            void AddPulverizaciones(CicloCultivo ciclo, string tipo, string icono, string color)
            {
                foreach (var p in ciclo.Pulverizaciones)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = p.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {(p.ProductoAgroquimico?.Nombre ?? "Sin producto")} {(p.VolumenLitrosHa.HasValue ? $"({p.VolumenLitrosHa} L/ha)" : "")}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            void AddMonitoreos(CicloCultivo ciclo, string tipo, string icono, string color)
            {
                foreach (var m in ciclo.Monitoreos)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = m.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {(m.Observacion ?? "Sin observación")}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            void AddRiegos(CicloCultivo ciclo, string tipo, string icono, string color)
            {
                foreach (var r in ciclo.Riegos)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = r.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {(r.VolumenAguaM3.HasValue ? $"{r.VolumenAguaM3} m³" : "Sin volumen")}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            void AddOtrasLabores(CicloCultivo ciclo, string tipo, string icono, string color)
            {
                foreach (var o in ciclo.OtrasLabores)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = o.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"{tipo} - {(o.Observacion ?? "Sin descripción")}",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            // Procesar ciclos activos primero, luego históricos
            var ciclosOrdenados = activosCiclos
                .OrderByDescending(cc => cc.FechaInicio)
                .ToList();

            foreach (var ciclo in ciclosOrdenados)
            {
                AddEventosFromCiclo(ciclo, ciclo.Siembras, "Siembra", "🌱", "#28a745");
                AddLabores(ciclo, ciclo.Fertilizaciones, "Fertilización", "🧪", "#17a2b8");
                AddPulverizaciones(ciclo, "Pulverización", "✈️", "#6f42c1");
                AddRiegos(ciclo, "Riego", "💧", "#007bff");
                AddMonitoreos(ciclo, "Monitoreo", "🔍", "#ffc107");
                AddCosechas(ciclo, "Cosecha", "🌾", "#fd7e14");
                AddOtrasLabores(ciclo, "Otra Labor", "🔧", "#6c757d");

                // Agregar análisis de suelo
                foreach (var a in ciclo.AnalisisSuelos)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = a.Fecha,
                        TipoActividad = "Análisis de Suelo",
                        Icono = "🔬",
                        Color = "#20c997",
                        Descripcion = $"Análisis de Suelo - pH: {a.PH?.ToString("N1") ?? "N/A"}, MO: {a.MateriaOrganica?.ToString("N1") ?? "N/A"}%",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            return eventos
                .OrderByDescending(e => e.Fecha)
                .ToList();
        }

        private EvolucionCultivoDto BuildEvolucionCultivo(
            List<CicloCultivo> activosCiclos,
            List<RegistroClima> registrosClima,
            List<CicloCultivo> todosCiclos)
        {
            var evolucion = new EvolucionCultivoDto();

            // Datos de evolución - combinar fechas de actividades con clima
            var fechas = new List<DateTime>();

            // Agregar fechas de actividades del ciclo activo
            foreach (var ciclo in activosCiclos)
            {
                foreach (var s in ciclo.Siembras) fechas.Add(s.Fecha);
                foreach (var c in ciclo.Cosechas) fechas.Add(c.Fecha);
                foreach (var f in ciclo.Fertilizaciones) fechas.Add(f.Fecha);
            }

            // Agregar fechas de clima
            foreach (var r in registrosClima)
                fechas.Add(r.Fecha);

            // Ordenar y tomar muestras
            fechas = fechas.Distinct().OrderBy(f => f).ToList();
            if (fechas.Count > 30)
            {
                // Tomar muestras espaciadas
                var step = fechas.Count / 30;
                fechas = fechas.Where((f, i) => i % step == 0).ToList();
            }

            foreach (var fecha in fechas)
            {
                var clima = registrosClima.FirstOrDefault(r => r.Fecha.Date == fecha.Date);
                evolucion.Evolucion.Add(new DatoEvolucion
                {
                    Fecha = fecha,
                    Temperatura = null, // Open-Meteo data would go here
                    Precipitacion = clima?.Milimetros,
                    Humedad = null,
                    NDVI = null
                });
            }

            // Comparativa entre campañas
            var ciclosPorCampania = todosCiclos
                .GroupBy(cc => cc.IdCampania)
                .OrderByDescending(g => g.First().Campania?.Nombre)
                .ToList();

            if (ciclosPorCampania.Count >= 2)
            {
                var actual = ciclosPorCampania.First();
                var anterior = ciclosPorCampania.Skip(1).First();

                evolucion.Comparativa = new ComparativaEvolucionDto
                {
                    CampaniaAnterior = anterior.First().Campania?.Nombre ?? "Anterior",
                    NDVIPromedioActual = null, // Requiere datos satelitales
                    NDVIPromedioAnterior = null,
                    RendimientoActual = actual
                        .SelectMany(cc => cc.Cosechas)
                        .Where(c => c.RendimientoTonHa.HasValue)
                        .DefaultIfEmpty()
                        .Average(c => c?.RendimientoTonHa),
                    RendimientoAnterior = anterior
                        .SelectMany(cc => cc.Cosechas)
                        .Where(c => c.RendimientoTonHa.HasValue)
                        .DefaultIfEmpty()
                        .Average(c => c?.RendimientoTonHa)
                };
            }

            return evolucion;
        }

        private AnalisisClimaticoDto BuildAnalisisClimatico(List<RegistroClima> registrosClima)
        {
            var analisis = new AnalisisClimaticoDto();

            if (!registrosClima.Any())
            {
                analisis.BalanceHidrico = "Sin datos";
                return analisis;
            }

            var lluvias = registrosClima.Where(r => r.TipoClima == TipoClima.Lluvia).ToList();

            // Lluvia acumulada (últimos 30 días)
            var ultimos30Dias = DateTime.UtcNow.Date.AddDays(-30);
            analisis.LluviaAcumulada = lluvias
                .Where(r => r.Fecha >= ultimos30Dias)
                .Sum(r => r.Milimetros);

            // Días sin lluvia
            var ultimaLluvia = lluvias
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();
            analisis.DiasSinLluvia = ultimaLluvia != null
                ? (int)(DateTime.UtcNow.Date - ultimaLluvia.Fecha.Date).TotalDays
                : 999;

            // Heladas (granizo)
            analisis.CantidadHeladas = registrosClima.Count(r => r.TipoClima == TipoClima.Granizo);

            // Balance hídrico
            if (analisis.LluviaAcumulada < 20)
            {
                analisis.BalanceHidrico = "Déficit hídrico";
                analisis.EstresHidrico = "Alto";
            }
            else if (analisis.LluviaAcumulada < 50)
            {
                analisis.BalanceHidrico = "Moderado";
                analisis.EstresHidrico = "Medio";
            }
            else if (analisis.LluviaAcumulada < 100)
            {
                analisis.BalanceHidrico = "Normal";
                analisis.EstresHidrico = "Bajo";
            }
            else
            {
                analisis.BalanceHidrico = "Exceso hídrico";
                analisis.EstresHidrico = "Alto";
            }

            // Registros climáticos detallados
            analisis.Registros = registrosClima
                .OrderByDescending(r => r.Fecha)
                .Take(90)
                .Select(r => new DatoClimatico
                {
                    Fecha = r.Fecha,
                    Precipitacion = r.Milimetros,
                    TipoClima = r.TipoClima.ToString()
                })
                .ToList();

            return analisis;
        }

        private AnalisisSueloDto BuildAnalisisSuelo(List<CicloCultivo> activosCiclos)
        {
            var analisis = new AnalisisSueloDto();

            // Obtener el último análisis de suelo
            var ultimoAnalisis = activosCiclos
                .SelectMany(cc => cc.AnalisisSuelos)
                .OrderByDescending(a => a.Fecha)
                .FirstOrDefault();

            if (ultimoAnalisis == null)
                return analisis;

            analisis.FechaAnalisis = ultimoAnalisis.Fecha;
            analisis.ProfundidadCm = ultimoAnalisis.ProfundidadCm;
            analisis.Textura = ultimoAnalisis.Textura;

            // Interpretar cada parámetro
            analisis.PH = InterpretarParametro("pH", ultimoAnalisis.PH, "",
                v => v < 5.5m ? ("Ácido", "Bajo", "Aplicar enmienda calcárea para elevar el pH a niveles óptimos (6.0-7.0)")
                    : v <= 7.0m ? ("Óptimo", "Óptimo", "Mantener el pH actual, no requiere corrección.")
                    : ("Alcalino", "Alto", "Considerar aplicación de azufre o materia orgánica para reducir el pH."));

            analisis.MateriaOrganica = InterpretarParametro("MO", ultimoAnalisis.MateriaOrganica, "%",
                v => v < 2m ? ("Bajo", "Bajo", "Incorporar materia orgánica mediante abonos verdes, compost o estiércol.")
                    : v <= 5m ? ("Medio", "Medio", "Mantener prácticas de conservación de materia orgánica.")
                    : ("Alto", "Alto", "Nivel óptimo de materia orgánica. Continuar con las prácticas actuales."));

            analisis.Nitrogeno = InterpretarParametro("N", ultimoAnalisis.Nitrogeno, "ppm",
                v => v < 15m ? ("Bajo", "Bajo", "Aplicar fertilización nitrogenada (urea, CAN, etc.) para alcanzar niveles óptimos (15-30 ppm).")
                    : v <= 30m ? ("Óptimo", "Óptimo", "Nivel de nitrógeno adecuado. Monitorear durante el desarrollo del cultivo.")
                    : ("Alto", "Alto", "Exceso de nitrógeno. Reducir fertilización nitrogenada para evitar contaminación."));

            analisis.Fosforo = InterpretarParametro("P", ultimoAnalisis.Fosforo, "ppm",
                v => v < 10m ? ("Bajo", "Bajo", "Aplicar fósforo (superfosfato triple, fosfato diamónico) según requerimiento del cultivo.")
                    : v <= 20m ? ("Óptimo", "Óptimo", "Nivel de fósforo adecuado para la mayoría de los cultivos.")
                    : ("Alto", "Alto", "Nivel elevado de fósforo. Evitar aplicaciones adicionales para prevenir impacto ambiental."));

            analisis.Potasio = InterpretarParametro("K", ultimoAnalisis.Potasio, "meq/100g",
                v => v < 0.3m ? ("Bajo", "Bajo", "Aplicar potasio (cloruro de potasio, sulfato de potasio) para alcanzar niveles óptimos.")
                    : v <= 0.7m ? ("Óptimo", "Óptimo", "Nivel de potasio adecuado. Mantener monitoreo periódico.")
                    : ("Alto", "Alto", "Nivel elevado de potasio. Verificar posible desbalance con otros cationes."));

            analisis.ConductividadElectrica = InterpretarParametro("CE", ultimoAnalisis.ConductividadElectrica, "dS/m",
                v => v < 0.5m ? ("Muy baja", "Bajo", "Suelo con muy baja salinidad. Verificar disponibilidad de nutrientes.")
                    : v <= 1.5m ? ("Normal", "Óptimo", "Salinidad adecuada para la mayoría de los cultivos.")
                    : ("Alta", "Alto", "Salinidad elevada. Considerar cultivos tolerantes y prácticas de lavado de suelos."));

            analisis.CIC = InterpretarParametro("CIC", ultimoAnalisis.CIC, "meq/100g",
                v => v < 10m ? ("Baja", "Bajo", "Suelo con baja capacidad de intercambio catiónico. Mejorar con materia orgánica.")
                    : v <= 25m ? ("Media", "Medio", "Capacidad de intercambio catiónico adecuada.")
                    : ("Alta", "Alto", "Alta capacidad de intercambio catiónico. Buena retención de nutrientes."));

            // Generar recomendaciones generales
            var todosParametros = new[] {
                (analisis.PH, "pH"),
                (analisis.MateriaOrganica, "Materia Orgánica"),
                (analisis.Nitrogeno, "Nitrógeno"),
                (analisis.Fosforo, "Fósforo"),
                (analisis.Potasio, "Potasio"),
                (analisis.ConductividadElectrica, "Conductividad Eléctrica"),
                (analisis.CIC, "CIC")
            };

            foreach (var (param, nombre) in todosParametros)
            {
                if (param != null && param.Nivel == "Bajo" && !string.IsNullOrEmpty(param.Recomendacion))
                    analisis.Recomendaciones.Add($"{nombre}: {param.Recomendacion}");
            }

            return analisis;
        }

        private ParametroSueloDto InterpretarParametro(
            string nombre,
            decimal? valor,
            string unidad,
            Func<decimal, (string interpretacion, string nivel, string recomendacion)> interpretar)
        {
            if (!valor.HasValue)
                return new ParametroSueloDto { Unidad = unidad };

            var (interpretacion, nivel, recomendacion) = interpretar(valor.Value);

            return new ParametroSueloDto
            {
                Valor = valor,
                Unidad = unidad,
                Interpretacion = interpretacion,
                Nivel = nivel,
                Recomendacion = recomendacion
            };
        }

        private CostosRentabilidadDto BuildCostosRentabilidad(
            List<Lote> lotes,
            List<CicloCultivo> activosCiclos,
            List<Gasto> gastos,
            int? idCampaniaEfectiva)
        {
            var costos = new CostosRentabilidadDto();
            var desglose = new CostosDesglosadosDto();
            decimal totalARS = 0;
            decimal totalUSD = 0;

            foreach (var ciclo in activosCiclos)
            {
                foreach (var s in ciclo.Siembras)
                {
                    desglose.SiembraARS += s.CostoARS.GetValueOrDefault();
                    desglose.SiembraUSD += s.CostoUSD.GetValueOrDefault();
                }
                foreach (var f in ciclo.Fertilizaciones)
                {
                    desglose.FertilizacionARS += f.CostoARS.GetValueOrDefault();
                    desglose.FertilizacionUSD += f.CostoUSD.GetValueOrDefault();
                }
                foreach (var p in ciclo.Pulverizaciones)
                {
                    desglose.PulverizacionARS += p.CostoARS.GetValueOrDefault();
                    desglose.PulverizacionUSD += p.CostoUSD.GetValueOrDefault();
                }
                foreach (var r in ciclo.Riegos)
                {
                    desglose.RiegoARS += r.CostoARS.GetValueOrDefault();
                    desglose.RiegoUSD += r.CostoUSD.GetValueOrDefault();
                }
                foreach (var c in ciclo.Cosechas)
                {
                    desglose.CosechaARS += c.CostoARS.GetValueOrDefault();
                    desglose.CosechaUSD += c.CostoUSD.GetValueOrDefault();
                }
                foreach (var m in ciclo.Monitoreos)
                {
                    desglose.MonitoreoARS += m.CostoARS.GetValueOrDefault();
                    desglose.MonitoreoUSD += m.CostoUSD.GetValueOrDefault();
                }
                foreach (var a in ciclo.AnalisisSuelos)
                {
                    desglose.AnalisisSueloARS += a.CostoARS.GetValueOrDefault();
                    desglose.AnalisisSueloUSD += a.CostoUSD.GetValueOrDefault();
                }
                foreach (var o in ciclo.OtrasLabores)
                {
                    desglose.OtrasLaboresARS += o.CostoARS.GetValueOrDefault();
                    desglose.OtrasLaboresUSD += o.CostoUSD.GetValueOrDefault();
                }
            }

            totalARS = desglose.SiembraARS + desglose.FertilizacionARS + desglose.PulverizacionARS
                     + desglose.RiegoARS + desglose.CosechaARS + desglose.MonitoreoARS
                     + desglose.AnalisisSueloARS + desglose.OtrasLaboresARS;

            totalUSD = desglose.SiembraUSD + desglose.FertilizacionUSD + desglose.PulverizacionUSD
                     + desglose.RiegoUSD + desglose.CosechaUSD + desglose.MonitoreoUSD
                     + desglose.AnalisisSueloUSD + desglose.OtrasLaboresUSD;

            // Agregar gastos generales si coinciden con la campaña
            var gastosFiltrados = idCampaniaEfectiva.HasValue
                ? gastos.Where(g => g.IdCampania == idCampaniaEfectiva.Value).ToList()
                : gastos;

            foreach (var g in gastosFiltrados)
            {
                totalARS += g.CostoARS.GetValueOrDefault();
                totalUSD += g.CostoUSD.GetValueOrDefault();
            }

            var superficieTotal = lotes.Sum(l => l.SuperficieHectareas ?? 0);

            costos.Desglose = desglose;
            costos.CostoTotalARS = totalARS;
            costos.CostoTotalUSD = totalUSD;
            costos.CostoPorHaARS = superficieTotal > 0 ? Math.Round(totalARS / superficieTotal, 2) : totalARS;
            costos.CostoPorHaUSD = superficieTotal > 0 ? Math.Round(totalUSD / superficieTotal, 2) : totalUSD;

            // Margen estimado (usando rendimiento * precio estimado - costos)
            var ultimaCosecha = activosCiclos
                .SelectMany(cc => cc.Cosechas)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefault();

            if (ultimaCosecha?.RendimientoTonHa.HasValue == true && superficieTotal > 0)
            {
                var produccionTotal = ultimaCosecha.RendimientoTonHa.Value * superficieTotal;
                // Precio estimado de referencia (esto debería venir de una config)
                const decimal precioReferenciaARS = 250000; // $ARS por tonelada
                const decimal precioReferenciaUSD = 250;    // USD por tonelada

                var ingresoARS = produccionTotal * precioReferenciaARS;
                var ingresoUSD = produccionTotal * precioReferenciaUSD;

                costos.MargenEstimadoARS = Math.Round(ingresoARS - totalARS, 2);
                costos.MargenEstimadoUSD = Math.Round(ingresoUSD - totalUSD, 2);

                if (totalARS > 0)
                    costos.RentabilidadProyectada = Math.Round((ingresoARS - totalARS) / totalARS * 100, 1);
            }

            return costos;
        }

        private RendimientoCosechaDto BuildRendimientoCosecha(
            List<CicloCultivo> activosCiclos,
            List<CicloCultivo> todosCiclos)
        {
            var rendimiento = new RendimientoCosechaDto();

            // Última cosecha del ciclo activo
            var ultimaCosecha = activosCiclos
                .SelectMany(cc => cc.Cosechas)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefault();

            if (ultimaCosecha != null)
            {
                rendimiento.RendimientoTonHa = ultimaCosecha.RendimientoTonHa;
                rendimiento.HumedadCosecha = ultimaCosecha.HumedadGrano;
                rendimiento.SuperficieCosechadaHa = ultimaCosecha.SuperficieCosechadaHa;
                rendimiento.FechaCosecha = ultimaCosecha.Fecha;

                // Producción total
                var supCosechada = ultimaCosecha.SuperficieCosechadaHa ?? 0;
                if (ultimaCosecha.RendimientoTonHa.HasValue && supCosechada > 0)
                    rendimiento.ProduccionTotalTon = Math.Round(ultimaCosecha.RendimientoTonHa.Value * supCosechada, 2);
                else if (ultimaCosecha.RendimientoTonHa.HasValue)
                    rendimiento.ProduccionTotalTon = ultimaCosecha.RendimientoTonHa;
            }

            // Histórico de rendimientos
            var cosechasPorCampania = todosCiclos
                .SelectMany(cc => cc.Cosechas.Select(c => new
                {
                    Nombre = cc.Campania?.Nombre,
                    Cultivo = cc.Cultivo?.Nombre,
                    c.RendimientoTonHa
                }))
                .Where(c => c.RendimientoTonHa.HasValue)
                .GroupBy(c => new { c.Nombre, c.Cultivo })
                .Select(g => new DatoRendimientoHistorico
                {
                    Campania = g.Key.Nombre ?? "N/A",
                    Cultivo = g.Key.Cultivo,
                    RendimientoTonHa = Math.Round(g.Average(c => c.RendimientoTonHa!.Value), 2)
                })
                .OrderByDescending(h => h.Campania)
                .ToList();

            rendimiento.Historico = cosechasPorCampania;

            return rendimiento;
        }

        private List<AlertaDto> BuildAlertas(
            ResumenEjecutivoDto resumen,
            AnalisisClimaticoDto clima,
            AnalisisSueloDto suelo,
            RendimientoCosechaDto rendimiento,
            List<RegistroClima> registrosClima)
        {
            var alertas = new List<AlertaDto>();

            // 1. Alerta de falta de lluvia
            if (clima.DiasSinLluvia.HasValue && clima.DiasSinLluvia.Value > 7)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Falta de lluvia",
                    Severidad = clima.DiasSinLluvia.Value > 14 ? "Alta" : "Media",
                    Mensaje = $"{clima.DiasSinLluvia} días sin precipitaciones significativas (>5mm)",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Evaluar necesidad de riego complementario. Monitorear estrés hídrico en el cultivo.",
                    Icono = "ph-warning-circle"
                });
            }

            // 2. Alerta de exceso hídrico
            if (clima.LluviaAcumulada.HasValue && clima.LluviaAcumulada.Value > 100)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Exceso hídrico",
                    Severidad = "Alta",
                    Mensaje = $"Lluvia acumulada de {clima.LluviaAcumulada.Value}mm en los últimos 30 días",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Verificar drenaje del lote. Monitorear aparición de enfermedades fúngicas.",
                    Icono = "ph-warning"
                });
            }

            // 3. Alerta de caída de NDVI
            if (resumen.NDVIPromedio.HasValue && resumen.NDVIPromedio.Value < 0.3m)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "NDVI crítico",
                    Severidad = "Alta",
                    Mensaje = $"NDVI estimado en {resumen.NDVIPromedio.Value}, indica baja vitalidad del cultivo",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Evaluar causa: déficit hídrico, plaga, enfermedad o deficiencia nutricional.",
                    Icono = "ph-flask"
                });
            }

            // 4. Alerta de riesgo de helada (granizo registrado)
            if (clima.CantidadHeladas.HasValue && clima.CantidadHeladas.Value > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Heladas registradas",
                    Severidad = "Alta",
                    Mensaje = $"Se registraron {clima.CantidadHeladas} eventos de granizo/helada",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Evaluar daños en el cultivo. Considerar seguros agrícolas para próxima campaña.",
                    Icono = "ph-snowflake"
                });
            }

            // 5. Alerta de rendimiento bajo
            if (rendimiento.Historico.Count >= 2)
            {
                var promedioHistorico = rendimiento.Historico
                    .Where(h => h.RendimientoTonHa.HasValue)
                    .Average(h => h.RendimientoTonHa!.Value);

                if (rendimiento.RendimientoTonHa.HasValue && promedioHistorico > 0
                    && rendimiento.RendimientoTonHa.Value < promedioHistorico * 0.7m)
                {
                    alertas.Add(new AlertaDto
                    {
                        Tipo = "Bajo rendimiento",
                        Severidad = "Media",
                        Mensaje = $"Rendimiento actual ({rendimiento.RendimientoTonHa} tn/ha) está por debajo del 70% del histórico ({promedioHistorico:N2} tn/ha)",
                        Fecha = DateTime.UtcNow,
                        Recomendacion = "Analizar causas del bajo rendimiento: calidad de semilla, manejo, clima o suelo.",
                        Icono = "ph-trend-down"
                    });
                }
            }

            // 6. Alerta de parámetros de suelo
            if (suelo.PH?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Suelo ácido",
                    Severidad = "Media",
                    Mensaje = $"pH del suelo en {suelo.PH.Valor:N1} - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.PH.Recomendacion,
                    Icono = "ph-flask"
                });
            }

            if (suelo.Nitrogeno?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Deficiencia de Nitrógeno",
                    Severidad = "Media",
                    Mensaje = $"Nivel de Nitrógeno: {suelo.Nitrogeno.Valor:N0} ppm - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.Nitrogeno.Recomendacion,
                    Icono = "ph-flask"
                });
            }

            if (suelo.Fosforo?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Deficiencia de Fósforo",
                    Severidad = "Media",
                    Mensaje = $"Nivel de Fósforo: {suelo.Fosforo.Valor:N0} ppm - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.Fosforo.Recomendacion,
                    Icono = "ph-flask"
                });
            }

            return alertas;
        }

        private List<HistorialCampaniaDto> BuildHistorialMultiCampania(
            List<CicloCultivo> todosCiclos,
            List<Lote> lotes)
        {
            var historial = new List<HistorialCampaniaDto>();

            var ciclosPorCampania = todosCiclos
                .GroupBy(cc => new { cc.IdCampania, Campania = cc.Campania?.Nombre ?? "N/A" })
                .OrderByDescending(g => g.Key.Campania)
                .ToList();

            foreach (var grupo in ciclosPorCampania)
            {
                var cicloPrincipal = grupo.FirstOrDefault();
                var todasLasCosechas = grupo.SelectMany(cc => cc.Cosechas).ToList();
                var todasLasSiembras = grupo.SelectMany(cc => cc.Siembras).ToList();

                var entry = new HistorialCampaniaDto
                {
                    IdCampania = grupo.Key.IdCampania,
                    Campania = grupo.Key.Campania,
                    Cultivo = cicloPrincipal?.Cultivo?.Nombre ?? "N/A",
                    CantidadLabores = grupo.Sum(cc =>
                        cc.Siembras.Count + cc.Cosechas.Count + cc.Fertilizaciones.Count
                        + cc.Pulverizaciones.Count + cc.Riegos.Count + cc.Monitoreos.Count
                        + cc.AnalisisSuelos.Count + cc.OtrasLabores.Count),
                    RendimientoTonHa = todasLasCosechas
                        .Where(c => c.RendimientoTonHa.HasValue)
                        .DefaultIfEmpty()
                        .Average(c => c?.RendimientoTonHa)
                };

                // Costos
                decimal costosARS = 0, costosUSD = 0;
                foreach (var ciclo in grupo)
                {
                    foreach (var s in ciclo.Siembras)
                    {
                        costosARS += s.CostoARS.GetValueOrDefault();
                        costosUSD += s.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var c in ciclo.Cosechas)
                    {
                        costosARS += c.CostoARS.GetValueOrDefault();
                        costosUSD += c.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var f in ciclo.Fertilizaciones)
                    {
                        costosARS += f.CostoARS.GetValueOrDefault();
                        costosUSD += f.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var p in ciclo.Pulverizaciones)
                    {
                        costosARS += p.CostoARS.GetValueOrDefault();
                        costosUSD += p.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var r in ciclo.Riegos)
                    {
                        costosARS += r.CostoARS.GetValueOrDefault();
                        costosUSD += r.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var m in ciclo.Monitoreos)
                    {
                        costosARS += m.CostoARS.GetValueOrDefault();
                        costosUSD += m.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var a in ciclo.AnalisisSuelos)
                    {
                        costosARS += a.CostoARS.GetValueOrDefault();
                        costosUSD += a.CostoUSD.GetValueOrDefault();
                    }
                    foreach (var o in ciclo.OtrasLabores)
                    {
                        costosARS += o.CostoARS.GetValueOrDefault();
                        costosUSD += o.CostoUSD.GetValueOrDefault();
                    }
                }
                entry.CostoTotalARS = costosARS;
                entry.CostoTotalUSD = costosUSD;

                historial.Add(entry);
            }

            return historial;
        }

        // ============================================================
        // MÉTODOS EXISTENTES DE CÁLCULO DE COSTOS
        // ============================================================

        private decimal ObtenerCostosARS(Lote lote)
        {
            decimal total = 0;

            foreach (var s in lote.Siembras) total += s.CostoARS.GetValueOrDefault();
            foreach (var c in lote.Cosechas) total += c.CostoARS.GetValueOrDefault();
            foreach (var f in lote.Fertilizaciones) total += f.CostoARS.GetValueOrDefault();
            foreach (var p in lote.Pulverizaciones) total += p.CostoARS.GetValueOrDefault();
            foreach (var r in lote.Riegos) total += r.CostoARS.GetValueOrDefault();
            foreach (var m in lote.Monitoreos) total += m.CostoARS.GetValueOrDefault();
            foreach (var a in lote.AnalisisSuelos) total += a.CostoARS.GetValueOrDefault();
            foreach (var o in lote.OtrasLabores) total += o.CostoARS.GetValueOrDefault();

            return total;
        }

        private decimal ObtenerCostosUSD(Lote lote)
        {
            decimal total = 0;

            foreach (var s in lote.Siembras) total += s.CostoUSD.GetValueOrDefault();
            foreach (var c in lote.Cosechas) total += c.CostoUSD.GetValueOrDefault();
            foreach (var f in lote.Fertilizaciones) total += f.CostoUSD.GetValueOrDefault();
            foreach (var p in lote.Pulverizaciones) total += p.CostoUSD.GetValueOrDefault();
            foreach (var r in lote.Riegos) total += r.CostoUSD.GetValueOrDefault();
            foreach (var m in lote.Monitoreos) total += m.CostoUSD.GetValueOrDefault();
            foreach (var a in lote.AnalisisSuelos) total += a.CostoUSD.GetValueOrDefault();
            foreach (var o in lote.OtrasLabores) total += o.CostoUSD.GetValueOrDefault();

            return total;
        }
    }
}
