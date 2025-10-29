using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Models
{
    public class ActividadesIndexVM
    {
        public List<SelectListItem> Campos { get; set; } = new();
        public List<ActividadVM> Actividades { get; set; } = new();
        public int CampoSeleccionado { get; set; } = 0;
    }

    
}
