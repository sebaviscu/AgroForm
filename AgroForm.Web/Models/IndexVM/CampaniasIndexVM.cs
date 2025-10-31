using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Models.IndexVM
{
    public class CampaniasIndexVM
    {
        public List<CampaniaVM> Campanias { get; set; } = new List<CampaniaVM>();
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
    }
}
