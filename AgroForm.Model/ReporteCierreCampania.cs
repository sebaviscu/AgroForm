using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class ReporteCierreCampania : EntityBaseWithLicencia
    {
        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        // Datos generales
        public string NombreCampania { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Resumen global
        public decimal SuperficieTotalHa { get; set; }
        public decimal ToneladasProducidas { get; set; }
        public decimal CostoTotalArs => AnalisisSueloArs+CostoSiembrasArs+CostoRiegosArs+CostoPulverizacionesArs+
                                       CostoCosechasArs+CostoMonitoreosArs+CostoFertilizantesArs+CostoOtrasLaboresArs;
        public decimal CostoTotalUsd => AnalisisSueloUsd+CostoSiembrasUsd+CostoRiegosUsd+CostoPulverizacionesUsd+
                                       CostoCosechasUsd+CostoMonitoreosUsd+CostoFertilizantesUsd+CostoOtrasLaboresUsd;
        public decimal CostoPorHa { get; set; }
        public decimal CostoPorTonelada { get; set; }
        public decimal RendimientoPromedioHa { get; set; }


        // Cantidad de labores por tipo
        public decimal AnalisisSueloArs { get; set; }
        public decimal AnalisisSueloUsd { get; set; }

        public decimal CostoSiembrasArs { get; set; }
        public decimal CostoSiembrasUsd { get; set; }

        public decimal CostoRiegosArs { get; set; }
        public decimal CostoRiegosUsd { get; set; }

        public decimal CostoPulverizacionesArs { get; set; }
        public decimal CostoPulverizacionesUsd { get; set; }

        public decimal CostoCosechasArs { get; set; }
        public decimal CostoCosechasUsd { get; set; }

        public decimal CostoMonitoreosArs { get; set; }
        public decimal CostoMonitoreosUsd { get; set; }

        public decimal CostoFertilizantesArs { get; set; }
        public decimal CostoFertilizantesUsd { get; set; }

        public decimal CostoOtrasLaboresArs { get; set; }
        public decimal CostoOtrasLaboresUsd { get; set; }

        // Gastos

        public decimal GastosTotalesArs { get; set; }
        public decimal GastosTotalesUsd { get; set; }
        public string GastosPorCategoriaJson { get; set; } = string.Empty;

        // Datos climáticos
        public decimal LluviaAcumuladaTotal { get; set; }
        public string LluviasPorMesJson { get; set; } = string.Empty; // JSON con { "Mes": "Enero", "Lluvia": 150.5 }
        public string EventosExtremosJson { get; set; } = string.Empty; // JSON con eventos climáticos

        // Datos detallados (JSON para evitar tablas excesivas)
        public string ResumenPorCultivoJson { get; set; } = string.Empty;
        public string ResumenPorCampoJson { get; set; } = string.Empty;
        public string ResumenPorLoteJson { get; set; } = string.Empty;

    }

    // Modelos para los datos JSON
    public class ResumenCultivo
    {
        public string NombreCultivo { get; set; } = string.Empty;
        public decimal SuperficieHa { get; set; }
        public decimal ToneladasProducidas { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal RendimientoHa { get; set; }
        public Dictionary<string, decimal> CostosPorTipo { get; set; } = new();
        public Dictionary<string, int> LaboresPorTipo { get; set; } = new();
    }

    public class ResumenCampo
    {
        public string NombreCampo { get; set; } = string.Empty;
        public decimal SuperficieHa { get; set; }
        public decimal ToneladasProducidas { get; set; }
        public decimal CostoTotalArs { get; set; }
        public decimal CostoTotalUsd { get; set; }
        public List<ResumenLote> Lotes { get; set; } = new();
    }

    public class ResumenLote
    {
        public string NombreLote { get; set; } = string.Empty;
        public string? Cultivo { get; set; }
        public decimal SuperficieHa { get; set; }
        public decimal ToneladasProducidas { get; set; }
        public decimal CostoTotalArs { get; set; }
        public decimal CostoTotalUsd { get; set; }
        public decimal RendimientoHa { get; set; }
    }
}
