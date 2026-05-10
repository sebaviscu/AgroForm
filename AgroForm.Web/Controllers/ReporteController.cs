using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(AuthenticationSchemes = "AgroFormAuth")]
[Route("[controller]")]
public class ReporteController : Controller
{
    private readonly ICierreCampaniaService _cierreCampaniaService;
    private readonly ICampoService _campoService;
    private readonly IReportService _reportService;

    public ReporteController(
        ICierreCampaniaService cierreCampaniaService,
        ICampoService campoService,
        IReportService reportService)
    {
        _cierreCampaniaService = cierreCampaniaService;
        _campoService = campoService;
        _reportService = reportService;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Campo()
    {
        var campos = await _campoService.GetAllAsync();
        return View();
    }

    [HttpGet("CierreCampania/{idCampania}")]
    public async Task<IActionResult> CierreCampania(int idCampania)
    {
        var gResponse = new GenericResponse<byte[]>();

        try
        {
            var resultReporteCierreCampania = await _cierreCampaniaService.GetByIdCampania(idCampania);

            if (!resultReporteCierreCampania.Success)
            {
                gResponse.Success = false;
                gResponse.Message = resultReporteCierreCampania.ErrorMessage;
                return BadRequest(gResponse);
            }

            var result = await _cierreCampaniaService.GenerarPdfReporteAsync(resultReporteCierreCampania.Data);

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
            gResponse.Success = false;
            gResponse.Message = "Ah ocurrido un error";
            return BadRequest(gResponse);
        }

    }

    /// <summary>
    /// Obtiene el reporte integral de un campo con todas las secciones
    /// </summary>
    [HttpPost("[action]")]
    public async Task<IActionResult> GetReporteCampoIntegral([FromBody] ReporteCampoRequest request)
    {
        var gResponse = new GenericResponse<ReporteCampoIntegralDto>();

        try
        {
            var result = await _reportService.GetReporteCampoIntegralAsync(request.IdCampo, request.IdCampania);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = result.Data;
            gResponse.Message = "Reporte generado correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            gResponse.Success = false;
            gResponse.Message = "Ha ocurrido un error al generar el reporte";
            return BadRequest(gResponse);
        }
    }

    /// <summary>
    /// Obtiene las campañas disponibles para un campo
    /// </summary>
    [HttpGet("GetCampaniasByCampo/{idCampo}")]
    public async Task<IActionResult> GetCampaniasByCampo(int idCampo)
    {
        try
        {
            var campo = await _campoService.GetByIdWithDetailsAsync(idCampo);

            if (!campo.Success || campo.Data == null)
            {
                return NotFound(new { Success = false, Message = "Campo no encontrado" });
            }

            var campanias = campo.Data.Lotes
                .SelectMany(l => l.CicloCultivos)
                .Select(cc => new
                {
                    Id = cc.IdCampania,
                    Nombre = cc.Campania?.Nombre ?? "N/A"
                })
                .Distinct()
                .OrderByDescending(c => c.Nombre)
                .ToList();

            return Ok(new { Success = true, Data = campanias });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Ha ocurrido un error al obtener las campañas" });
        }
    }
}
