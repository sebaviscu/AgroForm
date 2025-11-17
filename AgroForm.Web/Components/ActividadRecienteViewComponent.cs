using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model.Configuracion;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Components
{
    public class ActividadRecienteViewComponent : ViewComponent
    {
        public ActividadRecienteViewComponent()
        {

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}

