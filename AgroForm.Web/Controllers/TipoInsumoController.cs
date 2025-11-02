using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class TipoInsumoController : BaseController<TipoInsumo, TipoInsumoVM, ITipoInsumoService>
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
