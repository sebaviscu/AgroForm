using AgroForm.Business.Contracts;
using AgroForm.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class HistoricoPrecioInsumoController : BaseController<HistoricoPrecioInsumo, IHistoricoPrecioInsumoService>
    {
        public HistoricoPrecioInsumoController(ILogger<HistoricoPrecioInsumoController> logger, IMapper mapper, IHistoricoPrecioInsumoService service)
            : base(logger, mapper, service)
        {
        }

    }
}
