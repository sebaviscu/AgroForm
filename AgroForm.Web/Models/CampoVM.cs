namespace AgroForm.Web.Models
{
    public class CampoVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public decimal? SuperficieHectareas { get; set; }
    }
}
