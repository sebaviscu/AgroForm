using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Fertilizacion : EntityBaseWithLicencia, ILabor
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

        public decimal? CantidadKgHa { get; set; }

        public int? IdNutriente { get; set; }
        public Catalogo Nutriente { get; set; } = null!;

        public int? IdTipoFertilizante { get; set; }
        public Catalogo TipoFertilizante { get; set; } = null!;

        public decimal? DosisKgHa { get; set; }
        public decimal? Costo { get; set; }

        public int IdMetodoAplicacion { get; set; }
        public Catalogo MetodoAplicacion { get; set; } = null!;
    }

}
