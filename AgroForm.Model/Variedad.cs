using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class Variedad : EntityBase
    {
        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public TipoVariedad Tipo { get; set; } = TipoVariedad.Variedad;
        public bool Activo { get; set; } = true;
    }

}
