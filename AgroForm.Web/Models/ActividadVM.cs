namespace AgroForm.Web.Models
{
    public class ActividadVM : EntityBaseWithLicenciaVM
    {
        public string TipoActividad { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public int CampoId { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Responsable { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public TimeSpan? Duracion { get; set; }
    }
}
