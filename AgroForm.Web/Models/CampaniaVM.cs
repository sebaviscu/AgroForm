using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class CampaniaVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public EstadosCamapaña EstadosCamapaña { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public ICollection<LoteVM> Lotes { get; set; } = new List<LoteVM>();
        public string EstadoDisplay => EstadosCamapaña.GetDisplayName();
    }
}
