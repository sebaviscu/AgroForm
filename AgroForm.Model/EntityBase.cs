using AgroForm.Model.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public abstract class EntityBase
    {
        public int Id { get; set; }
        [NonUpdatable]
        public DateTime? RegistrationDate { get; set; }
        [NonUpdatable]
        public string? RegistrationUser { get; set; } = string.Empty;
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; } = string.Empty;
    }

    public abstract class EntityBaseWithLicencia : EntityBase
    {
        public int? IdLicencia { get; set; }
    }

    public interface IEntityBaseWithCampania
    {
        int IdCampania { get; set; }
    }

    public interface IEntityBaseWithMoneda
    {
        int IdMoneda { get; set; }
    }

    /// <summary>
    /// Interface for entities where IdLicencia can be NULL (global records) or have a value (license-owned records).
    /// Used for multitenant entities that support both global and per-license records.
    /// </summary>
    public interface IOptionalLicenciaEntity
    {
        int? IdLicencia { get; set; }
    }

    /// <summary>
    /// Interface for side-table entities that manage visibility of global records per license.
    /// These tables control which global records are visible/hidden for a specific license.
    /// </summary>
    public interface ILicenciaVisibility
    {
        int IdLicencia { get; set; }
        int Id { get; set; }
        bool Activo { get; set; }
    }
}
