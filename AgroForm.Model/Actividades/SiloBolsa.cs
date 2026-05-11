namespace AgroForm.Model.Actividades
{
    public class SiloBolsa : EntityBaseWithLicencia, ILabor, IEntityBaseWithCampania, IEntityBaseWithMoneda
    {
        // Propiedades específicas de Silo Bolsa
        public string Codigo { get; set; } = string.Empty;
        public decimal? Longitud { get; set; }
        public decimal? CapacidadTotalTn { get; set; }
        public decimal? HumedadGrano { get; set; }

        // ILabor
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;
        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;
        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        // IEntityBaseWithMoneda
        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;
        public bool EsDolar => IdMoneda == 2;

        // IEntityBaseWithCampania
        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;
        public int IdCicloCultivo { get; set; }
        public CicloCultivo CicloCultivo { get; set; } = null!;
    }
}
