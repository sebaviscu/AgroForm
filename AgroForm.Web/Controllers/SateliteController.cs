using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    /// <summary>
    /// Controlador de proxy de tiles satelitales y consultas analíticas.
    /// 
    /// Tiles (FASE 0):
    /// - Sirve como intermediario entre el frontend Leaflet y Sentinel Hub
    /// - Frontend solicita tiles a este endpoint (autenticado)
    /// - Backend verifica caché en disco, proxy a Sentinel Hub WMTS si no está cacheado
    /// - Cachea en disco con TTL 7 días
    /// 
    /// Datos analíticos (FASE 1):
    /// - Endpoints para consultar índices satelitales persistidos en SQL
    /// - Endpoint para scheduler n8n (lotes pendientes de actualización)
    /// - Endpoint para actualización bajo demanda
    /// 
    /// Seguridad: toda comunicación con Sentinel Hub es server-side.
    /// Las credenciales (instanceId, client secret) NUNCA se exponen al frontend.
    /// </summary>
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    [Route("satelite")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SateliteController : Controller
    {
        private readonly ISateliteService _sateliteService;
        private readonly ILogger<SateliteController> _logger;

        public SateliteController(ISateliteService sateliteService, ILogger<SateliteController> logger)
        {
            _sateliteService = sateliteService;
            _logger = logger;
        }

        // ============================================================
        // TILES WMTS (FASE 0)
        // ============================================================

        /// <summary>
        /// Obtiene un tile de mapa satelital para un índice y fecha específicos.
        /// Los tiles se cachean en disco y se sirven con Cache-Control: public, max-age=604800 (7 días).
        /// </summary>
        /// <param name="z">Zoom level (0-19)</param>
        /// <param name="x">Tile X coordinate</param>
        /// <param name="y">Tile Y coordinate</param>
        /// <param name="indice">Índice: "NDVI" o "NDWI"</param>
        /// <param name="fecha">Fecha en formato YYYY-MM-DD (explícita, obligatoria)</param>
        [HttpGet("tiles/{z}/{x}/{y}.png")]
        [ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["indice", "fecha"])]
        public async Task<IActionResult> GetTile(
            int z,
            int x,
            int y,
            [FromQuery] string indice,
            [FromQuery] string fecha)
        {
            // Validar fecha explícita
            if (string.IsNullOrWhiteSpace(fecha) || !DateOnly.TryParseExact(fecha, "yyyy-MM-dd", out _))
            {
                return BadRequest(new { error = "El parámetro 'fecha' es obligatorio y debe ser YYYY-MM-DD explícito." });
            }

            // Validar índice
            var indicesValidos = new[] { "NDVI", "NDWI" };
            if (string.IsNullOrWhiteSpace(indice) || !indicesValidos.Contains(indice.ToUpperInvariant()))
            {
                return BadRequest(new { error = $"Índice inválido. Valores permitidos: {string.Join(", ", indicesValidos)}." });
            }

            try
            {
                var tileBytes = await _sateliteService.GetTileAsync(z, x, y, indice.ToUpperInvariant(), fecha);

                if (tileBytes == null)
                {
                    _logger.LogWarning("Tile no disponible: {Indice} {Fecha} z={Z} x={X} y={Y}",
                        indice, fecha, z, x, y);
                    return NotFound();
                }

                // Cache-Control: 7 días (el TTL en disco ya maneja la expiración)
                Response.Headers["Cache-Control"] = "public, max-age=604800";
                Response.Headers["X-Tile-Source"] = "agroform-proxy";
                Response.Headers["X-Tile-Indice"] = indice.ToUpperInvariant();
                Response.Headers["X-Tile-Fecha"] = fecha;

                return File(tileBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sirviendo tile satelital: {Indice} {Fecha}", indice, fecha);
                return StatusCode(500, new { error = "Error interno al procesar tile satelital." });
            }
        }

        // ============================================================
        // DATOS ANALÍTICOS (FASE 1)
        // ============================================================

        /// <summary>
        /// Obtiene los índices satelitales persistidos para un lote específico.
        /// </summary>
        /// <param name="idLote">ID del lote</param>
        /// <param name="idCampania">Campaña para filtrar (opcional)</param>
        [HttpGet("indices/lote/{idLote}")]
        public async Task<IActionResult> GetIndicesLote(int idLote, [FromQuery] int? idCampania = null)
        {
            try
            {
                var indices = await _sateliteService.GetIndicesLoteAsync(idLote, idCampania);
                if (indices == null)
                {
                    return Ok(new GenericResponse<IndicesSatelitalesLoteDto>
                    {
                        Success = false,
                        Message = "No hay datos satelitales para este lote. Los datos se poblarán con el próximo ciclo de actualización.",
                        Object = null
                    });
                }

                return Ok(new GenericResponse<IndicesSatelitalesLoteDto>
                {
                    Success = true,
                    Object = indices,
                    Message = "Datos satelitales obtenidos correctamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener índices satelitales del lote {IdLote}", idLote);
                return StatusCode(500, new GenericResponse<IndicesSatelitalesLoteDto>
                {
                    Success = false,
                    Message = "Error interno al obtener datos satelitales"
                });
            }
        }

        /// <summary>
        /// Obtiene los índices satelitales promedio para un campo completo.
        /// </summary>
        /// <param name="idCampo">ID del campo</param>
        /// <param name="idCampania">Campaña para filtrar (opcional)</param>
        [HttpGet("indices/campo/{idCampo}")]
        public async Task<IActionResult> GetIndicesCampo(int idCampo, [FromQuery] int? idCampania = null)
        {
            try
            {
                var indices = await _sateliteService.GetIndicesCampoAsync(idCampo, idCampania);
                if (indices == null)
                {
                    return Ok(new GenericResponse<IndicesSatelitalesLoteDto>
                    {
                        Success = false,
                        Message = "No hay datos satelitales para este campo.",
                        Object = null
                    });
                }

                return Ok(new GenericResponse<IndicesSatelitalesLoteDto>
                {
                    Success = true,
                    Object = indices,
                    Message = "Datos satelitales del campo obtenidos correctamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener índices satelitales del campo {IdCampo}", idCampo);
                return StatusCode(500, new GenericResponse<IndicesSatelitalesLoteDto>
                {
                    Success = false,
                    Message = "Error interno al obtener datos satelitales"
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de lotes pendientes de actualización satelital.
        /// Usado por n8n scheduler para orquestar actualizaciones batch.
        /// </summary>
        /// <param name="diasSinActualizar">Días sin actualizar para considerar pendiente. Default: 5</param>
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetLotesPendientes([FromQuery] int diasSinActualizar = 5)
        {
            try
            {
                var pendientes = await _sateliteService.GetLotesPendientesAsync(diasSinActualizar);

                return Ok(new GenericResponse<List<LotePendienteActualizacionDto>>
                {
                    Success = true,
                    Object = pendientes,
                    Message = $"Se encontraron {pendientes.Count} lotes pendientes de actualización"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes pendientes de actualización satelital");
                return StatusCode(500, new GenericResponse<List<LotePendienteActualizacionDto>>
                {
                    Success = false,
                    Message = "Error al obtener lotes pendientes"
                });
            }
        }

        /// <summary>
        /// Actualiza bajo demanda los índices satelitales de un lote.
        /// Consulta Sentinel Hub Statistics API y persiste en SQL.
        /// </summary>
        /// <param name="idLote">ID del lote a actualizar</param>
        /// <param name="request">Parámetros opcionales (fecha desde/hasta)</param>
        [HttpPost("actualizar/{idLote}")]
        public async Task<IActionResult> ActualizarIndicesLote(int idLote, [FromBody] IndicesSatelitalesRequest? request = null)
        {
            try
            {
                var registrosInsertados = await _sateliteService.ActualizarIndicesLoteAsync(
                    idLote,
                    request?.FechaDesde,
                    request?.FechaHasta);

                return Ok(new
                {
                    success = true,
                    message = $"Actualización completada. {registrosInsertados} registros insertados.",
                    registrosInsertados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar índices satelitales del lote {IdLote}", idLote);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar lote: {ex.Message}"
                });
            }
        }

        // ============================================================
        // HEALTH CHECK
        // ============================================================

        /// <summary>
        /// Verifica la salud del servicio satelital probando conexión OAuth con Copernicus Data Space Ecosystem.
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> Health()
        {
            var ok = await _sateliteService.HealthCheckAsync();

            return Ok(new
            {
                status = ok ? "healthy" : "unhealthy",
                service = "satelite",
                timestamp = TimeHelper.GetArgentinaTime(),
                copernicusConnected = ok
            });
        }
    }
}
