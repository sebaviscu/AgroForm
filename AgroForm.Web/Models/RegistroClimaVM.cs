using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class RegistroClimaVM : EntityBaseWithLicenciaVM
    {
        public DateTime Fecha { get; set; }
        public decimal Milimetros { get; set; }

        public int LoteId { get; set; }
    }
}
