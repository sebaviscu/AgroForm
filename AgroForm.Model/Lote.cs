using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Lote : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int CampoId { get; set; }
        public Campo Campo { get; set; } = null!;

        public int CampaniaId { get; set; }
        public Campania Campania { get; set; } = null!;

        public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
        public ICollection<RegistroLluvia> RegistrosLluvia { get; set; } = new List<RegistroLluvia>();
    }
}
