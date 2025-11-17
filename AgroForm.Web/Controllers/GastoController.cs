using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;

namespace AgroForm.Web.Controllers
{
    public class GastoController : BaseController<Gasto, GastoVM, IGastoService>
    {
        public GastoController(ILogger<GastoController> logger, IMapper mapper, IGastoService service)
            : base(logger, mapper, service)
        {
        }



    }
}
