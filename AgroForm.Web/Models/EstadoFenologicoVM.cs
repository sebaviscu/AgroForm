using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class EstadoFenologicoVM : EntityBaseVM
    {

        public int IdCultivo { get; set; }
        public CultivoVM Cultivo { get; set; } = null!;

        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }

}
