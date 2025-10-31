using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class RegistroClimaController : BaseController<RegistroClima, IRegistroClimaService>
    {
        public RegistroClimaController(ILogger<RegistroClimaController> logger, IMapper mapper, IRegistroClimaService service)
            : base(logger, mapper, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
