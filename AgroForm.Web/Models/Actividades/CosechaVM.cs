using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class CosechaVM : ActividadVM
    {
        public decimal RendimientoTonHa { get; set; }
        public decimal HumedadGrano { get; set; }
        public decimal SuperficieCosechadaHa { get; set; }
        public int IdCultivo { get; set; }
        public CultivoVM Cultivo { get; set; } = null!;
    }

}
