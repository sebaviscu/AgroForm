using AgroForm.Model.Actividades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Cultivo : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;

        public List<Variedad> Variedades { get; set; } = new();
        public List<EstadoFenologico> EstadosFenologicos { get; set; } = new();
    }

}
