using AgroForm.Model.Actividades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Cultivo : EntityBaseWithLicencia, IOptionalLicenciaEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;
        public string? Color { get; set; } // Hex color for dashboard visualization

        public List<EstadoFenologico> EstadosFenologicos { get; set; } = new();
        public List<LicenciasCultivos> LicenciasCultivos { get; set; } = new();
    }

}
