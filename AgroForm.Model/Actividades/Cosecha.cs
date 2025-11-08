using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class Cosecha : EntityBaseWithLicencia, ILabor
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


        public decimal RendimientoTonHa { get; set; }
        public decimal HumedadGrano { get; set; }
        public decimal SuperficieCosechadaHa { get; set; }
        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;
    }

}
