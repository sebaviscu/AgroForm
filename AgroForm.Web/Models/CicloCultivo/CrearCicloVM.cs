using System.ComponentModel.DataAnnotations;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models.CicloCultivo
{
    public class CrearCicloVM
    {
        [Required]
        public int IdLote { get; set; }

        [Required]
        public int IdCultivo { get; set; }

        public int? IdVariedad { get; set; }

        public EpocaSiembra? Epoca { get; set; }
    }
}
