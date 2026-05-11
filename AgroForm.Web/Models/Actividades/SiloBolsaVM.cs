namespace AgroForm.Web.Models
{
    public class SiloBolsaVM : ActividadVM
    {
        public string Codigo { get; set; } = string.Empty;
        public decimal? Longitud { get; set; }
        public decimal? CapacidadTotalTn { get; set; }
        public decimal? HumedadGrano { get; set; }
    }
}
