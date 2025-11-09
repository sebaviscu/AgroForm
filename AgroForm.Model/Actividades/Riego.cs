using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Riego : EntityBaseWithLicencia, ILabor
    {
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }
        public decimal? HorasRiego { get; set; }
        public decimal? VolumenAguaM3 { get; set; }

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;

        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;


        public int? IdMetodoRiego { get; set; }
        public Catalogo MetodoRiego { get; set; } = null!;

        public int? IdFuenteAgua { get; set; }
        public Catalogo? FuenteAgua { get; set; }

        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;
    }

}
