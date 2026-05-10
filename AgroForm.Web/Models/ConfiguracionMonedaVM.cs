namespace AgroForm.Web.Models
{
    public class ConfiguracionMonedaVM
    {
        public int IdMonedaReferencia { get; set; }
        public List<MonedaVM> MonedasDisponibles { get; set; } = new();
    }
}
