using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class InsumoController : BaseController<Insumo, IInsumoService>
    {
        public InsumoController(ILogger<InsumoController> logger, IMapper mapper, IInsumoService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
