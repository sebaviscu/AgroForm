using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Cosecha : EntityBaseWithLicencia, ILabor
    {

        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? RendimientoTonHa { get; set; }
        public decimal? HumedadGrano { get; set; }
        public decimal? SuperficieCosechadaHa { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;

        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;
    }

}
