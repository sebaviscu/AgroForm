using AgroForm.Business.Contracts;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Components
{
    public class ActividadRapidaViewComponent : ViewComponent
    {
        private readonly ITipoActividadService _tipoActividadService;
        private readonly ILoteService _loteService;
        public ActividadRapidaViewComponent(
            ITipoActividadService tipoActividadService,
            ILoteService loteService)
        {
            _tipoActividadService = tipoActividadService;
            _loteService = loteService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tiposActividad = await _tipoActividadService.GetAllAsync();
            var lotes = await _loteService.GetAllWithDetailsAsync();

            var vm = new ActividadRapidaVM
            {
                Fecha = DateTime.Now,
                Lotes = lotes.Data?.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"[{t.Campo.Nombre.ToUpper()}] {t.Nombre}"
                }).ToList() ?? new List<SelectListItem>(),

               
                TiposActividadCompletos = tiposActividad.Data?.Select(t => new ActividadVM
                {
                    Id = t.Id,
                    TipoActividad = t.Nombre,
                    IconoTipoActividad = t.Icono,
                    IconoColorTipoActividad = t.ColorIcono
                }).ToList() ?? new List<ActividadVM>()
            };

            return View(vm);
        }
    }
}