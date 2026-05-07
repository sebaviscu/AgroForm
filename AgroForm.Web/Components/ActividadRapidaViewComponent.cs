using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using Mapster;
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
            var claimUser = HttpContext.User;
            var idCampania = UtilidadService.GetClaimValue<int>(claimUser, "Campania");

            var tiposActividad = await _tipoActividadService.GetAllByCamapniaAsync();
            var lotes = await _loteService.GetAllWithDetailsAsync();

            var lotesVM = lotes.Data.Adapt<List<LoteVM>>();

            var vm = new ActividadRapidaVM
            {
                Fecha = TimeHelper.GetArgentinaTime(),
                Lotes = lotesVM,
                TiposActividadCompletos = tiposActividad.Data?.Select(t => new ActividadVM
                {
                    Id = t.Id,
                    TipoActividad = t.Nombre,
                    IdTipoActividad = t.Id,
                    IconoTipoActividad = t.Icono,
                    IconoColorTipoActividad = t.ColorIcono
                }).ToList() ?? new List<ActividadVM>()
            };

            return View(vm);
        }
    }
}