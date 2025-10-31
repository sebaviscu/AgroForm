namespace AgroForm.Web.Models
{
    public class MonedaVM : EntityBaseVM
    {
        public string Codigo { get; set; } = string.Empty; // "ARS", "USD", "EUR"
        public string Nombre { get; set; } = string.Empty; // "Peso Argentino", "Dólar Estadounidense"
        public string? Simbolo { get; set; } // "$", "U$S"
        public decimal? TipoCambioReferencia { get; set; } // tipo de cambio actual (opcional)
    }
}
