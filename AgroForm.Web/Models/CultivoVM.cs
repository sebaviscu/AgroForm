using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class CultivoVM : EntityBaseVM
    {
        public string Nombre { get; set; } = string.Empty;
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;

        public List<VariedadVM> Variedades { get; set; } = new();
        public List<EstadoFenologicoVM> EstadosFenologicos { get; set; } = new();
    }

}
