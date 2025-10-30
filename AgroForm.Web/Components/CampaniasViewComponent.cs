using AgroForm.Business.Contracts;
using AgroForm.Model;
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
        try
        {
            var campañasResponse = await _campañaService.GetAllAsync();

            if (campañasResponse.Success)
            {
                return View(campañasResponse.Data);
            }

            // Si hay error, retornar lista vacía
            return View(new List<Campania>());
        }
        catch (Exception)
        {
            // En caso de error, retornar lista vacía
            return View(new List<Campania>());
        }
    }
}