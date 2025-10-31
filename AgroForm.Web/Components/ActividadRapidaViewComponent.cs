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
        private readonly IInsumoService _insumoService;

        public ActividadRapidaViewComponent(
            ITipoActividadService tipoActividadService,
            IInsumoService insumoService)
        {
            _tipoActividadService = tipoActividadService;
            _insumoService = insumoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tiposActividad = await _tipoActividadService.GetAllAsync();
            var insumos = await _insumoService.GetAllAsync();

            var vm = new ActividadRapidaVM
            {
                Fecha = DateTime.Now,
                TiposActividad = tiposActividad.Data?.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre
                }).ToList() ?? new List<SelectListItem>(),

                InsumosDisponibles = insumos.Data?.Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Nombre
                }).ToList() ?? new List<SelectListItem>(),

                Insumo = new InsumoVM()
            };

            return View(vm);
        }
    }
}
