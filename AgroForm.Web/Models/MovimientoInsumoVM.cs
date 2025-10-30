using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class MovimientoInsumoVM : EntityBaseWithLicenciaVM
    {
        public int ActividadId { get; set; }

        public int InsumoId { get; set; }

        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public int MonedaId { get; set; }

        public int UsuarioId { get; set; }

        public bool EsEntrada { get; set; }
    }
}
