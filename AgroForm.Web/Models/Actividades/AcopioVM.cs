using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class AcopioVM : ActividadVM
    {
        public TipoAcopio TipoAcopio { get; set; } = TipoAcopio.SiloBolsa;
        public string Codigo { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public int IdCultivo { get; set; }
        public string? Cultivo { get; set; }
        public decimal? CantidadActualTn { get; set; }
        public decimal? CapacidadTotalTn { get; set; }
        public decimal? HumedadGrano { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;

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
