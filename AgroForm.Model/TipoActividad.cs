using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class TipoActividad : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string ColorIcono { get; set; } = string.Empty;

        public int? IdTipoInsumo { get; set; }
        public TipoInsumo? TipoInsumo { get; set; }
    }
}
