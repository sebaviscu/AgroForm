using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class CampoVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public decimal? SuperficieHectareas { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string CoordenadasPoligono { get; set; } = string.Empty; // JSON con las coordenadas

        public ICollection<LoteVM> Lotes { get; set; } = new List<LoteVM>();
    }
}
