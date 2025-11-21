using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class GastoVM : EntityBaseVM
    {
        public TipoGastoEnum TipoGasto { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public DateOnly Fecha { get; set; }
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        public int IdMoneda { get; set; }

        public int IdCampania { get; set; }

        public string TipoGastoString => TipoGasto.ToString();
        public bool EsDolar { get; set; }
        public bool EsDolarEdit => IdMoneda == (int)Monedas.DolarOficial;

    }
}
