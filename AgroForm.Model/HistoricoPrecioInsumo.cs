using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class HistoricoPrecioInsumo : EntityBaseWithLicencia
    {
        public int InsumoId { get; set; }
        public Insumo Insumo { get; set; } = null!;

        public int MonedaId { get; set; }
        public Moneda Moneda { get; set; } = null!;

        public decimal Precio { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public string? Observaciones { get; set; }
    }

}
