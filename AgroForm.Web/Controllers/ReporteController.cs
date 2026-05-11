using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

[Authorize(AuthenticationSchemes = "AgroFormAuth")]
[Route("[controller]")]
public class ReporteController : Controller
{
    private readonly ICierreCampaniaService _cierreCampaniaService;
    private readonly ICampoService _campoService;
    private readonly IReportService _reportService;
    private readonly ICampaniaService _campaniaService;
    private readonly ICultivoService _cultivoService;
    private readonly ILoteService _loteService;
    private readonly ICatalogoService _catalogoService;

    public ReporteController(
        ICierreCampaniaService cierreCampaniaService,
        ICampoService campoService,
        IReportService reportService,
        ICampaniaService campaniaService,
        ICultivoService cultivoService,
        ILoteService loteService,
        ICatalogoService catalogoService)
    {
        _cierreCampaniaService = cierreCampaniaService;
        _campoService = campoService;
        _reportService = reportService;
        _campaniaService = campaniaService;
        _cultivoService = cultivoService;
        _loteService = loteService;
        _catalogoService = catalogoService;
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

    // ============================================================
    // Reporte de Rendimiento de Cosecha
    // ============================================================

    /// <summary>
    /// Vista principal del reporte de rendimiento de cosecha
    /// </summary>
    [HttpGet("[action]")]
    public async Task<IActionResult> Cosecha()
    {
        try
        {
            // Obtener datos para los filtros
            var camposResult = await _campoService.GetAllAsync();
            var campaniasResult = await _campaniaService.GetAllAsync();
            var cultivosResult = await _cultivoService.GetAllAsync();

            var viewModel = new RendimientoCosechaVM
            {
                Campos = camposResult.Success
                    ? camposResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre }).ToList()
                    : new List<FiltroItem>(),
                Campanias = campaniasResult.Success
                    ? campaniasResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre ?? "N/A" }).ToList()
                    : new List<FiltroItem>(),
                Cultivos = cultivosResult.Success
                    ? cultivosResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre ?? "Sin nombre" }).ToList()
                    : new List<FiltroItem>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // En caso de error, retornar vista con datos vacíos
            return View(new RendimientoCosechaVM());
        }
    }

    /// <summary>
    /// Obtiene los datos del reporte de rendimiento de cosecha vía AJAX
    /// </summary>
    [HttpPost("GetRendimientoCosechaData")]
    public async Task<IActionResult> GetRendimientoCosechaData([FromBody] RendimientoCosechaRequest request)
    {
        var gResponse = new GenericResponse<RendimientoCosechaReporteDto>();

        try
        {
            var result = await _reportService.GetRendimientoCosechaAsync(
                request.IdCampania,
                request.IdCampo,
                request.IdLote,
                request.IdCultivo,
                request.FechaDesde,
                request.FechaHasta,
                request.OrdenarPor,
                request.OrdenDireccion,
                request.Pagina,
                request.TamanoPagina);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = result.Data;
            gResponse.Message = "Reporte de rendimiento generado correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            gResponse.Success = false;
            gResponse.Message = "Ha ocurrido un error al generar el reporte de rendimiento";
            return BadRequest(gResponse);
        }
    }

    // ============================================================
    // Reporte de Aplicaciones (Pulverización + Fertilización)
    // ============================================================

    /// <summary>
    /// Vista principal del reporte de aplicaciones agrícolas
    /// </summary>
    [HttpGet("[action]")]
    public async Task<IActionResult> Aplicaciones()
    {
        try
        {
            var camposResult = await _campoService.GetAllAsync();
            var campaniasResult = await _campaniaService.GetAllAsync();
            var cultivosResult = await _cultivoService.GetAllAsync();

            // Obtener productos agroquímicos del catálogo
            var catalogoResult = await _catalogoService.GetAllAsync();
            var productos = catalogoResult.Success
                ? catalogoResult.Data.Where(c => c.Tipo == TipoCatalogoEnum.ProductoAgroquimico && c.Activo).ToList()
                : new List<Catalogo>();

            // Obtener nutrientes del catálogo
            var nutrientes = catalogoResult.Success
                ? catalogoResult.Data.Where(c => c.Tipo == TipoCatalogoEnum.Nutriente && c.Activo).ToList()
                : new List<Catalogo>();

            // Obtener tipos de fertilizante
            var tiposFertilizante = catalogoResult.Success
                ? catalogoResult.Data.Where(c => c.Tipo == TipoCatalogoEnum.TipoFertilizante && c.Activo).ToList()
                : new List<Catalogo>();

            // Combinar productos (agroquímicos + nutrientes + fertilizantes) para el filtro
            var todosProductos = new List<FiltroItem>();
            todosProductos.AddRange(productos.Select(p => new FiltroItem { Id = p.Id, Nombre = p.Nombre }));
            todosProductos.AddRange(nutrientes.Select(n => new FiltroItem { Id = n.Id, Nombre = n.Nombre }));
            todosProductos.AddRange(tiposFertilizante.Select(t => new FiltroItem { Id = t.Id, Nombre = t.Nombre }));
            todosProductos = todosProductos.OrderBy(p => p.Nombre).ToList();

            var viewModel = new AplicacionesVM
            {
                Campos = camposResult.Success
                    ? camposResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre }).ToList()
                    : new List<FiltroItem>(),
                Campanias = campaniasResult.Success
                    ? campaniasResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre ?? "N/A" }).ToList()
                    : new List<FiltroItem>(),
                Cultivos = cultivosResult.Success
                    ? cultivosResult.Data.Select(c => new FiltroItem { Id = c.Id, Nombre = c.Nombre ?? "Sin nombre" }).ToList()
                    : new List<FiltroItem>(),
                Productos = todosProductos
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            return View(new AplicacionesVM());
        }
    }

    /// <summary>
    /// Obtiene los datos del reporte de aplicaciones vía AJAX
    /// </summary>
    [HttpPost("GetAplicacionesData")]
    public async Task<IActionResult> GetAplicacionesData([FromBody] AplicacionRequest request)
    {
        var gResponse = new GenericResponse<AplicacionReporteDto>();

        try
        {
            var result = await _reportService.GetAplicacionesAsync(
                request.IdCampania,
                request.IdCampo,
                request.IdLote,
                request.IdCultivo,
                request.IdTipoAplicacion,
                request.IdProducto,
                request.FechaDesde,
                request.FechaHasta,
                request.OrdenarPor,
                request.OrdenDireccion,
                request.Pagina,
                request.TamanoPagina);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = result.Data;
            gResponse.Message = "Reporte de aplicaciones generado correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            gResponse.Success = false;
            gResponse.Message = "Ha ocurrido un error al generar el reporte de aplicaciones";
            return BadRequest(gResponse);
        }
    }
}
