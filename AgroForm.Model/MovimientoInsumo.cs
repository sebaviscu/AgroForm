using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class MovimientoInsumo : EntityBaseWithLicencia
    {
        public int ActividadId { get; set; }
        public Actividad Actividad { get; set; } = null!;

        public int InsumoId { get; set; }
        public Insumo Insumo { get; set; } = null!;

        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public int MonedaId { get; set; }
        public Moneda Moneda { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public bool EsEntrada { get; set; } // true = compra, false = uso
    }

}
