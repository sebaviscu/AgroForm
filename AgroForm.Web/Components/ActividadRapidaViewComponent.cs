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
        private readonly ILoteService _loteService;
        public ActividadRapidaViewComponent(
            ITipoActividadService tipoActividadService,
            IInsumoService insumoService,
            ILoteService loteService)
        {
            _tipoActividadService = tipoActividadService;
            _insumoService = insumoService;
            _loteService = loteService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tiposActividad = await _tipoActividadService.GetAllAsync();
            var insumos = await _insumoService.GetAllAsync();
            var lotes = await _loteService.GetAllWithDetailsAsync();

            var vm = new ActividadRapidaVM
            {
                Fecha = DateTime.Now,
                Lotes = lotes.Data?.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"[{t.Campo.Nombre.ToUpper()}] {t.Nombre}"
                }).ToList() ?? new List<SelectListItem>(),

                TiposActividad = tiposActividad.Data?.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre,
                    Group = new SelectListGroup { Name = t.Icono ?? "ph-question" }
                }).ToList() ?? new List<SelectListItem>(),

                InsumosDisponibles = insumos.Data?.OrderBy(_ => _.Nombre).Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Nombre,
                    Group = new SelectListGroup { Name = i.UnidadMedida ?? "" }
                }).ToList() ?? new List<SelectListItem>(),

            };

            return View(vm);
        }
    }
}