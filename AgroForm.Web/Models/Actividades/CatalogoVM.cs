using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class CatalogoVM : EntityBaseWithLicenciaVM
    {
        public TipoCatalogoEnum Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

    }

}
