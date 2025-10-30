using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Insumo : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? UnidadMedida { get; set; }
        public decimal? StockActual { get; set; }
        public decimal? StockMinimo { get; set; }

        public int? MarcaId { get; set; }
        public Marca? Marca { get; set; }

        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public int? TipoInsumoId { get; set; }
        public TipoInsumo? TipoInsumo{ get; set; }

        public bool Estado { get; set; } = true;
        public ICollection<MovimientoInsumo> MovimientosInsumo { get; set; } = new List<MovimientoInsumo>();
        public ICollection<HistoricoPrecioInsumo> HistoricoPrecios { get; set; } = new List<HistoricoPrecioInsumo>();

    }
}
