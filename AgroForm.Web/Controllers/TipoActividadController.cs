using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class TipoActividadController : BaseController<TipoActividad, ITipoActividadService>
    {
        public TipoActividadController(ILogger<TipoActividadController> logger, IMapper mapper, ITipoActividadService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
