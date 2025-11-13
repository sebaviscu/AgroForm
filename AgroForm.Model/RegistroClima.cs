using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class RegistroClima : EntityBaseWithLicencia
    {
        public DateTime Fecha { get; set; }
        public decimal Milimetros { get; set; }
        public TipoClima TipoClima { get; set; }
        public string? Observaciones { get; set; }
        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        
        //public int IdCampo { get; set; }
        //public Campo Campo { get; set; } = null!;
    }
}
