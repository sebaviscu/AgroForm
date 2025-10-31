using AgroForm.Business.Contracts;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class InsumosViewComponent : ViewComponent
{

    public InsumosViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}