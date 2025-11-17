using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models.IndexVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CampaniasViewComponent : ViewComponent
{
    private readonly ICampaniaService _campañaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CampaniasViewComponent(ICampaniaService campañaService, IHttpContextAccessor httpContextAccessor)
    {
        _campañaService = campañaService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var vm = new SelectorCampaniasVM();
        try
        {
            var campañasResponse = await _campañaService.GetAllAsync();

            if (campañasResponse.Success)
            {
                var claimUser = HttpContext.User;
                vm.IdCampaniaSeleccionada = UtilidadService.GetClaimValue<int>(claimUser, "Campania");
                vm.Campanias = campañasResponse.Data.Where(_ =>
                                    _.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso
                                    || _.EstadosCampania == EnumClass.EstadosCamapaña.Iniciada).ToList();
                return View(vm);
            }

            return View(vm);
        }
        catch (Exception)
        {
            return View(vm);
        }
    }
}