using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class LicenciaConUsuarioVM
    {
        // Propiedades de Licencia
        public string? RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
        public TipoLicenciaEnum TipoLicencia { get; set; }
        public bool EsPrueba { get; set; }
        public DateTime? FechaFinPrueba { get; set; }
        public bool Activo { get; set; }

        // Propiedades de Usuario (solo para creación)
        public UsuarioCreacionVM? Usuario { get; set; }
    }

    // VM para creación de usuario con repetición de contraseña
    public class UsuarioCreacionVM
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? RepetirPassword { get; set; }
        public string? PhoneNumber { get; set; }
        public int Rol { get; set; }
        public bool Activo { get; set; }
    }
}
