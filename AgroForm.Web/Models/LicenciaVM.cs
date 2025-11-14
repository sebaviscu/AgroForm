using AgroForm.Model;
using AgroForm.Web.Models;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class LicenciaVM : EntityBaseVM
    {
        public string? RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
        public TipoLicenciaEnum TipoLicencia { get; set; }

        public bool EsPrueba { get; set; }
        public DateTime? FechaFinPrueba { get; set; }
        public bool Activo { get; set; }

        public UsuarioVM? Usuario { get; set; }

        public ICollection<PagoLicenciaVM>? PagoLicencias { get; set; }
    }
}
