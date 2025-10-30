using AgroForm.Business.Contracts;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MenuLateralViewComponent : ViewComponent
{
    public MenuLateralViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}