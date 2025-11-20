using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Siembra : EntityBaseWithLicencia, ILabor, IEntityBaseWithCampania, IEntityBaseWithMoneda
    {

        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? SuperficieHa { get; set; }
        public decimal? DensidadSemillaKgHa { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }


        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;
        public bool EsDolar => Moneda != null && Moneda.Id == 2 ? true : false;

        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;

        public int? IdVariedad { get; set; }
        public Variedad? Variedad { get; set; }

        public int? IdMetodoSiembra { get; set; }
        public Catalogo MetodoSiembra { get; set; } = null!;

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;
    }

}
