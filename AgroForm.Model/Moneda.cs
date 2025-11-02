using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Moneda: EntityBase
    {
        public string Codigo { get; set; } = string.Empty; // "ARS", "USD", "EUR"
        public string Nombre { get; set; } = string.Empty; // "Peso Argentino", "Dólar Estadounidense"
        public string? Simbolo { get; set; } // "$", "U$S"
        public decimal? TipoCambioReferencia { get; set; } // tipo de cambio actual (opcional)

        public ICollection<HistoricoPrecioInsumo> HistoricoPrecios { get; set; } = new List<HistoricoPrecioInsumo>();

    }
}
