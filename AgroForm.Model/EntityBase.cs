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
        public string? RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }

    public abstract class EntityBaseWithLicencia : EntityBase
    {
        public int IdLicencia { get; set; }
    }

    public interface IEntityBaseWithCampania
    {
        int IdCampania { get; set; }
    }

    public interface IEntityBaseWithMoneda
    {
        int IdMoneda { get; set; }
    }
}
