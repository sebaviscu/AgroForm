using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Actividad : EntityBaseWithLicencia
    {
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = string.Empty;

        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;

        public int TipoActividadId { get; set; }
        public TipoActividad TipoActividad { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public MovimientoInsumo? MovimientoInsumo { get; set; }

    }
}
