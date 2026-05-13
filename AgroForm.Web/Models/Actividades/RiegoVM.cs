using AgroForm.Model;
using AgroForm.Model.Unidades;

namespace AgroForm.Web.Models
{
    public class RiegoVM : ActividadVM
    {
        public decimal? HorasRiego { get; set; }
        public decimal? VolumenAguaM3 { get; set; }

        /// <summary>
        /// Original volume value (maps from entity)
        /// </summary>
        public decimal? VolumenAgua { get; set; }
        public int? IdUnidadVolumenAgua { get; set; }
        public UnidadMedida? UnidadVolumenAgua { get; set; }

        public int IdMetodoRiego { get; set; }
        public CatalogoVM MetodoRiego { get; set; } = null!;

        public int? IdFuenteAgua { get; set; }
        public CatalogoVM? FuenteAgua { get; set; }
    }

}
