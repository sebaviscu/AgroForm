using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CultivoController : BaseController<Cultivo, CultivoVM, ICultivoService>
    {
        public CultivoController(ILogger<CultivoController> logger, IMapper mapper, ICultivoService service)
            : base(logger, mapper, service)
        {
        }

    }
}
