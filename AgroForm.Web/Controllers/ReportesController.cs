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
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ICierreCampaniaService _cierreCampaniaService;
    private readonly IPdfService _pdfService;

    public ReportesController(ICierreCampaniaService cierreCampaniaService, IPdfService pdfService)
    {
        _cierreCampaniaService = cierreCampaniaService;
        _pdfService = pdfService;
    }

    [HttpGet("cierre-campania/{idCampania}/pdf")]
    public async Task<IActionResult> GenerarPdfCierreCampania(int idCampania)
    {
        try
        {
            var pdfBytes = await _cierreCampaniaService.GenerarPdfReporteAsync(idCampania);
            
            return File(pdfBytes, "application/pdf", 
                $"Reporte_Cierre_Campania_{idCampania}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al generar PDF: {ex.Message}");
        }
    }

    [HttpGet("cierre-campania/{idCampania}/preview")]
    public async Task<IActionResult> PreviewPdfCierreCampania(int idCampania)
    {
        try
        {
            var pdfBytes = await _cierreCampaniaService.GenerarPdfReporteAsync(idCampania);
            
            return File(pdfBytes, "application/pdf");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al generar PDF: {ex.Message}");
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
