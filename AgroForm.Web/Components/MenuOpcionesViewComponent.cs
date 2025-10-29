using AgroForm.Business.Contracts;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MenuOpcionesViewComponent : ViewComponent
{
    public MenuOpcionesViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}