using AgroForm.Model;
using AgroForm.Model.Unidades;

namespace AgroForm.Web.Models
{
    public class FertilizacionVM : ActividadVM
    {
        public decimal? CantidadKgHa { get; set; }

        /// <summary>
        /// Original entity values (maps from/to entity)
        /// </summary>
        public decimal? Cantidad { get; set; }
        public int? IdUnidadCantidad { get; set; }
        public UnidadMedida? UnidadCantidad { get; set; }
        public decimal? Dosis { get; set; }
        public int? IdUnidadDosis { get; set; }
        public UnidadMedida? UnidadDosis { get; set; }

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
