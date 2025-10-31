using AgroForm.Business.Contracts;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Components
{
    public class CamposActividadesViewComponent : ViewComponent
    {

        public CamposActividadesViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}