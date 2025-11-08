using AgroForm.Model;
using AgroForm.Model.Actividades;

namespace AgroForm.Web.Models
{
    public class LoteVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int idCampo { get; set; }

        public int idCampania { get; set; }

        public ICollection<LaborDTO> Actividades { get; set; } = new List<LaborDTO>();

    }
}
