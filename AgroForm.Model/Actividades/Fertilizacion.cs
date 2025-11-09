using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Fertilizacion : EntityBaseWithLicencia, ILabor
    {
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public decimal? DosisKgHa { get; set; }
        public decimal? CantidadKgHa { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }


        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdTipoActividad { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int? IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? IdNutriente { get; set; }
        public Catalogo Nutriente { get; set; } = null!;

        public int? IdTipoFertilizante { get; set; }
        public Catalogo TipoFertilizante { get; set; } = null!;

        public int? IdMetodoAplicacion { get; set; }
        public Catalogo MetodoAplicacion { get; set; } = null!;

        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

    }

}
