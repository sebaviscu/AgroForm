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
        private readonly IActividadService _actividadService;
        public ActividadRecienteViewComponent(IActividadService actividadService)
        {
            _actividadService = actividadService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}

