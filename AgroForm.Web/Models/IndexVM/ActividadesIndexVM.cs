using AgroForm.Model.Actividades;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Models.IndexVM
{
    public class ActividadesIndexVM
    {
        public List<SelectListItem> Campos { get; set; } = new();
        public List<LaborDTO> Actividades { get; set; } = new();
        public int CampoSeleccionado { get; set; } = 0;
    }

    
}
