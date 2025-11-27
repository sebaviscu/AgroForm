using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Components
{
    public class TablaGastosViewComponent : ViewComponent
    {
        public TablaGastosViewComponent()
        {

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
