using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model
{
    public class Licencia : EntityBase
    {
        public string? RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
        public TipoLicenciaEnum TipoLicencia { get; set; }
        public bool Activo { get; set; }

        public bool EsPrueba { get; set; }
        public DateTime? FechaFinPrueba { get; set; }

        public ICollection<PagoLicencia>? PagoLicencias { get; set; }
    }

    public class PagoLicencia : EntityBaseWithLicencia
    {
        public Licencia? Licencia { get; set; }

        public TipoPagoLicenciaEnum TipoPagoLicencia { get; set; }
        public decimal Precio { get; set; }
        public DateTime Fecha {  get; set; }
    }
    
}
