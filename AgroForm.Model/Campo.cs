using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Campo : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public decimal? SuperficieHectareas { get; set; }

        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
