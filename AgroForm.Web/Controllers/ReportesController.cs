using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

[ApiController]
[Route("Reportes")]
[Authorize(AuthenticationSchemes = "AgroFormAuth")]

public class ReportesController : ControllerBase
{
    private readonly ICierreCampaniaService _cierreCampaniaService;
    private readonly IPdfService _pdfService;

    public ReportesController(ICierreCampaniaService cierreCampaniaService, IPdfService pdfService)
    {
        _cierreCampaniaService = cierreCampaniaService;
        _pdfService = pdfService;
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

//// Para descargar
// fetch(`/api/reportes/cierre-campania/${idCampania}/pdf`)
//     .then(response => response.blob())
//     .then(blob => {
//         const url = window.URL.createObjectURL(blob);
//         const a = document.createElement('a');
//         a.href = url;
//         a.download = `reporte_cierre_${idCampania}.pdf`;
//         a.click();
//     });

// // Para previsualizar
// window.open(`/api/reportes/cierre-campania/${idCampania}/preview`, '_blank');
