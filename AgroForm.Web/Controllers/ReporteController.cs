using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(AuthenticationSchemes = "AgroFormAuth")]
public class ReporteController : Controller
{
    private readonly ICierreCampaniaService _cierreCampaniaService;
    private readonly ICampoService _campoService;

    public ReporteController(ICierreCampaniaService cierreCampaniaService, ICampoService campoService)
    {
        _cierreCampaniaService = cierreCampaniaService;
        _campoService = campoService;
    }

    public async Task<IActionResult> Campo()
    {
        var campos = await _campoService.GetAllAsync();

        return View();
    }

    [HttpGet("CierreCampania/{idCamapnia}")]
    public async Task<IActionResult> CierreCampania(int idCamapnia)
    {
        var gResponse = new GenericResponse<byte[]>();

        try
        {
            var resultReporteCierreCampania = await _cierreCampaniaService.GetByIdCampania(idCamapnia);

            if(!resultReporteCierreCampania.Success)
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
}
