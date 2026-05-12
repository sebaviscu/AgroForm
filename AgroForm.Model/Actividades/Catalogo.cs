using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model.Actividades
{
    public class Catalogo : EntityBaseWithLicencia, IOptionalLicenciaEntity
    {
        public TipoCatalogoEnum Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        public List<LicenciasCatalogos> LicenciasCatalogos { get; set; } = new();
    }

}
