using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class EstadoFenologicoController : BaseController<EstadoFenologico, EstadoFenologicoVM, IEstadoFenologicoService>
    {
        public EstadoFenologicoController(ILogger<EstadoFenologicoController> logger, IMapper mapper, IEstadoFenologicoService service)
            : base(logger, mapper, service)
        {
        }
    }
}
