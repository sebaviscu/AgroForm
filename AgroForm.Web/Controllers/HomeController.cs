using AgroForm.Business.Contracts;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AgroForm.Web.Controllers;

[Authorize(AuthenticationSchemes = "AgroFormAuth")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMonedaService _monedaService;
    private readonly IActividadService _actividadService;
    private readonly IGastoService _gastoService;

    public HomeController(ILogger<HomeController> logger, IMonedaService monedaService, IActividadService actividadService, IGastoService gastoService)
    {
        _logger = logger;
        _monedaService = monedaService;
        _actividadService = actividadService;
        _gastoService = gastoService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeIndexVM();

        var dolar = await _monedaService.ObtenerTipoCambioActualAsync();
        vm.CotizacionDolar = dolar.TipoCambioReferencia.ToString("N0");
        //vm.CotizacionFecha = dolar.ModificationDate.HasValue ? dolar.ModificationDate.Value.ToString("dd/MM/yyyy") : "-";
        vm.CotizacionFecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        var gastosResult = await _gastoService.GetAllByCamapniaAsync();

        if (gastosResult.Success)
            vm.CargarDistribucionGastos(gastosResult.Data);

        var siembraResult = await  _actividadService.GetSiembrasAsync();

        if (siembraResult.Success)
            vm.CargarCultivosDesdeSiembras(siembraResult.Data);

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
