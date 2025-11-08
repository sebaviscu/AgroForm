using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class AnalisisSueloVM : ActividadVM
    {
        public decimal? ProfundidadCm { get; set; }
        public decimal? PH { get; set; }
        public decimal? MateriaOrganica { get; set; }
        public decimal? Nitrogeno { get; set; }
        public decimal? Fosforo { get; set; }
        public decimal? Potasio { get; set; }
        public decimal? ConductividadElectrica { get; set; }
        public decimal? CIC { get; set; } // Capacidad de intercambio catiónico
        public string Textura { get; set; } = string.Empty;

        public int? IdLaboratorio { get; set; }
        public CatalogoVM? Laboratorio { get; set; }

    }

}
