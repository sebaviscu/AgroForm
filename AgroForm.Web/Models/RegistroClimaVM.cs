using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class RegistroClimaVM : EntityBaseWithLicenciaVM
    {
        public DateTime Fecha { get; set; }
        public decimal Milimetros { get; set; } = 0m;
        public TipoClima TipoClima { get; set; }
        public string? Observaciones { get; set; }
        public int IdCampo { get; set; }
        public CampoVM? Campo { get; set; }

        public bool EsLluvia => TipoClima == TipoClima.Lluvia;
        public string TipoClimaString => TipoClima.ToString();
    }
}
