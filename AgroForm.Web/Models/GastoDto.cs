namespace AgroForm.Web.Models
{
    public class GastoDto
    {
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        /// <summary>
        /// Indica si el registro proviene de una labor (actividad) o de un gasto directo.
        /// true = labor (actividad agrícola), false = gasto directo.
        /// </summary>
        public bool EsLabor { get; set; }
    }
}
