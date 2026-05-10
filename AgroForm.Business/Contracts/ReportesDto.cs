namespace AgroForm.Business.Contracts
{
    public class ReporteComparativaCampoDto
    {
        public int IdCampo { get; set; }
        public string Campo { get; set; } = string.Empty;
        public int IdLote { get; set; }
        public string Lote { get; set; } = string.Empty;
        public decimal? SuperficieHa { get; set; }

        // Cultivo
        public string? Cultivo { get; set; }
        public int? IdCultivo { get; set; }

        // Fechas
        public DateTime? FechaSiembra { get; set; }
        public DateTime? FechaCosecha { get; set; }

        // Rendimiento
        public decimal? RendimientoTonHa { get; set; }
        public decimal? RendimientoTotalTon { get; set; }

        // Insumos / Fertilizantes
        public decimal? TotalFertilizantesKgHa { get; set; }
        public decimal? TotalPulverizacionesLtsHa { get; set; }

        // Costos
        public decimal? CostoTotalARS { get; set; }
        public decimal? CostoTotalUSD { get; set; }
        public decimal? CostoPorHaARS { get; set; }
        public decimal? CostoPorHaUSD { get; set; }

        // Margen bruto (estimado)
        public decimal? MargenBrutoARS { get; set; }
        public decimal? MargenBrutoUSD { get; set; }

        // Cantidad de labores
        public int CantidadLabores { get; set; }

        // Para ranking
        public int RankingRendimiento { get; set; }
    }
}
