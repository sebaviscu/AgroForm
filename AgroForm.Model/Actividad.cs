using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Actividad : EntityBaseWithLicencia
    {
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? Cantidad { get; set; }

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? IdInsumo { get; set; }
        public Insumo? Insumo { get; set; }

    }
}
