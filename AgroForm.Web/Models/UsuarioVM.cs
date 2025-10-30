using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class UsuarioVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public Roles Rol { get; set; }
        public bool Activo { get; set; } = true;
        public bool EmailConfirmed { get; set; } = true;

        public string PasswordHash { get; set; } = string.Empty;
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}
