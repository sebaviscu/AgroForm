using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class HistoricoPrecioInsumoVM : EntityBaseWithLicenciaVM
    {
        public int InsumoId { get; set; }

        public int MonedaId { get; set; }

        public decimal Precio { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public string? Observaciones { get; set; }
    }
}
