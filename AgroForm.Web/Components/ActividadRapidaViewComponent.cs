using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroForm.Web.Components
{
    public class ActividadRapidaViewComponent : ViewComponent
    {
        private readonly ITipoActividadService _tipoActividadService;
        private readonly ILoteService _loteService;
        private readonly IMapper _mapper;

        public ActividadRapidaViewComponent(
            ITipoActividadService tipoActividadService,
            ILoteService loteService,
            IMapper mapper)
        {
            _tipoActividadService = tipoActividadService;
            _loteService = loteService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimUser = HttpContext.User;
            var idCampania = UtilidadService.GetClaimValue<int>(claimUser, "Campania");

            var tiposActividad = await _tipoActividadService.GetAllByCamapniaAsync();
            var lotes = await _loteService.GetAllWithDetailsAsync();

            var lotesVM = _mapper.Map<List<LoteVM>>(lotes.Data);

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