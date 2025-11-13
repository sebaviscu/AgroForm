using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Models.IndexVM
{
    public class RegistroClimaIndexVM
    {
        public List<SelectListItem> Campos { get; set; } = new();
        public List<RegistroClimaVM> Climas { get; set; } = new();
    }
}
