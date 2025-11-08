using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class RiegoVM : ActividadVM
    {
        public decimal? HorasRiego { get; set; }
        public decimal? VolumenAguaM3 { get; set; }

        public int IdMetodoRiego { get; set; }
        public CatalogoVM MetodoRiego { get; set; } = null!;

        public int? IdFuenteAgua { get; set; }
        public CatalogoVM? FuenteAgua { get; set; }
    }

}
