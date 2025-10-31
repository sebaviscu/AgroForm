using AgroForm.Model;
using AgroForm.Model.Configuracion;

namespace AgroForm.Web.Models
{
    public abstract class EntityBaseVM
    {
        public int Id { get; set; }
        [NonUpdatable]
        public DateTime? RegistrationDate { get; set; }
        [NonUpdatable]
        public string? RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }

    public abstract class EntityBaseWithLicenciaVM : EntityBaseVM
    {
        public int IdLicencia { get; set; }
    }
}

