namespace AgroForm.Web.Models
{
    public class MonedaVM : EntityBaseVM
    {
        public string Codigo { get; set; } = string.Empty; // "ARS", "USD", "EUR"
        public string Nombre { get; set; } = string.Empty; // "Peso Argentino", "Dólar Estadounidense"
        public string NombreUpper => Nombre.ToUpper();
        public string? Simbolo { get; set; } // "$", "U$S"
        public decimal? TipoCambioReferencia { get; set; } // tipo de cambio actual (opcional)

        public string FechaActualizadfo => ModificationDate.HasValue ? ModificationDate.Value.ToString("dd/MM/yyyy HH:mm") : "-";
    }
}
