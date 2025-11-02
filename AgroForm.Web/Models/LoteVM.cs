using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class LoteVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int idCampo { get; set; }

        public int idCampania { get; set; }

        public ICollection<ActividadVM> Actividades { get; set; } = new List<ActividadVM>();

    }
}
