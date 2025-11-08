using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class SiembraVM : ActividadVM
    {
        public decimal? SuperficieHa { get; set; }
        public decimal? DensidadSemillaKgHa { get; set; }

        public int IdCultivo { get; set; }
        public CultivoVM Cultivo { get; set; } = null!;

        public int? IdVariedad { get; set; }
        public VariedadVM? Variedad { get; set; }

        public int IdMetodoSiembra { get; set; }
        public CatalogoVM MetodoSiembra { get; set; } = null!;
    }

}
