using static AgroForm.Model.EnumClass;

namespace AgroForm.Model.Actividades
{
    public class Acopio : EntityBaseWithLicencia, ILabor, IEntityBaseWithCampania, IEntityBaseWithMoneda
    {
        // Propiedades generales (todos los tipos de acopio)
        public TipoAcopio TipoAcopio { get; set; } = TipoAcopio.SiloBolsa;
        public string Codigo { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;
        public int IdLote { get; set; }
        public Lote? Lote { get; set; }
        public decimal? CantidadActualTn { get; set; }
        public decimal? CapacidadTotalTn { get; set; }
        public decimal? HumedadGrano { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;

        // ILabor
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
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

        // --- Campos específicos por tipo de acopio ---

        // Silo Bolsa
        public decimal? Longitud { get; set; }
        public decimal? Diametro { get; set; }
        public DateTime? FechaEmbolsado { get; set; }

        // Silo
        public string? TipoSilo { get; set; }
        public bool? Aireacion { get; set; }
        public decimal? TemperaturaGrano { get; set; }

        // Planta Externa
        public string? Empresa { get; set; }
        public string? NumeroContrato { get; set; }
        public decimal? TarifaAlmacenaje { get; set; }
    }
}
