using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Models.CicloCultivo
{
    public class CicloCultivoIndexVM
    {
        public List<CicloCultivoVM> Ciclos { get; set; } = new();
        public List<SelectListItem> Lotes { get; set; } = new();
        public List<SelectListItem> Cultivos { get; set; } = new();
    }
}
