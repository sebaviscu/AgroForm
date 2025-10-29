using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model.Configuracion
{
    public class UserAuth
    {
        public int IdUsuario { get; set; }
        public int IdLicencia { get; set; }
        public Roles IdRol { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool Result { get; set; }
    }
}
