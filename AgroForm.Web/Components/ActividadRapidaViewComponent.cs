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
            //var insumos = await _insumoService.GetAllAsync();
            var lotes = await _loteService.GetAllWithDetailsAsync();

            var analisisSuelo = tiposActividad.Data.First(_ => _.Nombre == "Analisis de suelo");
            analisisSuelo.Nombre = "AnalisisSuelo";

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
                    IconoColorTipoActividad = t.ColorIcono,
                    IdTipoInsumo = t.IdTipoInsumo?.ToString() ?? ""
                }).ToList() ?? new List<ActividadVM>()
            };

            return View(vm);
        }
    }
}