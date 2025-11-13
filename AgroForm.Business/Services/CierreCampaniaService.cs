using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
     public class CierreCampaniaService : ServiceBase<ReporteCierreCampania>, ICierreCampaniaService 
    {

        private readonly IGenericRepository<Campania> _campaniaRepo;

        public CierreCampaniaService(
        IDbContextFactory<AppDbContext> contextFactory, 
        ILogger<ServiceBase<ReporteCierreCampania>> logger, 
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
            : base(contextFactory, logger, httpContextAccessor)
        {
            var _campaniaRepo = unitOfWork.Repository<Campania >();

        }


     public async Task<ReporteCierreCampania> GenerarReporteCierreAsync(int idCampania)
        {
            var campania = await _campaniaRepo
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.AnalisisSuelos)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Cosechas)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Fertilizaciones)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Monitoreos)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.OtrasLabores)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Siembras)
                        .ThenInclude(l => l.Cultivo)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Riegos)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.Pulverizaciones)
                .Include(c => c.Lotes)
                    .ThenInclude(l => l.RegistrosClima)
                .FirstOrDefaultAsync(c => c.Id == idCampania);

            if (campania == null) throw new Exception("Campaña no encontrada");

            var reporte = new ReporteCierreCampania
            {
                IdCampania = idCampania,
                NombreCampania = campania.Nombre,
                FechaInicio = campania.FechaInicio,
                FechaFin = campania.FechaFin.GetValueOrDefault()
            };

            // Calcular datos generales
            CalcularDatosGeneralesAsync(campania, reporte);

            // Calcular datos por cultivo
            CalcularDatosPorCultivoAsync(campania, reporte);

            // Calcular datos por campo y lote
            CalcularDatosPorCampoAsync(campania, reporte);

            // Calcular datos climáticos
            CalcularDatosClimaticosAsync(campania, reporte);

            return reporte;
        }

        private void CalcularDatosGeneralesAsync(Campania campania, ReporteCierreCampania reporte)
        {
            var lotes = campania.Lotes.ToList();

            // Obtener cosechas para calcular producción
            var cosechas = lotes.SelectMany(_=>_.Cosechas).ToList();
            reporte.ToneladasProducidas = cosechas.Sum(c => c.RendimientoTonHa * c.SuperficieCosechadaHa) ?? 0;


            // Superficie y producción
            reporte.SuperficieTotalHa = cosechas.Sum(c => c.SuperficieCosechadaHa) ?? 0;


            reporte.CostoCosechasArs= cosechas.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoCosechasUsd= cosechas.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var analisisSuelo = lotes.SelectMany(_=>_.AnalisisSuelos).ToList();
            reporte.AnalisisSueloArs = analisisSuelo.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.AnalisisSueloUsd = analisisSuelo.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var riegos = lotes.SelectMany(_=>_.Riegos).ToList();
            reporte.CostoRiegosArs = riegos.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoRiegosUsd= riegos.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var Siembras = lotes.SelectMany(_=>_.Siembras).ToList();
            reporte.CostoSiembrasArs = Siembras.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoSiembrasUsd= Siembras.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var Fertilizaciones = lotes.SelectMany(_=>_.Fertilizaciones).ToList();
            reporte.CostoFertilizantesArs = Fertilizaciones.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoFertilizantesUsd = Fertilizaciones.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var OtrasLabores = lotes.SelectMany(_=>_.OtrasLabores).ToList();
            reporte.CostoOtrasLaboresArs = OtrasLabores.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoOtrasLaboresUsd = OtrasLabores.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var Pulverizaciones = lotes.SelectMany(_=>_.Pulverizaciones).ToList();
            reporte.CostoPulverizacionesArs = Pulverizaciones.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoPulverizacionesUsd = Pulverizaciones.Sum(_ => _.CostoUSD.GetValueOrDefault());

            var Monitoreos = lotes.SelectMany(_=>_.Monitoreos).ToList();
            reporte.CostoMonitoreosArs = Monitoreos.Sum(_ => _.CostoARS.GetValueOrDefault());
            reporte.CostoMonitoreosUsd = Monitoreos.Sum(_ => _.CostoUSD.GetValueOrDefault());

            // Costos
            reporte.CostoPorHa = reporte.SuperficieTotalHa > 0 ? reporte.CostoTotalArs / reporte.SuperficieTotalHa : 0;
            reporte.CostoPorTonelada = reporte.ToneladasProducidas > 0 ? reporte.CostoTotalArs / reporte.ToneladasProducidas : 0;
            reporte.RendimientoPromedioHa = reporte.SuperficieTotalHa > 0 ? reporte.ToneladasProducidas / reporte.SuperficieTotalHa : 0;


        }

        private void CalcularDatosPorCultivoAsync(Campania campania, ReporteCierreCampania reporte)
        {
            var lotes = campania.Lotes.ToList();
            var Siembras = lotes.SelectMany(_ => _.Siembras).ToList();

            var cultivos = Siembras
                .Where(x => x.Cultivo != null)
                .GroupBy(x => x.Cultivo)
                .ToList();

            var resumenCultivos = new List<ResumenCultivo>();

            foreach (var grupo in cultivos)
            {
                var lotesGrupo = grupo.Select(x => x.Lote).ToList();
                var laboresGrupo = lotesGrupo.SelectMany(l => l.Cosechas).ToList();

                var resumen = new ResumenCultivo
                {
                    // Aquí necesitarías obtener el nombre del cultivo desde la base de datos
                    NombreCultivo = $"Cultivo {grupo.Key}",
                    SuperficieHa = lotesGrupo.Sum(l => l.SuperficieHectareas ?? 0),
                    ToneladasProducidas = laboresGrupo
                        .Sum(c => c.RendimientoTonHa * c.SuperficieCosechadaHa) ?? 0,
                    CostoTotal = laboresGrupo.Sum(l => l.CostoARS ?? 0),
                    RendimientoHa = 0 // Calcular basado en superficie y producción
                };

                resumenCultivos.Add(resumen);
            }

            reporte.ResumenPorCultivoJson = JsonSerializer.Serialize(resumenCultivos);
        }

        private void CalcularDatosPorCampoAsync(Campania campania, ReporteCierreCampania reporte)
        {
            // Similar a CalcularDatosPorCultivoAsync pero agrupando por campo
            // Implementar lógica similar...
        }

        private void CalcularDatosClimaticosAsync(Campania campania, ReporteCierreCampania reporte)
        {
            var registrosClima = campania.Lotes.SelectMany(l => l.RegistrosClima).ToList();

            reporte.LluviaAcumuladaTotal = registrosClima.Sum(r => r.Milimetros);

            var lluviasPorMes = registrosClima
                .GroupBy(r => r.Fecha.ToString("MM-yyyy"))
                .Select(g => new { Mes = g.Key, Lluvia = g.Sum(r => r.Milimetros) })
                .ToList();

            reporte.LluviasPorMesJson = JsonSerializer.Serialize(lluviasPorMes);

            var eventosExtremos = registrosClima
                .Where(r => r.TipoClima != TipoClima.Lluvia)
                .Select(r => new {
                    Fecha = r.Fecha,
                    Tipo = r.TipoClima.ToString()
                })
                .ToList();

            reporte.EventosExtremosJson = JsonSerializer.Serialize(eventosExtremos);
        }

        public async Task<byte[]> GenerarPdfReporteAsync(int idCampania)
        {
            var reporte = await GetByIdAsync(idCampania);

            if (reporte == null)
            {
                reporte = await GenerarReporteCierreAsync(idCampania);
            }

            //return await _pdfService.GenerarPdfCierreCampaniaAsync(reporte);
            return default;
        }

        public async Task<List<ReporteCierreCampania>> ObtenerReportesAnterioresAsync(int idLicencia)
        {
            return await GetQuery()
                .Include(r => r.Campania)
                .Where(r => r.IdLicencia == idLicencia && r.EsDefinitivo)
                .OrderByDescending(r => r.FechaFin)
                .ToListAsync();
        }
    }
}
}
