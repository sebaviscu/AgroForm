using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class CultivoVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;
        public string? Color { get; set; }

        public List<EstadoFenologicoVM> EstadosFenologicos { get; set; } = new();
    }

}
