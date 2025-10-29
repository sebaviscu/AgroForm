using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class Campania : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public EstadosCamapaña EstadosCamapaña { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
