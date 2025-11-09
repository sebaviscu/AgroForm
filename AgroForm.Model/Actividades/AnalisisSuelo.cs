using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class AnalisisSuelo : EntityBaseWithLicencia, ILabor
    {
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        public decimal? ProfundidadCm { get; set; }
        public decimal? PH { get; set; }
        public decimal? MateriaOrganica { get; set; }
        public decimal? Nitrogeno { get; set; }
        public decimal? Fosforo { get; set; }
        public decimal? Potasio { get; set; }
        public decimal? ConductividadElectrica { get; set; }
        public decimal? CIC { get; set; } // Capacidad de intercambio catiónico
        public string Textura { get; set; } = string.Empty;

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

        public int IdMoneda { get; set; }
        public Moneda Moneda { get; set; } = null!;

        public int? IdLaboratorio { get; set; }
        public Catalogo? Laboratorio { get; set; }
    }

}
