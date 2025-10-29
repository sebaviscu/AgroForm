using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class RegistroLluvia : EntityBaseWithLicencia
    {
        public DateTime Fecha { get; set; }
        public decimal Milimetros { get; set; }

        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;
    }
}
