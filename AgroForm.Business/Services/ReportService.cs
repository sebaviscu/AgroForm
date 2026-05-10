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
                // 1. Obtener lotes con filtros
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

                // Filtro por campaña
                if (idCampania.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampania == idCampania.Value);
                else if (_userContext.IdCampaña.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampania == _userContext.IdCampaña.Value);

                // Filtro por campo
                if (idCampo.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.IdCampo == idCampo.Value);

                // Filtro por lote
                if (idLote.HasValue)
                    lotesQuery = lotesQuery.Where(l => l.Id == idLote.Value);

                var lotes = await lotesQuery.ToListAsync();

                // 2. Construir DTOs
                var result = new List<ReporteComparativaCampoDto>();

                foreach (var lote in lotes)
                {
                    // Get all siembras and cosechas directly from lote
                    var todasLasSiembras = lote.Siembras.ToList();
                    var todasLasCosechas = lote.Cosechas.ToList();

                    // Obtener última siembra y cosecha
                    var ultimaSiembra = todasLasSiembras
                        .OrderByDescending(s => s.Fecha)
                        .FirstOrDefault();

                    var ultimaCosecha = todasLasCosechas
                        .OrderByDescending(c => c.Fecha)
                        .FirstOrDefault();

                    // Filtrar por cultivo si se especificó
                    if (idCultivo.HasValue && ultimaSiembra != null && ultimaSiembra.IdCultivo != idCultivo.Value)
                        continue;

                    // Calcular totales de fertilizantes (kg/ha)
                    var totalFertKgHa = lote.Fertilizaciones
                        .Where(f => f.CantidadKgHa.HasValue)
                        .Sum(f => f.CantidadKgHa!.Value);

                    // Calcular total de pulverizaciones (L/ha)
                    var totalPulvLtsHa = lote.Pulverizaciones
                        .Where(p => p.VolumenLitrosHa.HasValue)
                        .Sum(p => p.VolumenLitrosHa!.Value);

                    // Calcular costos totales
                    var costosARS = ObtenerCostosARS(lote);
                    var costosUSD = ObtenerCostosUSD(lote);

                    // Rendimiento
                    var rendimientoTonHa = ultimaCosecha?.RendimientoTonHa;
                    var superficieHa = lote.SuperficieHectareas ?? ultimaSiembra?.SuperficieHa ?? 0;
                    decimal? rendimientoTotal = rendimientoTonHa.HasValue && superficieHa > 0
                        ? rendimientoTonHa.Value * superficieHa
                        : null;

                    // Costo por hectárea
                    var costoPorHaARS = superficieHa > 0 ? costosARS / superficieHa : costosARS;
                    var costoPorHaUSD = superficieHa > 0 ? costosUSD / superficieHa : costosUSD;

                    // Cantidad de labores
                    var cantidadLabores = todasLasSiembras.Count
                        + todasLasCosechas.Count
                        + lote.Fertilizaciones.Count
                        + lote.Pulverizaciones.Count
                        + lote.Riegos.Count
                        + lote.Monitoreos.Count
                        + lote.AnalisisSuelos.Count
                        + lote.OtrasLabores.Count;

                    // Margen bruto estimado (solo si hay rendimiento y costos)
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

                // 3. Calcular ranking por rendimiento
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
