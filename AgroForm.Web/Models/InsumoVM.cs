using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class InsumoVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? UnidadMedida { get; set; }
        public decimal? StockActual { get; set; }
        public decimal? StockMinimo { get; set; }

        public int? idMarca { get; set; }

        public int? idProveedor { get; set; }

        public int? IdTipoInsumo { get; set; }

        public bool Estado { get; set; } = true;
    }
}
