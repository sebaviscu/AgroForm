using AgroForm.Model;

namespace AgroForm.Web.Models.IndexVM
{
    public class SelectorCampaniasVM
    {
        public List<Campania> Campanias { get; set; } = new List<Campania>();
        public int? IdCampaniaSeleccionada { get; set; }
    }
}
