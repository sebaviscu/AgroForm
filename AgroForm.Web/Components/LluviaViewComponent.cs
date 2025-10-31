using AgroForm.Business.Contracts;
using AgroForm.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class LluviaViewComponent : ViewComponent
{

    public LluviaViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}