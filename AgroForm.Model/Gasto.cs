using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class Gasto : EntityBaseWithLicencia
    {
        public TipoGastoEnum TipoGasto { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public DateOnly Fecha { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        public int IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        public int CampaniaId { get; set; }
        public Campania? Campania { get; set; }

    }
}
