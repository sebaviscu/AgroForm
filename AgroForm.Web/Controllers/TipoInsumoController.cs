using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class TipoInsumoController : BaseController<TipoInsumo, ITipoInsumoService>
    {
        public TipoInsumoController(ILogger<TipoInsumoController> logger, IMapper mapper, ITipoInsumoService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
