using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class Usuario : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public Roles Rol { get; set; }
        public bool Activo { get; set; } = true;
        public bool EmailConfirmed { get; set; } = true;
        public bool SuperAdmin { get; set; } = false;

        public string PasswordHash { get; set; } = string.Empty;
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}
