using AgroForm.Business.Contracts;

namespace AgroForm.Web.Models
{
    public class ReporteComparativaVM
    {
        /// <summary>
        /// Lista de datos comparativos de campos/lotes
        /// </summary>
        public List<ReporteComparativaCampoDto> Datos { get; set; } = new();

        // Filtros aplicados
        public int? FiltroIdCampania { get; set; }
        public int? FiltroIdCampo { get; set; }
        public int? FiltroIdLote { get; set; }
        public int? FiltroIdCultivo { get; set; }

        // Datos para los filtros (selects)
        public List<FiltroItem> Campos { get; set; } = new();
        public List<FiltroItem> Lotes { get; set; } = new();
        public List<FiltroItem> Cultivos { get; set; } = new();

        // Métricas resumen
        public ResumenComparativa Resumen { get; set; } = new();

        /// <summary>
        /// Moneda seleccionada: "ARS" o "USD"
        /// </summary>
        public string Moneda { get; set; } = "ARS";
    }

    public class ResumenComparativa
    {
        public int TotalLotes { get; set; }
        public int LotesConRendimiento { get; set; }
        public decimal? RendimientoPromedio { get; set; }
        public decimal? RendimientoMaximo { get; set; }
        public string? LoteMejorRendimiento { get; set; }
        public decimal? RendimientoMinimo { get; set; }
        public string? LotePeorRendimiento { get; set; }
        public decimal? CostoPromedioPorHa { get; set; }
        public decimal? SuperficieTotal { get; set; }
    }

    public class FiltroItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
