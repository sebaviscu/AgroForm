using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AgroForm.Web.Models.IndexVM
{
    public class ActividadRapidaVM
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; } = DateTime.Now;
        public List<int> LotesIds { get; set; } = new List<int>();
        public List<SelectListItem>? Lotes { get; set; }
        public decimal? Costo { get; set; }

        [Required(ErrorMessage = "El tipo de actividad es requerido")]
        [Display(Name = "Tipo de Actividad")]
        public int TipoidActividad { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500)]
        public string? Observacion { get; set; }

        public decimal? Cantidad { get; set; }
        public int? idInsumo { get; set; }

        // Para llenar los dropdowns
        public List<SelectListItem>? TiposActividad { get; set; }
        public List<SelectListItem>? InsumosDisponibles { get; set; }
    }
}
