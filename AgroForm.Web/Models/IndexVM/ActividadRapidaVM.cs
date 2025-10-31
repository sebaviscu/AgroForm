using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AgroForm.Web.Models.IndexVM
{
    public class ActividadRapidaVM
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de actividad es requerido")]
        [Display(Name = "Tipo de Actividad")]
        public int TipoActividadId { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500)]
        public string? Observacion { get; set; }

        // Lista de insumos opcionales
        public InsumoVM? Insumo { get; set; }
        public decimal? Cantidad { get; set; }

        // Para llenar los dropdowns
        public List<SelectListItem>? TiposActividad { get; set; }
        public List<SelectListItem>? InsumosDisponibles { get; set; }
    }
}
