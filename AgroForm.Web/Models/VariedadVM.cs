using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class VariedadVM : EntityBaseVM
    {
        public int IdCultivo { get; set; }
        public CultivoVM Cultivo { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public TipoVariedad Tipo { get; set; } = TipoVariedad.Variedad;
        public bool Activo { get; set; } = true;
    }

}
