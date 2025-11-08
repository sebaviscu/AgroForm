using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class PulverizacionVM: ActividadVM
    {
        public decimal? VolumenLitrosHa { get; set; }
        public decimal? Dosis { get; set; }
        public string CondicionesClimaticas { get; set; } = string.Empty;

        public int IdProductoAgroquimico { get; set; }
        public CatalogoVM ProductoAgroquimico { get; set; } = null!;
    }

}
