using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace AgroForm.Web.Components
{
    public class ActividadRapidaViewComponent : ViewComponent
    {
        private readonly ITipoActividadService _tipoActividadService;
        private readonly ILoteService _loteService;
        private readonly ICicloCultivoService _cicloCultivoService;
        private readonly IUnidadMedidaService _unidadMedidaService;

        public ActividadRapidaViewComponent(
            ITipoActividadService tipoActividadService,
            ILoteService loteService,
            ICicloCultivoService cicloCultivoService,
            IUnidadMedidaService unidadMedidaService)
        {
            _tipoActividadService = tipoActividadService;
            _loteService = loteService;
            _cicloCultivoService = cicloCultivoService;
            _unidadMedidaService = unidadMedidaService;
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

            // Cargar configuraciones de unidades de medida para todos los tipos de actividad
            if (vm.TiposActividadCompletos != null && vm.TiposActividadCompletos.Any())
            {
                var allConfigs = new Dictionary<int, List<CampoUnidadConfigDto>>();
                foreach (var tipo in vm.TiposActividadCompletos)
                {
                    var config = await _unidadMedidaService.GetConfigUnidadesAsync(tipo.IdTipoActividad);
                    allConfigs[tipo.IdTipoActividad] = config;
                }
                vm.UnidadesConfigJson = JsonSerializer.Serialize(allConfigs, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            return View(vm);
        }
    }
}