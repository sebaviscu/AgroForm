using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class PagoLicenciaVM : EntityBaseWithLicenciaVM
    {
        public int IdLicencia { get; set; }
        public Licencia? Licencia { get; set; }

        public TipoPagoLicenciaEnum TipoPagoLicencia { get; set; }
        public decimal Precio { get; set; }
        public DateTime Fecha { get; set; }
    }
}
