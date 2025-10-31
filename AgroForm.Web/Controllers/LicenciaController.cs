using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class LicenciaController : BaseController<Licencia, ILicenciaService>
    {
        public LicenciaController(ILogger<LicenciaController> logger, IMapper mapper, ILicenciaService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
