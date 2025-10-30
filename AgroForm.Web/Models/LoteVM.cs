using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class LoteVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int CampoId { get; set; }

        public int CampaniaId { get; set; }
    }
}
