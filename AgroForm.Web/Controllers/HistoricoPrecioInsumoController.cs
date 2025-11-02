using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class HistoricoPrecioInsumoController : BaseController<HistoricoPrecioInsumo, HistoricoPrecioInsumoVM, IHistoricoPrecioInsumoService>
    {
        public HistoricoPrecioInsumoController(ILogger<HistoricoPrecioInsumoController> logger, IMapper mapper, IHistoricoPrecioInsumoService service)
            : base(logger, mapper, service)
        {
        }

    }
}
