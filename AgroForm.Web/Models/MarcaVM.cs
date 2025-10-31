using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class MarcaVM : EntityBaseWithLicenciaVM
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
