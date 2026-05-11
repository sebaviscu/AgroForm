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
                    .Include(l => l.CicloCultivos)
                        .ThenInclude(c => c.SiloBolsas)
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
                    // Activities are loaded through CicloCultivos, not directly on Lote
                    var todosLosCiclos = lote.CicloCultivos.ToList();

                    var todasLasSiembras = todosLosCiclos
                        .SelectMany(cc => cc.Siembras)
                        .ToList();

                    var todasLasCosechas = todosLosCiclos
                        .SelectMany(cc => cc.Cosechas)
                        .ToList();

                    var todasLasFertilizaciones = todosLosCiclos
                        .SelectMany(cc => cc.Fertilizaciones)
                        .ToList();

                    var todasLasPulverizaciones = todosLosCiclos
                        .SelectMany(cc => cc.Pulverizaciones)
                        .ToList();

                    var todosLosRiegos = todosLosCiclos
                        .SelectMany(cc => cc.Riegos)
                        .ToList();

                    var todosLosMonitoreos = todosLosCiclos
                        .SelectMany(cc => cc.Monitoreos)
                        .ToList();

                    var todosLosAnalisisSuelos = todosLosCiclos
                        .SelectMany(cc => cc.AnalisisSuelos)
                        .ToList();

                    var todasLasOtrasLabores = todosLosCiclos
                        .SelectMany(cc => cc.OtrasLabores)
                        .ToList();

                    var todosLosSiloBolsas = todosLosCiclos
                        .SelectMany(cc => cc.SiloBolsas)
                        .ToList();

                    var ultimaSiembra = todasLasSiembras
                        .OrderByDescending(s => s.Fecha)
                        .FirstOrDefault();

                    var ultimaCosecha = todasLasCosechas
                        .OrderByDescending(c => c.Fecha)
                        .FirstOrDefault();

                    if (idCultivo.HasValue && ultimaSiembra != null && ultimaSiembra.IdCultivo != idCultivo.Value)
                        continue;

                    var totalFertKgHa = todasLasFertilizaciones
                        .Where(f => f.CantidadKgHa.HasValue)
                        .Sum(f => f.CantidadKgHa!.Value);

                    var totalPulvLtsHa = todasLasPulverizaciones
                        .Where(p => p.VolumenLitrosHa.HasValue)
                        .Sum(p => p.VolumenLitrosHa!.Value);

                    var costosARS = ObtenerCostosARS(lote, todosLosCiclos);
                    var costosUSD = ObtenerCostosUSD(lote, todosLosCiclos);

                    var rendimientoTonHa = ultimaCosecha?.RendimientoTonHa;
                    var superficieHa = lote.SuperficieHectareas ?? ultimaSiembra?.SuperficieHa ?? 0;
                    decimal? rendimientoTotal = rendimientoTonHa.HasValue && superficieHa > 0
                        ? rendimientoTonHa.Value * superficieHa
                        : null;

                    var costoPorHaARS = superficieHa > 0 ? costosARS / superficieHa : costosARS;
                    var costoPorHaUSD = superficieHa > 0 ? costosUSD / superficieHa : costosUSD;

                    var cantidadLabores = todasLasSiembras.Count
                        + todasLasCosechas.Count
                        + todasLasFertilizaciones.Count
                        + todasLasPulverizaciones.Count
                        + todosLosRiegos.Count
                        + todosLosMonitoreos.Count
                        + todosLosAnalisisSuelos.Count
                        + todasLasOtrasLabores.Count;

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

        public async Task<OperationResult<ComparativaCamposDto>> GetComparativaCamposIntegralAsync(
            int idCampoPrincipal,
            int? idCampoSecundario,
            int? idCampania = null)
        {
            try
            {
                // Load both campos with all related data
                async Task<CampoComparativaDto?> LoadCampoComparativa(int idCampo)
                {
                    var campo = await _unitOfWork.Repository<Campo>().Query()
                        .Include(c => c.Lotes)
                            .ThenInclude(l => l.CicloCultivos)
                                .ThenInclude(cc => cc.Cultivo)
                        .Include(c => c.Lotes)
                            .ThenInclude(l => l.CicloCultivos)
                                .ThenInclude(cc => cc.Siembras)
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
                        .Include(c => c.Lotes)
                            .ThenInclude(l => l.CicloCultivos)
                                .ThenInclude(cc => cc.SiloBolsas)
                        .FirstOrDefaultAsync(c => c.Id == idCampo && c.IdLicencia == _userContext.IdLicencia);

                    if (campo == null) return null;

                    var lotes = campo.Lotes.ToList();
                    var superficieHa = lotes.Sum(l => l.SuperficieHectareas ?? 0);

                    // Filter cycles by campaign if specified
                    var ciclos = lotes
                        .SelectMany(l => l.CicloCultivos)
                        .Where(cc => !idCampania.HasValue || cc.IdCampania == idCampania.Value)
                        .ToList();

                    var todasLasSiembras = ciclos.SelectMany(cc => cc.Siembras).ToList();
                    var ultimaSiembra = todasLasSiembras.OrderByDescending(s => s.Fecha).FirstOrDefault();
                    var todasLasCosechas = ciclos.SelectMany(cc => cc.Cosechas).ToList();
                    var ultimaCosecha = todasLasCosechas.OrderByDescending(c => c.Fecha).FirstOrDefault();

                    // Calculate costs
                    var desglose = new CostosDesglosadosDto();
                    decimal totalARS = 0, totalUSD = 0;
                    foreach (var ciclo in ciclos)
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
                        foreach (var sb in ciclo.SiloBolsas)
                        {
                            desglose.SiloBolsasARS += sb.CostoARS.GetValueOrDefault();
                            desglose.SiloBolsasUSD += sb.CostoUSD.GetValueOrDefault();
                        }
                    }
                    totalARS = desglose.SiembraARS + desglose.FertilizacionARS + desglose.PulverizacionARS +
                               desglose.RiegoARS + desglose.CosechaARS + desglose.MonitoreoARS +
                               desglose.AnalisisSueloARS + desglose.OtrasLaboresARS + desglose.SiloBolsasARS;
                    totalUSD = desglose.SiembraUSD + desglose.FertilizacionUSD + desglose.PulverizacionUSD +
                               desglose.RiegoUSD + desglose.CosechaUSD + desglose.MonitoreoUSD +
                               desglose.AnalisisSueloUSD + desglose.OtrasLaboresUSD + desglose.SiloBolsasUSD;

                    var costoPorHaARS = superficieHa > 0 ? totalARS / superficieHa : 0;

                    // Count labores
                    var cantidadLabores = ciclos.Sum(cc =>
                        cc.Siembras.Count + cc.Cosechas.Count + cc.Fertilizaciones.Count +
                        cc.Pulverizaciones.Count + cc.Riegos.Count + cc.Monitoreos.Count +
                        cc.AnalisisSuelos.Count + cc.OtrasLabores.Count + cc.SiloBolsas.Count);

                    // Count alerts from data
                    var cantidadAlertas = 0;
                    var ultimoAnalisis = ciclos.SelectMany(cc => cc.AnalisisSuelos)
                        .OrderByDescending(a => a.Fecha).FirstOrDefault();
                    if (ultimoAnalisis != null)
                    {
                        if (ultimoAnalisis.PH < 5.5m) cantidadAlertas++;
                        if (ultimoAnalisis.Nitrogeno < 15m) cantidadAlertas++;
                        if (ultimoAnalisis.Fosforo < 10m) cantidadAlertas++;
                    }

                    // Rendimiento histórico
                    var rendimientoHistorico = ciclos
                        .GroupBy(cc => cc.Campania?.Nombre ?? "N/A")
                        .Select(g => new DatoRendimientoHistorico
                        {
                            Campania = g.Key,
                            Cultivo = g.First().Cultivo?.Nombre,
                            RendimientoTonHa = g.SelectMany(cc => cc.Cosechas)
                                .Where(c => c.RendimientoTonHa.HasValue)
                                .DefaultIfEmpty()
                                .Average(c => c?.RendimientoTonHa)
                        })
                        .Where(r => r.RendimientoTonHa.HasValue)
                        .OrderByDescending(r => r.Campania)
                        .ToList();

                    // Estado general
                    var estadoGeneral = "Sin datos";
                    if (todasLasSiembras.Any())
                    {
                        var diasDesdeSiembra = (int)(DateTime.UtcNow.Date - ultimaSiembra!.Fecha.Date).TotalDays;
                        if (diasDesdeSiembra < 30) estadoGeneral = "Implantación";
                        else if (diasDesdeSiembra < 90) estadoGeneral = "Crecimiento";
                        else if (diasDesdeSiembra < 150) estadoGeneral = "Maduración";
                        else estadoGeneral = "Finalizado";
                    }

                    return new CampoComparativaDto
                    {
                        IdCampo = campo.Id,
                        Nombre = campo.Nombre,
                        SuperficieHa = superficieHa,
                        CultivoPrincipal = ultimaSiembra?.Cultivo?.Nombre ?? "Sin cultivo",
                        CostoTotalARS = totalARS,
                        CostoPorHaARS = Math.Round(costoPorHaARS, 2),
                        RendimientoTonHa = ultimaCosecha?.RendimientoTonHa,
                        RentabilidadProyectada = null,
                        CantidadLabores = cantidadLabores,
                        CantidadAlertas = cantidadAlertas,
                        EstadoGeneral = estadoGeneral,
                        DesgloseCostos = desglose,
                        RendimientoHistorico = rendimientoHistorico
                    };
                }

                var campoPrincipal = await LoadCampoComparativa(idCampoPrincipal);
                if (campoPrincipal == null)
                    return OperationResult<ComparativaCamposDto>.Failure("Campo principal no encontrado", "NOT_FOUND");

                CampoComparativaDto? campoSecundario = null;
                if (idCampoSecundario.HasValue)
                    campoSecundario = await LoadCampoComparativa(idCampoSecundario.Value);

                return OperationResult<ComparativaCamposDto>.SuccessResult(new ComparativaCamposDto
                {
                    CampoPrincipal = campoPrincipal,
                    CampoSecundario = campoSecundario
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comparativa entre campos");
                return OperationResult<ComparativaCamposDto>.Failure(
                    $"Error al generar comparativa: {ex.Message}", "DATABASE_ERROR");
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
                    .Include(c => c.Lotes)
                        .ThenInclude(l => l.CicloCultivos)
                            .ThenInclude(cc => cc.SiloBolsas)
                    .Include(c => c.RegistrosClima)
                    .FirstOrDefaultAsync(c => c.Id == idCampo && c.IdLicencia == _userContext.IdLicencia);

                if (campo == null)
                    return OperationResult<ReporteCampoIntegralDto>.Failure("Campo no encontrado", "NOT_FOUND");

                // Determinar Campaña activa
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
                var tiposActividad = await _unitOfWork.Repository<TipoActividad>().Query()
                    .ToListAsync();
                var tipoActividadDict = tiposActividad.ToDictionary(t => t.Nombre, t => (t.Icono, t.ColorIcono));
                reporte.Timeline = await BuildTimelineAsync(todosCiclos, activosCiclos, idCampaniaEfectiva, tipoActividadDict);

                // --- 2.3 Evolución del Cultivo ---
                reporte.EvolucionCultivo = BuildEvolucionCultivo(activosCiclos, todosCiclos);

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

            // Multi-crop: group by lot to get each lot's crop
            foreach (var lote in lotes)
            {
                var ciclosLote = activosCiclos.Where(cc => cc.IdLote == lote.Id).ToList();
                if (!ciclosLote.Any())
                {
                    // No active cycles in this lot
                    resumen.Cultivos.Add(new CultivoResumenDto
                    {
                        Lote = lote.Nombre,
                        Nombre = "Sin cultivo",
                        SuperficieHa = lote.SuperficieHectareas ?? 0,
                        CantidadCiclos = 0,
                        CantidadInactivos = 0,
                        IsActivo = false
                    });
                    continue;
                }

                // Group by cultivo
                var cultivosEnLote = ciclosLote
                    .GroupBy(cc => cc.Cultivo?.Nombre ?? "N/A")
                    .Select(g => new CultivoResumenDto
                    {
                        Lote = lote.Nombre,
                        Nombre = g.Key,
                        Variedad = g.First().Variedad?.Nombre,
                        SuperficieHa = lote.SuperficieHectareas ?? 0,
                        CantidadCiclos = g.Count(),
                        CantidadInactivos = g.Count(cc => cc.FechaFin != null),
                        IsActivo = g.Any(cc => cc.FechaFin == null)
                    });

                foreach (var c in cultivosEnLote)
                    resumen.Cultivos.Add(c);
            }

            // Campaña actual (first ciclo's campaign)
            resumen.Campania = activosCiclos
                .FirstOrDefault()?.Campania?.Nombre;

            // Dias desde siembra (earliest siembra among all lots)
            var todasLasSiembras = activosCiclos
                .SelectMany(cc => cc.Siembras)
                .OrderByDescending(s => s.Fecha)
                .ToList();

            var ultimaSiembra = todasLasSiembras.FirstOrDefault();
            if (ultimaSiembra != null)
            {
                resumen.FechaSiembra = ultimaSiembra.Fecha;
                resumen.DiasDesdeSiembra = (int)(DateTime.UtcNow.Date - ultimaSiembra.Fecha.Date).TotalDays;
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

        private async Task<List<TimelineEventoDto>> BuildTimelineAsync(
            List<CicloCultivo> todosCiclos,
            List<CicloCultivo> activosCiclos,
            int? idCampaniaEfectiva,
            Dictionary<string, (string Icono, string Color)> tipoActividadDict)
        {
            var eventos = new List<TimelineEventoDto>();
            int eventId = 1;

            // Helper: Lookup icon/color from TipoActividad catalog or use defaults
            (string icono, string color) GetIconoYColor(string tipoActividad, string defaultIcono, string defaultColor)
            {
                if (tipoActividadDict.TryGetValue(tipoActividad, out var entry))
                    return (entry.Icono, entry.Color);
                return (defaultIcono, defaultColor);
            }

            void AddEventosFromCiclo(CicloCultivo ciclo, List<Siembra> siembras, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddCosechas(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddLabores(CicloCultivo ciclo, List<Fertilizacion> labores, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddPulverizaciones(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddMonitoreos(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddRiegos(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddOtrasLabores(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
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

            void AddSiloBolsas(CicloCultivo ciclo, string tipo, string defaultIcono, string defaultColor)
            {
                var (icono, color) = GetIconoYColor(tipo, defaultIcono, defaultColor);
                foreach (var sb in ciclo.SiloBolsas)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = sb.Fecha,
                        TipoActividad = tipo,
                        Icono = icono,
                        Color = color,
                        Descripcion = $"Código: {sb.Codigo}, Longitud: {sb.Longitud}m, Capacidad: {sb.CapacidadTotalTn}tn",
                        Lote = ciclo.Lote?.Nombre,
                        CicloCultivo = $"{ciclo.Cultivo?.Nombre} {ciclo.Campania?.Nombre}",
                    });
                }
            }

            // Procesar ciclos activos primero, luego histÃ³ricos
            var ciclosOrdenados = activosCiclos
                .OrderByDescending(cc => cc.FechaInicio)
                .ToList();

            foreach (var ciclo in ciclosOrdenados)
            {
                AddEventosFromCiclo(ciclo, ciclo.Siembras, "Siembra", "ph-plant", "#28a745");
                AddLabores(ciclo, ciclo.Fertilizaciones, "Fertilización", "ph-flask", "#17a2b8");
                AddPulverizaciones(ciclo, "Pulverización", "ph-airplane-tilt", "#6f42c1");
                AddRiegos(ciclo, "Riego", "ph-drop", "#007bff");
                AddMonitoreos(ciclo, "Monitoreo", "ph-eye", "#ffc107");
                AddCosechas(ciclo, "Cosecha", "ph-trend-up", "#fd7e14");
                AddOtrasLabores(ciclo, "Otra Labor", "ph-wrench", "#6c757d");
                AddSiloBolsas(ciclo, "Silo Bolsa", "ph-package", "#8B5E3C");

                // Agregar análisis de suelo
                var (analisisIcono, analisisColor) = GetIconoYColor("Análisis de Suelo", "ph-magnifying-glass", "#20c997");
                foreach (var a in ciclo.AnalisisSuelos)
                {
                    eventos.Add(new TimelineEventoDto
                    {
                        Id = eventId++,
                        Fecha = a.Fecha,
                        TipoActividad = "Análisis de Suelo",
                        Icono = analisisIcono,
                        Color = analisisColor,
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
            List<CicloCultivo> todosCiclos)
        {
            var evolucion = new EvolucionCultivoDto();

            // Build evolution data from activity dates (NDVI simulation based on days since planting)
            var siembras = activosCiclos
                .SelectMany(cc => cc.Siembras)
                .OrderBy(s => s.Fecha)
                .ToList();

            var cosechas = activosCiclos
                .SelectMany(cc => cc.Cosechas)
                .OrderBy(c => c.Fecha)
                .ToList();

            if (siembras.Any())
            {
                var primeraSiembra = siembras.First().Fecha;
                var ultimaFecha = cosechas.Any() ? cosechas.Last().Fecha : DateTime.UtcNow;
                var totalDias = (int)(ultimaFecha - primeraSiembra).TotalDays;

                if (totalDias > 0)
                {
                    // Generate evolution points (up to 20 data points)
                    var step = Math.Max(1, totalDias / 20);
                    for (int i = 0; i <= totalDias; i += step)
                    {
                        var fecha = primeraSiembra.AddDays(i);
                        var diasTranscurridos = i;

                        // Simulated NDVI curve based on crop growth stages
                        decimal? ndvi = null;
                        if (diasTranscurridos < 30) // Initial growth
                            ndvi = Math.Round(0.2m + (diasTranscurridos / 30m) * 0.4m, 2);
                        else if (diasTranscurridos < 90) // Vegetative peak
                            ndvi = Math.Round(0.6m + ((diasTranscurridos - 30) / 60m) * 0.3m, 2);
                        else if (diasTranscurridos < 150) // Senescence
                            ndvi = Math.Round(0.9m - ((diasTranscurridos - 90) / 60m) * 0.5m, 2);
                        else // Post-maturity
                            ndvi = Math.Round(0.4m, 2);

                        evolucion.Evolucion.Add(new DatoEvolucion
                        {
                            Fecha = fecha,
                            NDVI = ndvi,
                            Temperatura = null,
                            Precipitacion = null,
                            Humedad = null
                        });
                    }
                }
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
                    NDVIPromedioActual = null,
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

            // días sin lluvia
            var ultimaLluvia = lluvias
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefault();
            analisis.DiasSinLluvia = ultimaLluvia != null
                ? (int)(DateTime.UtcNow.Date - ultimaLluvia.Fecha.Date).TotalDays
                : 999;

            // Heladas (granizo)
            analisis.CantidadHeladas = registrosClima.Count(r => r.TipoClima == TipoClima.Granizo);

            // Balance hÃ­drico
            if (analisis.LluviaAcumulada < 20)
            {
                analisis.BalanceHidrico = "DÃ©ficit hÃ­drico";
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
                analisis.BalanceHidrico = "Exceso hÃ­drico";
                analisis.EstresHidrico = "Alto";
            }

            // Registros climÃ¡ticos detallados
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

            // Obtener el Ãºltimo anÃ¡lisis de suelo
            var ultimoAnalisis = activosCiclos
                .SelectMany(cc => cc.AnalisisSuelos)
                .OrderByDescending(a => a.Fecha)
                .FirstOrDefault();

            if (ultimoAnalisis == null)
                return analisis;

            analisis.FechaAnalisis = ultimoAnalisis.Fecha;
            analisis.ProfundidadCm = ultimoAnalisis.ProfundidadCm;
            analisis.Textura = ultimoAnalisis.Textura;

            // Interpretar cada parÃ¡metro - solo advertir, no recomendar aplicar
            analisis.PH = InterpretarParametro("pH", ultimoAnalisis.PH, "",
                v => v < 5.5m ? ("Ãcido", "Bajo", "pH bajo - monitorear condiciones del suelo")
                    : v <= 7.0m ? ("Ã“ptimo", "Ã“ptimo", "pH en rango Ã³ptimo")
                    : ("Alcalino", "Alto", "pH elevado - monitorear disponibilidad de nutrientes"));

            analisis.MateriaOrganica = InterpretarParametro("MO", ultimoAnalisis.MateriaOrganica, "%",
                v => v < 2m ? ("Bajo", "Bajo", "Materia orgÃ¡nica baja - monitorear fertilidad del suelo")
                    : v <= 5m ? ("Medio", "Medio", "Nivel adecuado de materia orgÃ¡nica")
                    : ("Alto", "Alto", "Nivel Ã³ptimo de materia orgÃ¡nica"));

            analisis.Nitrogeno = InterpretarParametro("N", ultimoAnalisis.Nitrogeno, "ppm",
                v => v < 15m ? ("Bajo", "Bajo", "NitrÃ³geno bajo - monitorear desarrollo del cultivo")
                    : v <= 30m ? ("Ã“ptimo", "Ã“ptimo", "Nivel de nitrÃ³geno adecuado")
                    : ("Alto", "Alto", "Exceso de nitrÃ³geno - monitorear posible impacto ambiental"));

            analisis.Fosforo = InterpretarParametro("P", ultimoAnalisis.Fosforo, "ppm",
                v => v < 10m ? ("Bajo", "Bajo", "FÃ³sforo bajo - monitorear disponibilidad para el cultivo")
                    : v <= 20m ? ("Ã“ptimo", "Ã“ptimo", "Nivel de fÃ³sforo adecuado")
                    : ("Alto", "Alto", "Nivel elevado de fÃ³sforo - monitorear"));

            analisis.Potasio = InterpretarParametro("K", ultimoAnalisis.Potasio, "meq/100g",
                v => v < 0.3m ? ("Bajo", "Bajo", "Potasio bajo - monitorear")
                    : v <= 0.7m ? ("Ã“ptimo", "Ã“ptimo", "Nivel de potasio adecuado")
                    : ("Alto", "Alto", "Nivel elevado de potasio - verificar balance con otros nutrientes"));

            analisis.ConductividadElectrica = InterpretarParametro("CE", ultimoAnalisis.ConductividadElectrica, "dS/m",
                v => v < 0.5m ? ("Muy baja", "Bajo", "Salinidad muy baja - monitorear disponibilidad de nutrientes")
                    : v <= 1.5m ? ("Normal", "Ã“ptimo", "Salinidad adecuada")
                    : ("Alta", "Alto", "Salinidad elevada - monitorear impacto en el cultivo"));

            analisis.CIC = InterpretarParametro("CIC", ultimoAnalisis.CIC, "meq/100g",
                v => v < 10m ? ("Baja", "Bajo", "CIC baja - monitorear retenciÃ³n de nutrientes")
                    : v <= 25m ? ("Media", "Medio", "CIC adecuada")
                    : ("Alta", "Alto", "CIC alta - buena retenciÃ³n de nutrientes"));

            // Generar advertencias generales (solo para parÃ¡metros bajos)
            var todosParametros = new[] {
                (analisis.PH, "pH"),
                (analisis.MateriaOrganica, "Materia OrgÃ¡nica"),
                (analisis.Nitrogeno, "NitrÃ³geno"),
                (analisis.Fosforo, "FÃ³sforo"),
                (analisis.Potasio, "Potasio"),
                (analisis.ConductividadElectrica, "Conductividad ElÃ©ctrica"),
                (analisis.CIC, "CIC")
            };

            foreach (var (param, nombre) in todosParametros)
            {
                if (param != null && param.Nivel == "Bajo" && !string.IsNullOrEmpty(param.Advertencia))
                    analisis.Recomendaciones.Add($"{nombre}: {param.Advertencia}");
            }

            return analisis;
        }

        private ParametroSueloDto InterpretarParametro(
            string nombre,
            decimal? valor,
            string unidad,
            Func<decimal, (string interpretacion, string nivel, string advertencia)> interpretar)
        {
            if (!valor.HasValue)
                return new ParametroSueloDto { Unidad = unidad };

            var (interpretacion, nivel, advertencia) = interpretar(valor.Value);

            return new ParametroSueloDto
            {
                Valor = valor,
                Unidad = unidad,
                Interpretacion = interpretacion,
                Nivel = nivel,
                Advertencia = advertencia
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
                foreach (var sb in ciclo.SiloBolsas)
                {
                    desglose.SiloBolsasARS += sb.CostoARS.GetValueOrDefault();
                    desglose.SiloBolsasUSD += sb.CostoUSD.GetValueOrDefault();
                }
            }

            totalARS = desglose.SiembraARS + desglose.FertilizacionARS + desglose.PulverizacionARS
                     + desglose.RiegoARS + desglose.CosechaARS + desglose.MonitoreoARS
                     + desglose.AnalisisSueloARS + desglose.OtrasLaboresARS + desglose.SiloBolsasARS;

            totalUSD = desglose.SiembraUSD + desglose.FertilizacionUSD + desglose.PulverizacionUSD
                     + desglose.RiegoUSD + desglose.CosechaUSD + desglose.MonitoreoUSD
                     + desglose.AnalisisSueloUSD + desglose.OtrasLaboresUSD + desglose.SiloBolsasUSD;

            // Agregar gastos generales si coinciden con la Campaña
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
                // Precio estimado de referencia (esto deberÃ­a venir de una config)
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

            // Ãšltima cosecha del ciclo activo
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

                // producción total
                var supCosechada = ultimaCosecha.SuperficieCosechadaHa ?? 0;
                if (ultimaCosecha.RendimientoTonHa.HasValue && supCosechada > 0)
                    rendimiento.ProduccionTotalTon = Math.Round(ultimaCosecha.RendimientoTonHa.Value * supCosechada, 2);
                else if (ultimaCosecha.RendimientoTonHa.HasValue)
                    rendimiento.ProduccionTotalTon = ultimaCosecha.RendimientoTonHa;
            }

            // HistÃ³rico de rendimientos
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
                    Recomendacion = "Evaluar necesidad de riego complementario. Monitorear estrÃ©s hÃ­drico en el cultivo.",
                    Icono = "ph-warning-circle"
                });
            }

            // 2. Alerta de exceso hÃ­drico
            if (clima.LluviaAcumulada.HasValue && clima.LluviaAcumulada.Value > 100)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Exceso hÃ­drico",
                    Severidad = "Alta",
                    Mensaje = $"Lluvia acumulada de {clima.LluviaAcumulada.Value}mm en los últimos 30 días",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Verificar drenaje del lote. Monitorear apariciÃ³n de enfermedades fÃºngicas.",
                    Icono = "ph-warning"
                });
            }

            // 3. Alerta de caída de NDVI
            if (resumen.NDVIPromedio.HasValue && resumen.NDVIPromedio.Value < 0.3m)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "NDVI crÃ­tico",
                    Severidad = "Alta",
                    Mensaje = $"NDVI estimado en {resumen.NDVIPromedio.Value}, indica baja vitalidad del cultivo",
                    Fecha = DateTime.UtcNow,
                    Recomendacion = "Evaluar causa: dÃ©ficit hÃ­drico, plaga, enfermedad o deficiencia nutricional.",
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
                    Recomendacion = "Evaluar daÃ±os en el cultivo. Considerar seguros agrÃ­colas para prÃ³xima Campaña.",
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
                        Mensaje = $"Rendimiento actual ({rendimiento.RendimientoTonHa} tn/ha) está por debajo del 70% del histÃ³rico ({promedioHistorico:N2} tn/ha)",
                        Fecha = DateTime.UtcNow,
                        Recomendacion = "Analizar causas del bajo rendimiento: calidad de semilla, manejo, clima o suelo.",
                        Icono = "ph-trend-down"
                    });
                }
            }

            // 6. Alerta de parÃ¡metros de suelo
            if (suelo.PH?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Suelo Ã¡cido",
                    Severidad = "Media",
                    Mensaje = $"pH del suelo en {suelo.PH.Valor:N1} - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.PH.Advertencia,
                    Icono = "ph-flask"
                });
            }

            if (suelo.Nitrogeno?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Deficiencia de NitrÃ³geno",
                    Severidad = "Media",
                    Mensaje = $"Nivel de NitrÃ³geno: {suelo.Nitrogeno.Valor:N0} ppm - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.Nitrogeno.Advertencia,
                    Icono = "ph-flask"
                });
            }

            if (suelo.Fosforo?.Nivel == "Bajo")
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Deficiencia de FÃ³sforo",
                    Severidad = "Media",
                    Mensaje = $"Nivel de FÃ³sforo: {suelo.Fosforo.Valor:N0} ppm - Nivel bajo",
                    Fecha = suelo.FechaAnalisis,
                    Recomendacion = suelo.Fosforo.Advertencia,
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
                        + cc.AnalisisSuelos.Count + cc.OtrasLabores.Count + cc.SiloBolsas.Count),
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
                    foreach (var sb in ciclo.SiloBolsas)
                    {
                        costosARS += sb.CostoARS.GetValueOrDefault();
                        costosUSD += sb.CostoUSD.GetValueOrDefault();
                    }
                }
                entry.CostoTotalARS = costosARS;
                entry.CostoTotalUSD = costosUSD;

                historial.Add(entry);
            }

            return historial;
        }

        // ============================================================
        // MÃ‰TODOS EXISTENTES DE CÃLCULO DE COSTOS
        // ============================================================

        private decimal ObtenerCostosARS(Lote lote, List<CicloCultivo>? ciclos = null)
        {
            decimal total = 0;

            var ciclosToUse = ciclos ?? lote.CicloCultivos;

            foreach (var ciclo in ciclosToUse)
            {
                foreach (var s in ciclo.Siembras) total += s.CostoARS.GetValueOrDefault();
                foreach (var c in ciclo.Cosechas) total += c.CostoARS.GetValueOrDefault();
                foreach (var f in ciclo.Fertilizaciones) total += f.CostoARS.GetValueOrDefault();
                foreach (var p in ciclo.Pulverizaciones) total += p.CostoARS.GetValueOrDefault();
                foreach (var r in ciclo.Riegos) total += r.CostoARS.GetValueOrDefault();
                foreach (var m in ciclo.Monitoreos) total += m.CostoARS.GetValueOrDefault();
                foreach (var a in ciclo.AnalisisSuelos) total += a.CostoARS.GetValueOrDefault();
                foreach (var o in ciclo.OtrasLabores) total += o.CostoARS.GetValueOrDefault();
                foreach (var sb in ciclo.SiloBolsas) total += sb.CostoARS.GetValueOrDefault();
            }

            return total;
        }

        private decimal ObtenerCostosUSD(Lote lote, List<CicloCultivo>? ciclos = null)
        {
            decimal total = 0;

            var ciclosToUse = ciclos ?? lote.CicloCultivos;

            foreach (var ciclo in ciclosToUse)
            {
                foreach (var s in ciclo.Siembras) total += s.CostoUSD.GetValueOrDefault();
                foreach (var c in ciclo.Cosechas) total += c.CostoUSD.GetValueOrDefault();
                foreach (var f in ciclo.Fertilizaciones) total += f.CostoUSD.GetValueOrDefault();
                foreach (var p in ciclo.Pulverizaciones) total += p.CostoUSD.GetValueOrDefault();
                foreach (var r in ciclo.Riegos) total += r.CostoUSD.GetValueOrDefault();
                foreach (var m in ciclo.Monitoreos) total += m.CostoUSD.GetValueOrDefault();
                foreach (var a in ciclo.AnalisisSuelos) total += a.CostoUSD.GetValueOrDefault();
                foreach (var o in ciclo.OtrasLabores) total += o.CostoUSD.GetValueOrDefault();
                foreach (var sb in ciclo.SiloBolsas) total += sb.CostoUSD.GetValueOrDefault();
            }

            return total;
        }

        // ============================================================
        // NUEVO: Reporte de Rendimiento de Cosecha
        // ============================================================

        public async Task<OperationResult<RendimientoCosechaReporteDto>> GetRendimientoCosechaAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string ordenarPor = "RendimientoTonHa",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamanoPagina = 20)
        {
            try
            {
                // 1. Query base de Cosecha con includes
                var query = _unitOfWork.Repository<Cosecha>().Query()
                    .Include(c => c.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(c => c.CicloCultivo)
                        .ThenInclude(cc => cc.Cultivo)
                    .Include(c => c.CicloCultivo)
                        .ThenInclude(cc => cc.Campania)
                    .Include(c => c.Moneda)
                    .Where(c => c.IdLicencia == _userContext.IdLicencia)
                    .AsQueryable();

                // 2. Aplicar filtros
                if (idCampania.HasValue)
                    query = query.Where(c => c.IdCampania == idCampania.Value);
                else if (_userContext.IdCampaña.HasValue)
                    query = query.Where(c => c.IdCampania == _userContext.IdCampaña.Value);

                if (idCampo.HasValue)
                    query = query.Where(c => c.Lote != null && c.Lote.IdCampo == idCampo.Value);

                if (idLote.HasValue)
                    query = query.Where(c => c.IdLote == idLote.Value);

                if (idCultivo.HasValue)
                    query = query.Where(c => c.IdCultivo == idCultivo.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(c => c.Fecha >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(c => c.Fecha <= fechaHasta.Value);

                // 3. Obtener datos para cálculos agregados (sin paginaciÃ³n)
                var todasLasCosechas = await query.ToListAsync();

                if (todasLasCosechas.Count == 0)
                {
                    return OperationResult<RendimientoCosechaReporteDto>.SuccessResult(
                        new RendimientoCosechaReporteDto());
                }

                // 4. Calcular KPIs
                var kpis = new RendimientoCosechaKpiDto();

                // Rendimiento promedio
                var conRendimiento = todasLasCosechas.Where(c => c.RendimientoTonHa.HasValue).ToList();
                kpis.TotalLotes = todasLasCosechas.Select(c => c.IdLote).Distinct().Count();
                kpis.LotesConRendimiento = conRendimiento.Select(c => c.IdLote).Distinct().Count();

                if (conRendimiento.Any())
                {
                    kpis.RendimientoPromedioTonHa = Math.Round(conRendimiento.Average(c => c.RendimientoTonHa!.Value), 2);

                    // Mejor rendimiento
                    var mejor = conRendimiento.OrderByDescending(c => c.RendimientoTonHa).First();
                    kpis.RendimientoMaximoTonHa = mejor.RendimientoTonHa;
                    kpis.LoteMejorRendimiento = mejor.Lote?.Nombre;

                    // Peor rendimiento
                    var peor = conRendimiento.OrderBy(c => c.RendimientoTonHa).First();
                    kpis.RendimientoMinimoTonHa = peor.RendimientoTonHa;
                    kpis.LotePeorRendimiento = peor.Lote?.Nombre;
                }

                // producción total
                kpis.ProduccionTotalTon = Math.Round(
                    conRendimiento.Sum(c => (c.RendimientoTonHa ?? 0) * (c.SuperficieCosechadaHa ?? 0)), 2);

                // Superficie total cosechada
                kpis.SuperficieTotalCosechadaHa = Math.Round(
                    todasLasCosechas.Sum(c => c.SuperficieCosechadaHa ?? 0), 2);

                // Humedad promedio
                var conHumedad = todasLasCosechas.Where(c => c.HumedadGrano.HasValue).ToList();
                if (conHumedad.Any())
                    kpis.HumedadPromedio = Math.Round(conHumedad.Average(c => c.HumedadGrano!.Value), 1);

                // Costo promedio por hectÃ¡rea
                var conCosto = todasLasCosechas.Where(c => c.CostoARS.HasValue && c.SuperficieCosechadaHa.HasValue && c.SuperficieCosechadaHa > 0).ToList();
                if (conCosto.Any())
                    kpis.CostoPromedioPorHa = Math.Round(
                        conCosto.Sum(c => c.CostoARS!.Value) / conCosto.Sum(c => c.SuperficieCosechadaHa!.Value), 2);

                // VariaciÃ³n vs Campaña anterior
                var campañas = todasLasCosechas
                    .Where(c => c.CicloCultivo?.Campania != null && c.RendimientoTonHa.HasValue)
                    .GroupBy(c => c.CicloCultivo.Campania!.Nombre ?? "N/A")
                    .Select(g => new { Campania = g.Key, Promedio = g.Average(c => c.RendimientoTonHa!.Value) })
                    .OrderByDescending(g => g.Campania)
                    .ToList();

                if (campañas.Count >= 2)
                {
                    kpis.CampaniaActual = campañas[0].Campania;
                    kpis.CampaniaAnterior = campañas[1].Campania;
                    if (campañas[1].Promedio > 0)
                    {
                        kpis.VariacionVsCampaniaAnterior = Math.Round(
                            ((campañas[0].Promedio - campañas[1].Promedio) / campañas[1].Promedio) * 100, 1);
                    }
                }
                else if (campañas.Count == 1)
                {
                    kpis.CampaniaActual = campañas[0].Campania;
                }

                // Moneda por defecto
                kpis.Moneda = "ARS";

                // 5. Construir datos de lotes para la tabla (con paginaciÃ³n y ordenamiento)
                var datosLotesQuery = todasLasCosechas.Select(c => new DatoRendimientoLoteDto
                {
                    IdCosecha = c.Id,
                    IdLote = c.IdLote,
                    Lote = c.Lote?.Nombre ?? "N/A",
                    Campo = c.Lote?.Campo?.Nombre ?? "N/A",
                    Cultivo = c.CicloCultivo?.Cultivo?.Nombre,
                    Campania = c.CicloCultivo?.Campania?.Nombre,
                    RendimientoTonHa = c.RendimientoTonHa,
                    ProduccionTotalTon = c.RendimientoTonHa.HasValue && c.SuperficieCosechadaHa.HasValue
                        ? Math.Round(c.RendimientoTonHa.Value * c.SuperficieCosechadaHa.Value, 2)
                        : null,
                    SuperficieCosechadaHa = c.SuperficieCosechadaHa,
                    HumedadGrano = c.HumedadGrano,
                    FechaCosecha = c.Fecha,
                    Costo = c.Costo,
                    CostoARS = c.CostoARS,
                    CostoUSD = c.CostoUSD,
                    Moneda = c.Moneda?.Nombre
                }).AsQueryable();

                // Ordenamiento
                datosLotesQuery = ordenDireccion.ToLower() == "asc"
                    ? datosLotesQuery.OrderBy(d => OrdenarPorPropiedad(d, ordenarPor))
                    : datosLotesQuery.OrderByDescending(d => OrdenarPorPropiedad(d, ordenarPor));

                var datosLotesList = datosLotesQuery.ToList();

                // Asignar ranking
                for (int i = 0; i < datosLotesList.Count; i++)
                {
                    datosLotesList[i].RankingRendimiento = i + 1;
                }

                // PaginaciÃ³n
                var totalRegistros = datosLotesList.Count;
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
                pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, totalPaginas)));

                var datosLotesPaginados = datosLotesList
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToList();

                // Asignar tendencia (comparaciÃ³n con promedio general)
                var promedioGeneral = kpis.RendimientoPromedioTonHa ?? 0;
                foreach (var d in datosLotesPaginados)
                {
                    if (d.RendimientoTonHa.HasValue && promedioGeneral > 0)
                    {
                        var diff = (d.RendimientoTonHa.Value - promedioGeneral) / promedioGeneral;
                        d.Tendencia = diff > 0.1m ? "subiendo" : diff < -0.1m ? "bajando" : "estable";
                    }
                    else
                    {
                        d.Tendencia = "estable";
                    }
                }

                // 6. Ranking de lotes (top 10)
                var rankingLotes = datosLotesList
                    .Where(d => d.RendimientoTonHa.HasValue)
                    .OrderByDescending(d => d.RendimientoTonHa)
                    .Take(10)
                    .Select((d, idx) => new RankingLoteDto
                    {
                        Posicion = idx + 1,
                        Lote = d.Lote,
                        Campo = d.Campo,
                        Cultivo = d.Cultivo,
                        RendimientoTonHa = d.RendimientoTonHa!.Value,
                        Campania = d.Campania
                    })
                    .ToList();

                // 7. Rendimiento por cultivo (para gráfico de torta)
                var rendimientoPorCultivo = todasLasCosechas
                    .Where(c => c.CicloCultivo?.Cultivo?.Nombre != null && c.RendimientoTonHa.HasValue && c.SuperficieCosechadaHa.HasValue)
                    .GroupBy(c => c.CicloCultivo.Cultivo!.Nombre!)
                    .Select(g => new DatoRendimientoPorCultivo
                    {
                        Cultivo = g.Key,
                        RendimientoPromedioTonHa = Math.Round(g.Average(c => c.RendimientoTonHa!.Value), 2),
                        SuperficieTotalHa = Math.Round(g.Sum(c => c.SuperficieCosechadaHa ?? 0), 2),
                        ProduccionTotalTon = Math.Round(g.Sum(c => (c.RendimientoTonHa ?? 0) * (c.SuperficieCosechadaHa ?? 0)), 2),
                        CantidadLotes = g.Select(c => c.IdLote).Distinct().Count(),
                        Color = ObtenerColorCultivo(g.Key)
                    })
                    .OrderByDescending(d => d.RendimientoPromedioTonHa)
                    .ToList();

                // 8. Rendimiento por Campaña (para gráfico de barras)
                var rendimientoPorCampania = todasLasCosechas
                    .Where(c => c.CicloCultivo?.Campania?.Nombre != null && c.RendimientoTonHa.HasValue)
                    .GroupBy(c => c.CicloCultivo.Campania!.Nombre!)
                    .Select(g => new DatoRendimientoPorCampania
                    {
                        Campania = g.Key,
                        RendimientoPromedioTonHa = Math.Round(g.Average(c => c.RendimientoTonHa!.Value), 2),
                        SuperficieTotalHa = Math.Round(g.Sum(c => c.SuperficieCosechadaHa ?? 0), 2),
                        ProduccionTotalTon = Math.Round(g.Sum(c => (c.RendimientoTonHa ?? 0) * (c.SuperficieCosechadaHa ?? 0)), 2),
                        CantidadCosechas = g.Count()
                    })
                    .OrderBy(d => d.Campania)
                    .ToList();

                // 9. Rendimiento por campo (para gráfico de barras horizontal)
                var rendimientoPorCampo = todasLasCosechas
                    .Where(c => c.Lote?.Campo?.Nombre != null && c.RendimientoTonHa.HasValue)
                    .GroupBy(c => c.Lote!.Campo!.Nombre!)
                    .Select(g => new DatoRendimientoPorCampo
                    {
                        Campo = g.Key,
                        RendimientoPromedioTonHa = Math.Round(g.Average(c => c.RendimientoTonHa!.Value), 2),
                        SuperficieTotalHa = Math.Round(g.Sum(c => c.SuperficieCosechadaHa ?? 0), 2),
                        ProduccionTotalTon = Math.Round(g.Sum(c => (c.RendimientoTonHa ?? 0) * (c.SuperficieCosechadaHa ?? 0)), 2),
                        CantidadLotes = g.Select(c => c.IdLote).Distinct().Count()
                    })
                    .OrderByDescending(d => d.RendimientoPromedioTonHa)
                    .ToList();

                // 10. Evolución histórica (para gráfico de líneas: rendimiento + humedad)
                var evolucion = todasLasCosechas
                    .Where(c => c.RendimientoTonHa.HasValue && c.CicloCultivo?.Campania?.Nombre != null)
                    .GroupBy(c => new { Campania = c.CicloCultivo.Campania!.Nombre!, Cultivo = c.CicloCultivo.Cultivo?.Nombre ?? "N/A" })
                    .Select(g => new DatoEvolucionRendimiento
                    {
                        Periodo = $"{g.Key.Campania} - {g.Key.Cultivo}",
                        Campania = g.Key.Campania,
                        Cultivo = g.Key.Cultivo,
                        RendimientoTonHa = Math.Round(g.Average(c => c.RendimientoTonHa!.Value), 2),
                        HumedadPromedio = g.Where(c => c.HumedadGrano.HasValue).Any()
                            ? Math.Round(g.Where(c => c.HumedadGrano.HasValue).Average(c => c.HumedadGrano!.Value), 1)
                            : null,
                        SuperficieHa = Math.Round(g.Sum(c => c.SuperficieCosechadaHa ?? 0), 2)
                    })
                    .OrderBy(d => d.Campania)
                    .ThenBy(d => d.Cultivo)
                    .ToList();

                // 11. Indicadores inteligentes
                var indicadores = GenerarIndicadoresInteligentes(kpis, conRendimiento, promedioGeneral);

                // 12. Armar reporte final
                var reporte = new RendimientoCosechaReporteDto
                {
                    Kpis = kpis,
                    DatosLotes = datosLotesPaginados,
                    RankingLotes = rankingLotes,
                    RendimientoPorCultivo = rendimientoPorCultivo,
                    RendimientoPorCampania = rendimientoPorCampania,
                    RendimientoPorCampo = rendimientoPorCampo,
                    EvolucionRendimiento = evolucion,
                    Indicadores = indicadores,
                    Paginacion = new PaginacionDto
                    {
                        PaginaActual = pagina,
                        TamanoPagina = tamanoPagina,
                        TotalRegistros = totalRegistros,
                        TotalPaginas = totalPaginas
                    }
                };

                return OperationResult<RendimientoCosechaReporteDto>.SuccessResult(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de rendimiento de cosecha");
                return OperationResult<RendimientoCosechaReporteDto>.Failure(
                    $"Error al generar reporte de rendimiento: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Ordena por propiedad usando expresión switch
        /// </summary>
        private static object OrdenarPorPropiedad(DatoRendimientoLoteDto dto, string propiedad)
        {
            return propiedad switch
            {
                "Lote" => dto.Lote,
                "Campo" => dto.Campo,
                "Cultivo" => (object?)dto.Cultivo ?? "",
                "Campania" => (object?)dto.Campania ?? "",
                "RendimientoTonHa" => (object?)dto.RendimientoTonHa ?? 0m,
                "ProduccionTotalTon" => (object?)dto.ProduccionTotalTon ?? 0m,
                "SuperficieCosechadaHa" => (object?)dto.SuperficieCosechadaHa ?? 0m,
                "HumedadGrano" => (object?)dto.HumedadGrano ?? 0m,
                "FechaCosecha" => (object?)dto.FechaCosecha ?? DateTime.MinValue,
                "CostoARS" => (object?)dto.CostoARS ?? 0m,
                "CostoUSD" => (object?)dto.CostoUSD ?? 0m,
                _ => (object?)dto.RendimientoTonHa ?? 0m
            };
        }

        /// <summary>
        /// Genera indicadores inteligentes basados en los KPIs calculados
        /// </summary>
        private static List<IndicadorInteligenteDto> GenerarIndicadoresInteligentes(
            RendimientoCosechaKpiDto kpis,
            List<Cosecha> conRendimiento,
            decimal promedioGeneral)
        {
            var indicadores = new List<IndicadorInteligenteDto>();

            // 1. Rendimiento vs promedio general
            if (kpis.RendimientoPromedioTonHa.HasValue && promedioGeneral > 0)
            {
                var ratio = kpis.RendimientoPromedioTonHa.Value / promedioGeneral;
                if (ratio > 1.2m)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Rendimiento superior",
                        Severidad = "Alta",
                        Titulo = "Rendimiento Superior",
                        Mensaje = $"El rendimiento promedio ({kpis.RendimientoPromedioTonHa} tn/ha) supera en {((ratio - 1) * 100):N0}% el promedio general ({promedioGeneral:N2} tn/ha)",
                        Recomendacion = "Documentar prácticas de manejo exitosas para replicarlas en otros lotes.",
                        Icono = "ph-trend-up",
                        Color = "#28a745",
                        Valor = kpis.RendimientoPromedioTonHa,
                        Umbral = promedioGeneral
                    });
                }
                else if (ratio < 0.8m)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Rendimiento inferior",
                        Severidad = "Alta",
                        Titulo = "⚠️ Rendimiento Inferior",
                        Mensaje = $"El rendimiento promedio ({kpis.RendimientoPromedioTonHa} tn/ha) está por debajo del 80% del promedio general ({promedioGeneral:N2} tn/ha)",
                        Recomendacion = "Evaluar factores limitantes: calidad de semilla, fecha de siembra, manejo de nutrientes, control de plagas.",
                        Icono = "ph-trend-down",
                        Color = "#dc3545",
                        Valor = kpis.RendimientoPromedioTonHa,
                        Umbral = promedioGeneral
                    });
                }
            }

            // 2. Humedad fuera de rango
            if (kpis.HumedadPromedio.HasValue)
            {
                if (kpis.HumedadPromedio > 18)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Humedad elevada",
                        Severidad = "Media",
                        Titulo = "💧 Humedad elevada en Grano",
                        Mensaje = $"La humedad promedio del grano es {kpis.HumedadPromedio}%, superior al recomendado (<14%)",
                        Recomendacion = "Considerar secado artificial o ajustar momento de cosecha para evitar descuentos por humedad.",
                        Icono = "ph-drop",
                        Color = "#ffc107",
                        Valor = kpis.HumedadPromedio,
                        Umbral = 14
                    });
                }
            }

            // 3. Variación vs Campaña anterior
            if (kpis.VariacionVsCampaniaAnterior.HasValue)
            {
                if (kpis.VariacionVsCampaniaAnterior < -10)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "caída interanual",
                        Severidad = "Alta",
                        Titulo = "⚠️ caída Interanual Significativa",
                        Mensaje = $"El rendimiento cayó {Math.Abs(kpis.VariacionVsCampaniaAnterior.Value):N1}% respecto a {kpis.CampaniaAnterior}",
                        Recomendacion = "Analizar condiciones climáticas y de manejo que difirieron entre campañas.",
                        Icono = "ph-chart-line-down",
                        Color = "#dc3545",
                        Valor = kpis.VariacionVsCampaniaAnterior,
                        Umbral = -10
                    });
                }
                else if (kpis.VariacionVsCampaniaAnterior > 15)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Mejora interanual",
                        Severidad = "Baja",
                        Titulo = "📈 Mejora Interanual Significativa",
                        Mensaje = $"El rendimiento aumentó {kpis.VariacionVsCampaniaAnterior:N1}% respecto a {kpis.CampaniaAnterior}",
                        Recomendacion = "Identificar y mantener las prácticas que contribuyeron a esta mejora.",
                        Icono = "ph-chart-line-up",
                        Color = "#28a745",
                        Valor = kpis.VariacionVsCampaniaAnterior,
                        Umbral = 15
                    });
                }
            }

            // 4. Baja superficie cosechada
            if (kpis.SuperficieTotalCosechadaHa.HasValue && kpis.SuperficieTotalCosechadaHa < 10)
            {
                indicadores.Add(new IndicadorInteligenteDto
                {
                    Tipo = "Superficie reducida",
                    Severidad = "Baja",
                    Titulo = "🗺️ Superficie Cosechada Reducida",
                    Mensaje = $"La superficie total cosechada es de solo {kpis.SuperficieTotalCosechadaHa:N1} ha",
                    Recomendacion = "Verificar si hay lotes sin cosechar o si los datos están completos.",
                    Icono = "ph-map-pin",
                    Color = "#6c757d",
                    Valor = kpis.SuperficieTotalCosechadaHa,
                    Umbral = 10
                });
            }

            // 5. Brecha entre mejor y peor lote
            if (kpis.RendimientoMaximoTonHa.HasValue && kpis.RendimientoMinimoTonHa.HasValue
                && kpis.RendimientoMaximoTonHa > 0 && kpis.RendimientoMinimoTonHa > 0)
            {
                var brecha = ((kpis.RendimientoMaximoTonHa.Value - kpis.RendimientoMinimoTonHa.Value) / kpis.RendimientoMaximoTonHa.Value) * 100;
                if (brecha > 50)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Brecha alta entre lotes",
                        Severidad = "Media",
                        Titulo = "📊 Alta Brecha entre Lotes",
                        Mensaje = $"Diferencia del {brecha:N0}% entre el mejor lote ({kpis.LoteMejorRendimiento}: {kpis.RendimientoMaximoTonHa} tn/ha) y el peor ({kpis.LotePeorRendimiento}: {kpis.RendimientoMinimoTonHa} tn/ha)",
                        Recomendacion = "Investigar diferencias de suelo, manejo o historia entre estos lotes para homogenizar la producción.",
                        Icono = "ph-git-diff",
                        Color = "#ffc107",
                        Valor = Math.Round(brecha, 1),
                        Umbral = 50
                    });
                }
            }

            // 6. Baja cobertura de datos (pocos lotes con rendimiento registrado)
            if (kpis.TotalLotes > 0 && kpis.LotesConRendimiento < kpis.TotalLotes)
            {
                var pctCobertura = (decimal)kpis.LotesConRendimiento / kpis.TotalLotes * 100;
                if (pctCobertura < 60)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Baja cobertura de datos",
                        Severidad = "Media",
                        Titulo = "📊 Baja Cobertura de Datos",
                        Mensaje = $"Solo {kpis.LotesConRendimiento} de {kpis.TotalLotes} lotes ({pctCobertura:N0}%) tienen rendimiento registrado",
                        Recomendacion = "Completar los registros de cosecha para tener una visión más precisa del rendimiento global.",
                        Icono = "ph-database",
                        Color = "#ffc107",
                        Valor = Math.Round(pctCobertura, 1),
                        Umbral = 60
                    });
                }
            }

            return indicadores;
        }

        /// <summary>
        /// Asigna un color consistente para cada cultivo
        /// </summary>
        private static string ObtenerColorCultivo(string cultivo)
        {
            return cultivo.ToLower() switch
            {
                "soja" => "#4CAF50",
                "maíz" or "maiz" => "#FF9800",
                "trigo" => "#2196F3",
                "girasol" => "#FFC107",
                "sorgo" => "#9C27B0",
                "cebada" => "#00BCD4",
                "avena" => "#795548",
                "colza" => "#607D8B",
                "alpiste" => "#8BC34A",
                "centeno" => "#E91E63",
                _ => "#4CAF50"
            };
        }

        // ============================================================
        // NUEVO: Reporte de Aplicaciones (Pulverización + Fertilización)
        // ============================================================

        public async Task<OperationResult<AplicacionReporteDto>> GetAplicacionesAsync(
            int? idCampania = null,
            int? idCampo = null,
            int? idLote = null,
            int? idCultivo = null,
            int? idTipoAplicacion = null,
            int? idProducto = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string ordenarPor = "Fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamanoPagina = 20)
        {
            try
            {
                var idLicencia = _userContext.IdLicencia;

                // ============================================================
                // 1. Query Pulverizaciones
                // ============================================================
                var pulvQuery = _unitOfWork.Repository<Pulverizacion>().Query()
                    .Include(p => p.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(p => p.CicloCultivo)
                        .ThenInclude(cc => cc.Cultivo)
                    .Include(p => p.CicloCultivo)
                        .ThenInclude(cc => cc.Campania)
                    .Include(p => p.ProductoAgroquimico)
                    .Include(p => p.Usuario)
                    .Include(p => p.Moneda)
                    .Where(p => p.IdLicencia == idLicencia)
                    .AsQueryable();

                // ============================================================
                // 2. Query Fertilizaciones
                // ============================================================
                var fertQuery = _unitOfWork.Repository<Fertilizacion>().Query()
                    .Include(f => f.Lote)
                        .ThenInclude(l => l.Campo)
                    .Include(f => f.CicloCultivo)
                        .ThenInclude(cc => cc.Cultivo)
                    .Include(f => f.CicloCultivo)
                        .ThenInclude(cc => cc.Campania)
                    .Include(f => f.Nutriente)
                    .Include(f => f.TipoFertilizante)
                    .Include(f => f.Usuario)
                    .Include(f => f.Moneda)
                    .Where(f => f.IdLicencia == idLicencia)
                    .AsQueryable();

                // ============================================================
                // 3. Aplicar filtros comunes
                // ============================================================
                if (idCampania.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.IdCampania == idCampania.Value);
                    fertQuery = fertQuery.Where(f => f.IdCampania == idCampania.Value);
                }
                else if (_userContext.IdCampaña.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.IdCampania == _userContext.IdCampaña.Value);
                    fertQuery = fertQuery.Where(f => f.IdCampania == _userContext.IdCampaña.Value);
                }

                if (idCampo.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.Lote != null && p.Lote.IdCampo == idCampo.Value);
                    fertQuery = fertQuery.Where(f => f.Lote != null && f.Lote.IdCampo == idCampo.Value);
                }

                if (idLote.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.IdLote == idLote.Value);
                    fertQuery = fertQuery.Where(f => f.IdLote == idLote.Value);
                }

                if (idCultivo.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.CicloCultivo != null && p.CicloCultivo.IdCultivo == idCultivo.Value);
                    fertQuery = fertQuery.Where(f => f.CicloCultivo != null && f.CicloCultivo.IdCultivo == idCultivo.Value);
                }

                if (fechaDesde.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.Fecha >= fechaDesde.Value);
                    fertQuery = fertQuery.Where(f => f.Fecha >= fechaDesde.Value);
                }

                if (fechaHasta.HasValue)
                {
                    pulvQuery = pulvQuery.Where(p => p.Fecha <= fechaHasta.Value);
                    fertQuery = fertQuery.Where(f => f.Fecha <= fechaHasta.Value);
                }

                if (idTipoAplicacion.HasValue)
                {
                    if (idTipoAplicacion.Value == 3) // Solo pulverizaciones
                        fertQuery = fertQuery.Where(f => false);
                    else if (idTipoAplicacion.Value == 4) // Solo fertilizaciones
                        pulvQuery = pulvQuery.Where(p => false);
                }

                // ============================================================
                // 4. Ejecutar queries y unificar en memoria
                // ============================================================
                var pulverizaciones = await pulvQuery.ToListAsync();
                var fertilizaciones = await fertQuery.ToListAsync();

                var todasLasAplicaciones = new List<AplicacionLoteDto>();

                // Mapear pulverizaciones
                foreach (var p in pulverizaciones)
                {
                    var superficieHa = p.Lote?.SuperficieHectareas;
                    var costoPorHa = p.CostoARS.HasValue && superficieHa.HasValue && superficieHa > 0
                        ? (decimal?)Math.Round(p.CostoARS.Value / superficieHa.Value, 2)
                        : (p.CostoUSD.HasValue && superficieHa.HasValue && superficieHa > 0
                            ? (decimal?)Math.Round(p.CostoUSD.Value / superficieHa.Value, 2)
                            : null);

                    todasLasAplicaciones.Add(new AplicacionLoteDto
                    {
                        Id = p.Id,
                        IdTipoActividad = 3,
                        TipoActividad = "Pulverización",
                        TipoActividadIcono = "ph-drop-half",
                        TipoActividadColor = "#0d6efd",
                        Fecha = p.Fecha,
                        IdLote = p.IdLote,
                        Lote = p.Lote?.Nombre ?? "N/A",
                        IdCampo = p.Lote?.IdCampo ?? 0,
                        Campo = p.Lote?.Campo?.Nombre ?? "N/A",
                        Campania = p.CicloCultivo?.Campania?.Nombre,
                        Cultivo = p.CicloCultivo?.Cultivo?.Nombre,
                        ProductoAplicado = p.ProductoAgroquimico?.Nombre,
                        TipoProducto = "Agroquímico",
                        Dosis = p.Dosis,
                        UnidadDosis = "Lts/Ha",
                        CantidadTotal = p.VolumenLitrosHa.HasValue && superficieHa.HasValue
                            ? Math.Round(p.VolumenLitrosHa.Value * superficieHa.Value, 2)
                            : null,
                        UnidadCantidad = "Litros",
                        CostoARS = p.CostoARS,
                        CostoUSD = p.CostoUSD,
                        Costo = p.Costo,
                        Moneda = p.Moneda?.Nombre,
                        Responsable = p.Usuario?.Nombre ?? p.RegistrationUser,
                        Observacion = p.Observacion,
                        ObservacionCortada = p.Observacion?.Length > 70
                            ? p.Observacion[..70] + "..."
                            : p.Observacion,
                        SuperficieHa = superficieHa,
                        CostoPorHa = costoPorHa
                    });
                }

                // Mapear fertilizaciones
                foreach (var f in fertilizaciones)
                {
                    var superficieHa = f.Lote?.SuperficieHectareas;
                    var productoNombre = !string.IsNullOrEmpty(f.TipoFertilizante?.Nombre)
                        ? f.TipoFertilizante.Nombre
                        : f.Nutriente?.Nombre ?? "Fertilizante";
                    var costoPorHa = f.CostoARS.HasValue && superficieHa.HasValue && superficieHa > 0
                        ? (decimal?)Math.Round(f.CostoARS.Value / superficieHa.Value, 2)
                        : (f.CostoUSD.HasValue && superficieHa.HasValue && superficieHa > 0
                            ? (decimal?)Math.Round(f.CostoUSD.Value / superficieHa.Value, 2)
                            : null);

                    todasLasAplicaciones.Add(new AplicacionLoteDto
                    {
                        Id = f.Id,
                        IdTipoActividad = 4,
                        TipoActividad = "Fertilización",
                        TipoActividadIcono = "ph-flask",
                        TipoActividadColor = "#198754",
                        Fecha = f.Fecha,
                        IdLote = f.IdLote,
                        Lote = f.Lote?.Nombre ?? "N/A",
                        IdCampo = f.Lote?.IdCampo ?? 0,
                        Campo = f.Lote?.Campo?.Nombre ?? "N/A",
                        Campania = f.CicloCultivo?.Campania?.Nombre,
                        Cultivo = f.CicloCultivo?.Cultivo?.Nombre,
                        ProductoAplicado = productoNombre,
                        TipoProducto = "Fertilizante",
                        Dosis = f.DosisKgHa ?? f.CantidadKgHa,
                        UnidadDosis = "Kg/Ha",
                        CantidadTotal = f.CantidadKgHa.HasValue && superficieHa.HasValue
                            ? Math.Round(f.CantidadKgHa.Value * superficieHa.Value, 2)
                            : (f.DosisKgHa.HasValue && superficieHa.HasValue
                                ? Math.Round(f.DosisKgHa.Value * superficieHa.Value, 2)
                                : null),
                        UnidadCantidad = "Kg",
                        CostoARS = f.CostoARS,
                        CostoUSD = f.CostoUSD,
                        Costo = f.Costo,
                        Moneda = f.Moneda?.Nombre,
                        Responsable = f.Usuario?.Nombre ?? f.RegistrationUser,
                        Observacion = f.Observacion,
                        ObservacionCortada = f.Observacion?.Length > 70
                            ? f.Observacion[..70] + "..."
                            : f.Observacion,
                        SuperficieHa = superficieHa,
                        CostoPorHa = costoPorHa
                    });
                }

                // Filtrar por producto si se especificó
                if (idProducto.HasValue)
                {
                    todasLasAplicaciones = todasLasAplicaciones
                        .Where(a => (a.IdTipoActividad == 3 && pulverizaciones.Any(p => p.Id == a.Id && p.IdProductoAgroquimico == idProducto))
                                 || (a.IdTipoActividad == 4 && fertilizaciones.Any(f => f.Id == a.Id && (f.IdNutriente == idProducto || f.IdTipoFertilizante == idProducto))))
                        .ToList();
                }

                if (todasLasAplicaciones.Count == 0)
                {
                    return OperationResult<AplicacionReporteDto>.SuccessResult(new AplicacionReporteDto());
                }

                // ============================================================
                // 5. Calcular KPIs
                // ============================================================
                var kpis = new AplicacionKpiDto();

                kpis.TotalAplicaciones = todasLasAplicaciones.Count;
                kpis.TotalPulverizaciones = todasLasAplicaciones.Count(a => a.IdTipoActividad == 3);
                kpis.TotalFertilizaciones = todasLasAplicaciones.Count(a => a.IdTipoActividad == 4);
                kpis.TotalLitrosAplicados = Math.Round(
                    todasLasAplicaciones.Where(a => a.IdTipoActividad == 3 && a.CantidadTotal.HasValue)
                        .Sum(a => a.CantidadTotal!.Value), 2);
                kpis.TotalKgAplicados = Math.Round(
                    todasLasAplicaciones.Where(a => a.IdTipoActividad == 4 && a.CantidadTotal.HasValue)
                        .Sum(a => a.CantidadTotal!.Value), 2);
                kpis.CostoTotalARS = Math.Round(
                    todasLasAplicaciones.Where(a => a.CostoARS.HasValue).Sum(a => a.CostoARS!.Value), 2);
                kpis.CostoTotalUSD = Math.Round(
                    todasLasAplicaciones.Where(a => a.CostoUSD.HasValue).Sum(a => a.CostoUSD!.Value), 2);

                // Costo por hectárea promedio
                var superficieTotal = todasLasAplicaciones
                    .Where(a => a.SuperficieHa.HasValue)
                    .Select(a => a.IdLote)
                    .Distinct()
                    .Select(idL => todasLasAplicaciones.First(a => a.IdLote == idL).SuperficieHa)
                    .Sum() ?? 0;

                if (superficieTotal > 0)
                {
                    kpis.CostoPromedioPorHaARS = kpis.CostoTotalARS.HasValue
                        ? Math.Round(kpis.CostoTotalARS.Value / superficieTotal, 2) : null;
                    kpis.CostoPromedioPorHaUSD = kpis.CostoTotalUSD.HasValue
                        ? Math.Round(kpis.CostoTotalUSD.Value / superficieTotal, 2) : null;
                }

                // Producto más aplicado (pulverizaciones)
                var prodGroup = todasLasAplicaciones
                    .Where(a => a.IdTipoActividad == 3 && !string.IsNullOrEmpty(a.ProductoAplicado))
                    .GroupBy(a => a.ProductoAplicado ?? "")
                    .Select(g => new { Producto = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(g => g.Cantidad)
                    .FirstOrDefault();
                if (prodGroup != null)
                {
                    kpis.ProductoMasAplicado = prodGroup.Producto;
                    kpis.ProductoMasAplicadoCantidad = prodGroup.Cantidad;
                }

                // Nutriente más aplicado (fertilizaciones)
                var nutriGroup = todasLasAplicaciones
                    .Where(a => a.IdTipoActividad == 4 && !string.IsNullOrEmpty(a.ProductoAplicado))
                    .GroupBy(a => a.ProductoAplicado ?? "")
                    .Select(g => new { Producto = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(g => g.Cantidad)
                    .FirstOrDefault();
                if (nutriGroup != null)
                {
                    kpis.NutrienteMasAplicado = nutriGroup.Producto;
                    kpis.NutrienteMasAplicadoCantidad = nutriGroup.Cantidad;
                }

                kpis.TotalLotes = todasLasAplicaciones.Select(a => a.IdLote).Distinct().Count();
                kpis.PromedioAplicacionesPorLote = kpis.TotalLotes > 0
                    ? Math.Round((decimal)kpis.TotalAplicaciones / kpis.TotalLotes, 1) : 0;
                kpis.SuperficieTotalTratadaHa = Math.Round(superficieTotal, 2);
                kpis.Moneda = "ARS";

                // ============================================================
                // 6. Ordenar y paginar tabla principal
                // ============================================================
                var queryOrdenada = ordenDireccion.ToLower() == "asc"
                    ? todasLasAplicaciones.AsQueryable().OrderBy(d => OrdenarAplicacionPorPropiedad(d, ordenarPor))
                    : todasLasAplicaciones.AsQueryable().OrderByDescending(d => OrdenarAplicacionPorPropiedad(d, ordenarPor));

                var listaOrdenada = queryOrdenada.ToList();

                var totalRegistros = listaOrdenada.Count;
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
                pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, totalPaginas)));

                var datosPaginados = listaOrdenada
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToList();

                // ============================================================
                // 7. Timeline de aplicaciones (historial combinado ordenado por fecha)
                // ============================================================
                var timeline = todasLasAplicaciones
                    .OrderByDescending(a => a.Fecha)
                    .Take(50)
                    .Select(a => new AplicacionTimelineDto
                    {
                        Id = a.Id,
                        IdTipoActividad = a.IdTipoActividad,
                        TipoActividad = a.TipoActividad,
                        Icono = a.TipoActividadIcono,
                        Color = a.TipoActividadColor,
                        Fecha = a.Fecha,
                        Lote = a.Lote,
                        Campania = a.Campania,
                        Cultivo = a.Cultivo,
                        Descripcion = $"{a.TipoActividad} - {a.ProductoAplicado ?? "Sin producto"} ({a.Dosis?.ToString("N2") ?? "-"} {a.UnidadDosis})",
                        ProductoAplicado = a.ProductoAplicado,
                        Dosis = a.Dosis,
                        Unidad = a.UnidadDosis ?? a.UnidadCantidad,
                        CostoARS = a.CostoARS,
                        CostoUSD = a.CostoUSD,
                        Responsable = a.Responsable
                    })
                    .ToList();

                // ============================================================
                // 8. Análisis de Insumos (agrupado por producto)
                // ============================================================
                var analisisInsumos = todasLasAplicaciones
                    .Where(a => !string.IsNullOrEmpty(a.ProductoAplicado))
                    .GroupBy(a => new { ProductoAplicado = a.ProductoAplicado ?? "", TipoProducto = a.TipoProducto ?? "" })
                    .Select(g => new InsumoConsumoDto
                    {
                        Producto = g.Key.ProductoAplicado,
                        TipoProducto = g.Key.TipoProducto,
                        CantidadTotal = Math.Round(g.Sum(a => a.CantidadTotal ?? 0), 2),
                        Unidad = g.Any(a => a.IdTipoActividad == 3) ? "Litros" : "Kg",
                        CostoTotalARS = Math.Round(g.Sum(a => a.CostoARS ?? 0), 2),
                        CostoTotalUSD = Math.Round(g.Sum(a => a.CostoUSD ?? 0), 2),
                        CantidadAplicaciones = g.Count(),
                        CantidadLotes = g.Select(a => a.IdLote).Distinct().Count(),
                        CultivoPrincipal = g.GroupBy(a => a.Cultivo)
                            .OrderByDescending(gc => gc.Count())
                            .Select(gc => gc.Key)
                            .FirstOrDefault(),
                        CampaniaPrincipal = g.GroupBy(a => a.Campania)
                            .OrderByDescending(gc => gc.Count())
                            .Select(gc => gc.Key)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(d => d.CantidadTotal)
                    .ToList();

                // ============================================================
                // 9. Trazabilidad (datos de auditoría)
                // ============================================================
                var trazabilidad = todasLasAplicaciones
                    .OrderByDescending(a => a.Fecha)
                    .Take(100)
                    .Select(a => new AplicacionTraceDto
                    {
                        Id = a.Id,
                        IdTipoActividad = a.IdTipoActividad,
                        TipoActividad = a.TipoActividad,
                        Icono = a.TipoActividadIcono,
                        Color = a.TipoActividadColor,
                        Fecha = a.Fecha,
                        Lote = a.Lote,
                        Campo = a.Campo,
                        Campania = a.Campania,
                        ProductoAplicado = a.ProductoAplicado,
                        Dosis = a.Dosis,
                        CantidadTotal = a.CantidadTotal,
                        CostoARS = a.CostoARS,
                        CostoUSD = a.CostoUSD,
                        Responsable = a.Responsable,
                        RegistrationDate = a.IdTipoActividad == 3
                            ? pulverizaciones.First(p => p.Id == a.Id).RegistrationDate ?? DateTime.MinValue
                            : fertilizaciones.First(f => f.Id == a.Id).RegistrationDate ?? DateTime.MinValue,
                        RegistrationUser = a.IdTipoActividad == 3
                            ? pulverizaciones.First(p => p.Id == a.Id).RegistrationUser
                            : fertilizaciones.First(f => f.Id == a.Id).RegistrationUser,
                        ModificationDate = a.IdTipoActividad == 3
                            ? pulverizaciones.First(p => p.Id == a.Id).ModificationDate
                            : fertilizaciones.First(f => f.Id == a.Id).ModificationDate,
                        ModificationUser = a.IdTipoActividad == 3
                            ? pulverizaciones.First(p => p.Id == a.Id).ModificationUser
                            : fertilizaciones.First(f => f.Id == a.Id).ModificationUser
                    })
                    .ToList();

                // ============================================================
                // 10. Datos para gráficos
                // ============================================================

                // 10a. Costos por producto (barras)
                var costosPorProducto = todasLasAplicaciones
                    .Where(a => !string.IsNullOrEmpty(a.ProductoAplicado) && (a.CostoARS.HasValue || a.CostoUSD.HasValue))
                    .GroupBy(a => new { ProductoAplicado = a.ProductoAplicado ?? "", TipoProducto = a.TipoProducto ?? "" })
                    .Select(g => new DatoAplicacionPorProducto
                    {
                        Producto = g.Key.ProductoAplicado,
                        TipoProducto = g.Key.TipoProducto,
                        CostoARS = Math.Round(g.Sum(a => a.CostoARS ?? 0), 2),
                        CostoUSD = Math.Round(g.Sum(a => a.CostoUSD ?? 0), 2),
                        CantidadTotal = Math.Round(g.Sum(a => a.CantidadTotal ?? 0), 2),
                        Unidad = g.Any(a => a.IdTipoActividad == 3) ? "Litros" : "Kg",
                        CantidadAplicaciones = g.Count(),
                        Color = g.Any(a => a.IdTipoActividad == 3) ? "#0d6efd" : "#198754"
                    })
                    .OrderByDescending(d => d.CostoARS)
                    .Take(15)
                    .ToList();

                // 10b. Distribución por tipo (torta/donut)
                var totalApps = (decimal)todasLasAplicaciones.Count;
                var distribucionPorTipo = new List<DatoAplicacionPorTipo>
                {
                    new DatoAplicacionPorTipo
                    {
                        Tipo = "Pulverización",
                        Cantidad = kpis.TotalPulverizaciones,
                        Porcentaje = totalApps > 0 ? Math.Round(kpis.TotalPulverizaciones / totalApps * 100, 1) : 0,
                        Color = "#0d6efd"
                    },
                    new DatoAplicacionPorTipo
                    {
                        Tipo = "Fertilización",
                        Cantidad = kpis.TotalFertilizaciones,
                        Porcentaje = totalApps > 0 ? Math.Round(kpis.TotalFertilizaciones / totalApps * 100, 1) : 0,
                        Color = "#198754"
                    }
                };

                // 10c. Timeline de aplicaciones (agrupado por mes)
                var appsTimeline = todasLasAplicaciones
                    .GroupBy(a => new { a.Fecha.Year, a.Fecha.Month })
                    .Select(g => new DatoAplicacionTimeline
                    {
                        Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                        FechaInicio = new DateTime(g.Key.Year, g.Key.Month, 1),
                        CantidadPulverizaciones = g.Count(a => a.IdTipoActividad == 3),
                        CantidadFertilizaciones = g.Count(a => a.IdTipoActividad == 4),
                        TotalAplicaciones = g.Count(),
                        CostoTotalARS = Math.Round(g.Sum(a => a.CostoARS ?? 0), 2)
                    })
                    .OrderBy(d => d.FechaInicio)
                    .ToList();

                // 10d. Comparativa por campaña (barras agrupadas)
                var comparativaCampania = todasLasAplicaciones
                    .Where(a => !string.IsNullOrEmpty(a.Campania))
                    .GroupBy(a => a.Campania!)
                    .Select(g => new DatoAplicacionPorCampania
                    {
                        Campania = g.Key,
                        TotalAplicaciones = g.Count(),
                        Pulverizaciones = g.Count(a => a.IdTipoActividad == 3),
                        Fertilizaciones = g.Count(a => a.IdTipoActividad == 4),
                        CostoTotalARS = Math.Round(g.Sum(a => a.CostoARS ?? 0), 2),
                        CostoTotalUSD = Math.Round(g.Sum(a => a.CostoUSD ?? 0), 2),
                        TotalLitros = Math.Round(g.Where(a => a.IdTipoActividad == 3).Sum(a => a.CantidadTotal ?? 0), 2),
                        TotalKg = Math.Round(g.Where(a => a.IdTipoActividad == 4).Sum(a => a.CantidadTotal ?? 0), 2)
                    })
                    .OrderBy(d => d.Campania)
                    .ToList();

                // 10e. Comparativa por campo (barras horizontales)
                var comparativaCampo = todasLasAplicaciones
                    .Where(a => !string.IsNullOrEmpty(a.Campo))
                    .GroupBy(a => new { Campo = a.Campo ?? "", a.IdCampo })
                    .Select(g => new DatoAplicacionPorCampo
                    {
                        Campo = g.Key.Campo,
                        TotalAplicaciones = g.Count(),
                        Pulverizaciones = g.Count(a => a.IdTipoActividad == 3),
                        Fertilizaciones = g.Count(a => a.IdTipoActividad == 4),
                        CostoTotalARS = Math.Round(g.Sum(a => a.CostoARS ?? 0), 2),
                        SuperficieHa = g.First().SuperficieHa ?? 0,
                        CantidadLotes = g.Select(a => a.IdLote).Distinct().Count()
                    })
                    .OrderByDescending(d => d.TotalAplicaciones)
                    .ToList();

                // ============================================================
                // 11. Indicadores inteligentes
                // ============================================================
                var indicadores = GenerarIndicadoresAplicacion(kpis, todasLasAplicaciones);

                // ============================================================
                // 12. Armar reporte final
                // ============================================================
                var reporte = new AplicacionReporteDto
                {
                    Kpis = kpis,
                    DatosAplicaciones = datosPaginados,
                    Timeline = timeline,
                    AnalisisInsumos = analisisInsumos,
                    Trazabilidad = trazabilidad,
                    CostosPorProducto = costosPorProducto,
                    DistribucionPorTipo = distribucionPorTipo,
                    AplicacionesTimeline = appsTimeline,
                    ComparativaPorCampania = comparativaCampania,
                    ComparativaPorCampo = comparativaCampo,
                    Indicadores = indicadores,
                    Paginacion = new PaginacionDto
                    {
                        PaginaActual = pagina,
                        TamanoPagina = tamanoPagina,
                        TotalRegistros = totalRegistros,
                        TotalPaginas = totalPaginas
                    }
                };

                return OperationResult<AplicacionReporteDto>.SuccessResult(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de aplicaciones agrícolas");
                return OperationResult<AplicacionReporteDto>.Failure(
                    $"Error al generar reporte de aplicaciones: {ex.Message}", "DATABASE_ERROR");
            }
        }

        /// <summary>
        /// Ordena aplicaciones por propiedad usando expresión switch
        /// </summary>
        private static object OrdenarAplicacionPorPropiedad(AplicacionLoteDto dto, string propiedad)
        {
            return propiedad switch
            {
                "Fecha" => (object)dto.Fecha,
                "TipoActividad" => dto.TipoActividad,
                "Lote" => dto.Lote,
                "Campo" => dto.Campo,
                "Campania" => (object?)dto.Campania ?? "",
                "Cultivo" => (object?)dto.Cultivo ?? "",
                "ProductoAplicado" => (object?)dto.ProductoAplicado ?? "",
                "Dosis" => (object?)dto.Dosis ?? 0m,
                "CantidadTotal" => (object?)dto.CantidadTotal ?? 0m,
                "CostoARS" => (object?)dto.CostoARS ?? 0m,
                "CostoUSD" => (object?)dto.CostoUSD ?? 0m,
                "CostoPorHa" => (object?)dto.CostoPorHa ?? 0m,
                "SuperficieHa" => (object?)dto.SuperficieHa ?? 0m,
                "Responsable" => (object?)dto.Responsable ?? "",
                _ => (object)dto.Fecha
            };
        }

        /// <summary>
        /// Genera indicadores inteligentes para el reporte de aplicaciones
        /// </summary>
        private static List<IndicadorInteligenteDto> GenerarIndicadoresAplicacion(
            AplicacionKpiDto kpis,
            List<AplicacionLoteDto> aplicaciones)
        {
            var indicadores = new List<IndicadorInteligenteDto>();

            // 1. Proporción Pulverización vs Fertilización
            if (kpis.TotalAplicaciones > 0)
            {
                var pctPulv = (decimal)kpis.TotalPulverizaciones / kpis.TotalAplicaciones * 100;
                var pctFert = (decimal)kpis.TotalFertilizaciones / kpis.TotalAplicaciones * 100;

                if (pctPulv > 75)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Predominio de pulverizaciones",
                        Severidad = "Media",
                        Titulo = "Alto porcentaje de pulverizaciones",
                        Mensaje = $"Las pulverizaciones representan el {pctPulv:N0}% del total de aplicaciones ({kpis.TotalPulverizaciones} de {kpis.TotalAplicaciones})",
                        Recomendacion = "Evaluar si se puede optimizar el manejo integrado de plagas para reducir aplicaciones.",
                        Icono = "ph-drop-half",
                        Color = "#0d6efd",
                        Valor = pctPulv,
                        Umbral = 75
                    });
                }

                if (kpis.TotalFertilizaciones == 0)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Sin fertilizaciones registradas",
                        Severidad = "Alta",
                        Titulo = "Sin fertilizaciones registradas",
                        Mensaje = "No se encontraron registros de fertilización en el período seleccionado.",
                        Recomendacion = "Verificar si las fertilizaciones se están registrando correctamente en el sistema.",
                        Icono = "ph-warning-circle",
                        Color = "#dc3545",
                        Valor = 0,
                        Umbral = 1
                    });
                }

                if (pctFert > 75)
                {
                    indicadores.Add(new IndicadorInteligenteDto
                    {
                        Tipo = "Predominio de fertilizaciones",
                        Severidad = "Baja",
                        Titulo = "Alto porcentaje de fertilizaciones",
                        Mensaje = $"Las fertilizaciones representan el {pctFert:N0}% del total de aplicaciones.",
                        Recomendacion = "Monitorear que las dosis de fertilización estén alineadas con los análisis de suelo.",
                        Icono = "ph-flask",
                        Color = "#198754",
                        Valor = pctFert,
                        Umbral = 75
                    });
                }
            }

            // 2. Costo elevado
            if (kpis.CostoTotalARS.HasValue && kpis.CostoTotalARS > 1000000)
            {
                indicadores.Add(new IndicadorInteligenteDto
                {
                    Tipo = "Costo elevado",
                    Severidad = "Media",
                    Titulo = "Costo total significativo",
                    Mensaje = $"El costo total de aplicaciones es de ${kpis.CostoTotalARS.Value:N0} ARS",
                    Recomendacion = "Revisar los costos unitarios de los productos y considerar alternativas más económicas.",
                    Icono = "ph-currency-circle-dollar",
                    Color = "#ffc107",
                    Valor = kpis.CostoTotalARS,
                    Umbral = 1000000
                });
            }

            // 3. Lote con más aplicaciones
            var loteTop = aplicaciones
                .GroupBy(a => new { a.IdLote, a.Lote })
                .Select(g => new { g.Key.Lote, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .FirstOrDefault();

            if (loteTop != null && loteTop.Cantidad > kpis.PromedioAplicacionesPorLote * 2)
            {
                indicadores.Add(new IndicadorInteligenteDto
                {
                    Tipo = "Lote con alta intensidad",
                    Severidad = "Media",
                    Titulo = $"Lote con muchas aplicaciones: {loteTop.Lote}",
                    Mensaje = $"El lote {loteTop.Lote} tiene {loteTop.Cantidad} aplicaciones, superando ampliamente el promedio de {kpis.PromedioAplicacionesPorLote:N1} por lote.",
                    Recomendacion = "Revisar el historial del lote para identificar posibles problemas sanitarios recurrentes.",
                    Icono = "ph-map-pin",
                    Color = "#fd7e14",
                    Valor = loteTop.Cantidad,
                    Umbral = kpis.PromedioAplicacionesPorLote * 2
                });
            }

            // 4. Superficie sin aplicaciones
            if (kpis.SuperficieTotalTratadaHa == 0 && kpis.TotalAplicaciones > 0)
            {
                indicadores.Add(new IndicadorInteligenteDto
                {
                    Tipo = "Sin superficie registrada",
                    Severidad = "Baja",
                    Titulo = "Superficie no disponible",
                    Mensaje = "No se pudo calcular la superficie tratada porque algunos lotes no tienen hectáreas registradas.",
                    Recomendacion = "Completar el dato de superficie en los lotes para mejorar los indicadores.",
                    Icono = "ph-info",
                    Color = "#6c757d",
                    Valor = 0,
                    Umbral = 1
                });
            }

            return indicadores;
        }
    }
}

