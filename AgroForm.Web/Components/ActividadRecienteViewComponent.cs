using AgroForm.Business.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

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
            var actividades = await _actividadService.GetAllAsync();

            return View(actividades);
        }
    }
}

