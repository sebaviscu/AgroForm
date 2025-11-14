using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model.Actividades
{
    public class Monitoreo : EntityBaseWithLicencia, ILabor
    {
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public IdMonitoreoEnum IdMonitoreo { get; set; }

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? IdEstadoFenologico { get; set; }
        public EstadoFenologico? EstadoFenologico { get; set; }

        public int? IdTipoMonitoreo { get; set; }
        public Catalogo TipoMonitoreo { get; set; } = null!;

        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;
        public bool EsDolar => Moneda != null && Moneda.Id == 2 ? true : false;
    }

}
