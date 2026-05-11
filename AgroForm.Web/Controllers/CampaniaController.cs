using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AgroForm.Web.Utilities;
using Mapster;
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

        public CampaniaController(ILogger<CampaniaController> logger, ICampaniaService service, ILoteService loteService, ICierreCampaniaService cierreCampaniaService)
            : base(logger, service)
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

        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] CampaniaVM dto)
        {
            // Llamar al método base para mantener la lógica estándar
            var result = await base.Create(dto);

            // Si la creación fue exitosa, verificar si es la primera campaña del usuario
            if (result is OkObjectResult okResult && okResult.Value is GenericResponse<CampaniaVM> response && response.Success)
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });
                if (user.IdCampaña == null)
                {
                    await UpdateClaimAsync("Campania", response.Object.Id.ToString());
                    response.Message = "Registro creado correctamente. Se ha asignado la campaña actual.";
                }
            }

            return result;
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
                ValidarAutorizacion(new[] { Roles.Administrador });

                // Primero finalizar la campaña (cierra ciclos activos y cambia estado)
                var resultadoFinalizacion = await _service.FinalizarCampaña(id);
                if (!resultadoFinalizacion.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = resultadoFinalizacion.ErrorMessage;
                    return BadRequest(gResponse);
                }

                // Generar reporte de cierre
                var reporte = await _cierreCampaniaService.GenerarReporteCierreAsync(id);
                if (!reporte.Success)
                {
                    _logger.LogWarning("No se pudo generar el reporte de cierre para la campaña {Id}: {Error}", id, reporte.ErrorMessage);
                    // Continuamos aunque falle el reporte, la campaña ya está finalizada
                }

                // Limpiar el claim de idCampania del usuario
                await UpdateClaimAsync("Campania", null);

                gResponse.Success = true;
                gResponse.Message = "Campaña finalizada correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar campaña");
                gResponse.Success = false;
                gResponse.Message = "Ha ocurrido un error al cerrar la campaña";
                return BadRequest(gResponse);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Iniciar(int id)
        {
            var gResponse = new GenericResponse<CampaniaVM>();
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = await _service.IniciarCampania(id);
                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                // Actualizar claim del usuario a la nueva campaña activa
                await UpdateClaimAsync("Campania", id.ToString());

                gResponse.Success = true;
                gResponse.Object = Map<Campania, CampaniaVM>(result.Data);
                gResponse.Message = "Campaña iniciada correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar campaña {Id}", id);
                gResponse.Success = false;
                gResponse.Message = "Ocurrió un error al iniciar la campaña";
                return BadRequest(gResponse);
            }
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GenerarPdf(int id)
        //{
        //    var gResponse = new GenericResponse<byte[]>();

        //    try
        //    {
        //        var result = await _cierreCampaniaService.GenerarPdfReporteAsync(id);


        //        if (!result.Success)
        //        {
        //            gResponse.Success = false;
        //            gResponse.Message = result.ErrorMessage;
        //            return BadRequest(gResponse);
        //        }

        //        gResponse.Success = true;
        //        gResponse.Object = result.Data;
        //        gResponse.Message = "Reporte creado";
        //        return Ok(gResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al generar pdf");
        //        gResponse.Success = false;
        //        gResponse.Message = "Ah ocurrido un error";
        //        return BadRequest(gResponse);
        //    }

        //}

        [HttpPost("{id}")]
        public async Task<IActionResult> SetCurrent(int id)
        {
            var gResponse = new GenericResponse<bool>();

            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (user.IdCampaña != id)
                {
                    await UpdateClaimAsync("Campania", id.ToString());
                    gResponse.Message = "Cambio de Camapaña";
                }
                else
                    gResponse.Message = string.Empty;

                gResponse.Success = true;
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar de campaña");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

        }
    }
}
