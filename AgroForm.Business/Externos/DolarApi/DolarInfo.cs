using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Externos.DolarApi
{
    public class DolarInfo
    {
        public string Moneda { get; set; } = string.Empty;
        public string Casa { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
