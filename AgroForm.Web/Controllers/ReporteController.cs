using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Web.Models;
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

    [HttpGet("[action]")]
    public async Task<IActionResult> ComparativaCampos()
    {
        try
        {
            // Obtener datos para los filtros
            var camposResult = await _campoService.GetAllAsync();
            var cultivosResult = await _reportService.GetComparativaCamposAsync(); // Para obtener cultivos disponibles

            var viewModel = new ReporteComparativaVM
            {
                Campos = camposResult.Success 
                    ? camposResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre }).ToList()
                    : new List<FiltroItem>(),
                Cultivos = cultivosResult.Success
                    ? cultivosResult.Data
                        .Where(d => d.IdCultivo.HasValue)
                        .Select(d => new FiltroItem { Id = d.IdCultivo!.Value, Nombre = d.Cultivo ?? "Sin nombre" })
                        .DistinctBy(c => c.Id)
                        .ToList()
                    : new List<FiltroItem>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // En caso de error, retornar vista con datos vacíos
            return View(new ReporteComparativaVM());
        }
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
    /// Obtiene las campañas disponibles para un campo (desde GetHistorialByIdAsync para obtener TODAS)
    /// </summary>
    [HttpGet("GetCampaniasByCampo/{idCampo}")]
    public async Task<IActionResult> GetCampaniasByCampo(int idCampo)
    {
        try
        {
            var campo = await _campoService.GetHistorialByIdAsync(idCampo);

            if (!campo.Success || campo.Data == null)
            {
                return NotFound(new { Success = false, Message = "Campo no encontrado" });
            }

            var campanias = campo.Data.Lotes
                .SelectMany(l => l.CicloCultivos)
                .Where(cc => cc.Campania != null)
                .Select(cc => new
                {
                    Id = cc.IdCampania,
                    Nombre = cc.Campania!.Nombre ?? "N/A"
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

    /// <summary>
    /// Obtiene los lotes de un campo específico para el filtro
    /// </summary>
    [HttpGet("GetLotesByCampo/{idCampo}")]
    public async Task<IActionResult> GetLotesByCampo(int idCampo)
    {
        try
        {
            var campo = await _campoService.GetByIdAsync(idCampo);

            if (!campo.Success || campo.Data == null)
            {
                return NotFound(new { Success = false, Message = "Campo no encontrado" });
            }

            var lotes = campo.Data.Lotes
                .Select(l => new
                {
                    Id = l.Id,
                    Nombre = l.Nombre
                })
                .OrderBy(l => l.Nombre)
                .ToList();

            return Ok(new { Success = true, Data = lotes });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Ha ocurrido un error al obtener los lotes" });
        }
    }

    /// <summary>
    /// Obtiene datos comparativos de campos/lotes para el reporte
    /// </summary>
    [HttpPost("GetComparativaCamposData")]
    public async Task<IActionResult> GetComparativaCamposData([FromBody] ReporteComparativaRequest request)
    {
        var gResponse = new GenericResponse<List<ReporteComparativaCampoDto>>();

        try
        {
            var result = await _reportService.GetComparativaCamposAsync(
                request.IdCampania,
                request.IdCampo,
                request.IdLote,
                request.IdCultivo);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = result.Data;
            gResponse.Message = "Datos comparativos obtenidos correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            gResponse.Success = false;
            gResponse.Message = "Ha ocurrido un error al obtener los datos comparativos";
            return BadRequest(gResponse);
        }
    }

    /// <summary>
    /// Obtiene la comparativa integral entre dos campos
    /// </summary>
    [HttpPost("[action]")]
    public async Task<IActionResult> GetComparativa([FromBody] ComparativaRequest request)
    {
        var gResponse = new GenericResponse<ComparativaCamposDto>();

        try
        {
            var result = await _reportService.GetComparativaCamposIntegralAsync(
                request.IdCampoPrincipal,
                request.IdCampoSecundario,
                request.IdCampania);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = result.Data;
            gResponse.Message = "Comparativa generada correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            gResponse.Success = false;
            gResponse.Message = "Ha ocurrido un error al generar la comparativa";
            return BadRequest(gResponse);
        }
    }
}
