using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class MovimientoInsumoController : BaseController<MovimientoInsumo, IMovimientoInsumoService>
    {
        public MovimientoInsumoController(ILogger<MovimientoInsumoController> logger, IMapper mapper, IMovimientoInsumoService service)
            : base(logger, mapper, service)
        {
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
