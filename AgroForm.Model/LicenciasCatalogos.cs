using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    /// <summary>
    /// Side table that controls visibility of global Catalogo records per license.
    /// When a record exists with Activo = false, the global catalog item is hidden for that license.
    /// Absence of a record means the global catalog item is visible by default.
    /// </summary>
    public class LicenciasCatalogos : ILicenciaVisibility
    {
        public int Id { get; set; }
        public int IdLicencia { get; set; }
        public int IdCatalogo { get; set; }
        public bool Activo { get; set; } = true;
        public int? Orden { get; set; }

        [NonUpdatable]
        public DateTime? RegistrationDate { get; set; }
        [NonUpdatable]
        public string? RegistrationUser { get; set; } = string.Empty;
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; } = string.Empty;

        public Licencia Licencia { get; set; } = null!;
        public Catalogo Catalogo { get; set; } = null!;
    }
}
