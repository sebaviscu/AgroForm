using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CampaniaController : BaseController<Campania, CampaniaVM, ICampaniaService>
    {
        private readonly ILoteService _loteService;
        private readonly ICierreCampaniaService _cierreCampaniaService;

        public CampaniaController(ILogger<CampaniaController> logger, IMapper mapper, ICampaniaService service, ILoteService loteService, ICierreCampaniaService cierreCampaniaService)
            : base(logger, mapper, service)
        {
            _loteService = loteService;
            _cierreCampaniaService = cierreCampaniaService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                return View();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
        }


        [HttpGet]
        public override async Task<IActionResult> GetAllDataTable()
        {
            var result = await _service.GetAllWithDetailsAsync();
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            var dataVM = Map<List<Campania>, List<CampaniaVM>>(result.Data);

            return Json(new
            {
                success = true,
                data = dataVM
            });
        }

        [HttpPut]
        public override async Task<IActionResult> Update([FromBody] CampaniaVM dto)
        {
            var entity = Map<CampaniaVM, Campania>(dto);
            var result = await _service.UpdateAsync(entity);

            await _loteService.CreateRangeAsync(entity.Lotes.ToList());

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<Campania, CampaniaVM>(result.Data);
            gResponse.Message = "Registro actualizado correctamente";
            return Ok(gResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Finalizar(int id)
        {
            var gResponse = new GenericResponse<int>();

            try
            {
                var reporte = await _cierreCampaniaService.GenerarReporteCierreAsync(id);

                if (!reporte.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = reporte.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Object = reporte.Data.Id;
                gResponse.Message = "Campaña finalizada";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar campaña");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GenerarPdf(int id)
        {
            var gResponse = new GenericResponse<byte[]>();

            try
            {
                var result = await _cierreCampaniaService.GenerarPdfReporteAsync(id);


                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Object = result.Data;
                gResponse.Message = "Reporte creado";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar pdf");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

        }
    }
}
