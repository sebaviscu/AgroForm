using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class FertilizacionVM : ActividadVM
    {
        public decimal? CantidadKgHa { get; set; }

        public int? IdNutriente { get; set; }
        public CatalogoVM Nutriente { get; set; } = null!;

        public int? IdTipoFertilizante { get; set; }
        public CatalogoVM TipoFertilizante { get; set; } = null!;

        public decimal? DosisKgHa { get; set; }
        public decimal? Costo { get; set; }

        public int IdMetodoAplicacion { get; set; }
        public CatalogoVM MetodoAplicacion { get; set; } = null!;
    }

}
