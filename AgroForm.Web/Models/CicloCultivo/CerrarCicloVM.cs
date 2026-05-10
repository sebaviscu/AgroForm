using System.ComponentModel.DataAnnotations;

namespace AgroForm.Web.Models.CicloCultivo
{
    public class CerrarCicloVM
    {
        [Required]
        public int IdCicloCultivo { get; set; }
    }
}
