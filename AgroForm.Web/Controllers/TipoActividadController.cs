using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class TipoActividadController : BaseController<TipoActividad, TipoActividadVM, ITipoActividadService>
    {
        public TipoActividadController(ILogger<TipoActividadController> logger, ITipoActividadService service)
            : base(logger, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
