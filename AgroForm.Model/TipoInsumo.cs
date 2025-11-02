using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class TipoInsumo : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
        public ICollection<TipoActividad> TiposActividad { get; set; } = new List<TipoActividad>();


    }
}
