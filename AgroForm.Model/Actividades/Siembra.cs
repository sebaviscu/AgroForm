using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Siembra : EntityBaseWithLicencia, ILabor
    {
        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public decimal? SuperficieHa { get; set; }
        public decimal? DensidadSemillaKgHa { get; set; }
        public decimal? Costo { get; set; }

        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;

        public int? IdVariedad { get; set; }
        public Variedad? Variedad { get; set; }

        public int IdMetodoSiembra { get; set; }
        public Catalogo MetodoSiembra { get; set; } = null!;
    }

}
