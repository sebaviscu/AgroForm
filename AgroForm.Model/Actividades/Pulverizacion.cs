using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Pulverizacion : EntityBaseWithLicencia, ILabor
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

        public decimal? Costo { get; set; }

        public decimal? VolumenLitrosHa { get; set; }
        public decimal? Dosis { get; set; }
        public string CondicionesClimaticas { get; set; } = string.Empty;

        public int IdProductoAgroquimico { get; set; }
        public Catalogo ProductoAgroquimico { get; set; } = null!;
    }

}
