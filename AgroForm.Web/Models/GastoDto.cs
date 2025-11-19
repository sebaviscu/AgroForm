namespace AgroForm.Web.Models
{
    public class GastoDto
    {
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
    }
}
