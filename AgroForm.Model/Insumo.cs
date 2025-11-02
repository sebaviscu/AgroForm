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
        public decimal? PrecioActual { get; set; }
        public bool Activo { get; set; } = true;
        public int IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        public int? IdMarca { get; set; }
        public Marca? Marca { get; set; }

        public int? IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        public int? IdTipoInsumo { get; set; }
        public TipoInsumo? TipoInsumo{ get; set; }

        public ICollection<HistoricoPrecioInsumo> HistoricoPrecios { get; set; } = new List<HistoricoPrecioInsumo>();

    }
}
