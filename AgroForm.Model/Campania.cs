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
        public EstadosCamapaña EstadosCampania { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
        public ICollection<RegistroClima> RegistrosClima { get; set; } = new List<RegistroClima>();
        public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
        public ReporteCierreCampania? ReporteCierreCampania { get; set; }
    }
}
