using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Satelital;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgroForm.Business.Services
{
    /// <summary>
    /// Servicio de integración con Copernicus Data Space Ecosystem.
    /// Maneja autenticación OIDC, proxy de tiles via Processing API, caché en disco,
    /// consultas de series temporales y persistencia local en SQL Server.
    /// 
    /// Flujo de tiles:
    /// 1. Obtiene token OIDC (cacheado en MemoryCache 55 min)
    /// 2. Busca tile en caché de disco
    /// 3. Si no existe: lo solicita a Copernicus Processing API y lo cachea
    /// 4. Devuelve bytes del tile
    /// 
    /// Flujo de datos analíticos (Processing API + STAC):
    /// 1. Busca primero en SQL (datos persistidos)
    /// 2. Si no hay datos recientes, descubre escenas vía STAC API
    /// 3. Consulta Processing API para calcular índices sobre el polígono
    /// 4. Persiste el resultado en SQL
    /// 5. Devuelve DTO con promedios y serie temporal
    /// 
    /// NOTA: Registro gratuito en https://dataspace.copernicus.eu/
    /// Crear OAuth Client en el dashboard de Copernicus.
    /// NO se requiere instanceId ni plan pago.
    /// 
    /// Este servicio se auto-registra por naming convention (ISateliteService → SateliteService)
    /// en ServiceExtensions.cs y StartupHelper.cs
    /// </summary>
    public class SateliteService : ISateliteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly CopernicusConfig _config;
        private readonly ILogger<SateliteService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly string _tilesRoot;

        // MemoryCache keys
        private const string CACHE_KEY_TOKEN = "Copernicus_AccessToken";
        private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(55); // Token expira a los 60 min

        // Cache keys para datos analíticos (JSON, no PNG)
        private const string CACHE_KEY_PREFIX_INDICES = "Satelite_Indices_";
        private static readonly TimeSpan IndicesMemoryCacheDuration = TimeSpan.FromMinutes(15); // 15 min para datos analíticos

        public SateliteService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IOptions<CopernicusConfig> config,
            ILogger<SateliteService> logger,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _config = config.Value;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _tilesRoot = _config.TileCacheRoot;

            if (string.IsNullOrEmpty(_config.TileCacheRoot))
            {
                _tilesRoot = Path.Combine(Path.GetTempPath(), "satelite-tiles");
            }
        }

        // ============================================================
        // TILES VÍA PROCESSING API (FASE 0)
        // ============================================================

        /// <summary>
        /// Obtiene un tile para un índice y fecha específicos.
        /// Busca primero en caché de disco; si no existe, lo obtiene de Copernicus Processing API y lo cachea.
        /// </summary>
        public async Task<byte[]?> GetTileAsync(int z, int x, int y, string indice, string fecha)
        {
            // 1. Validar fecha explícita
            if (!DateOnly.TryParseExact(fecha, "yyyy-MM-dd", out _))
            {
                _logger.LogWarning("Fecha inválida: {Fecha}. Debe ser YYYY-MM-DD explícito.", fecha);
                return null;
            }

            // 2. Construir ruta de caché en disco
            var tileRelPath = Path.Combine(indice, fecha, z.ToString(), x.ToString(), $"{y}.png");
            var tileFullPath = Path.Combine(_tilesRoot, tileRelPath);

            // 3. Verificar caché de disco
            if (File.Exists(tileFullPath))
            {
                var fileInfo = new FileInfo(tileFullPath);
                var age = TimeHelper.GetArgentinaTime() - fileInfo.LastWriteTimeUtc;

                // Si el tile tiene menos de TileCacheDías, servirlo desde caché
                if (age.TotalDays < _config.TileCacheDias)
                {
                    _logger.LogDebug("Cache HIT: {Path}", tileRelPath);
                    return await File.ReadAllBytesAsync(tileFullPath);
                }

                // Tile expirado: eliminar y regenerar
                _logger.LogDebug("Cache EXPIRED: {Path} (age: {Age}d)", tileRelPath, Math.Round(age.TotalDays, 1));
                File.Delete(tileFullPath);
            }

            // 4. Cache MISS: obtener tile desde Copernicus Processing API
            _logger.LogInformation("Cache MISS: Solicitando tile {Indice} {Fecha} z={Z} x={X} y={Y}",
                indice, fecha, z, x, y);

            try
            {
                var tileBytes = await FetchTileFromCopernicusAsync(z, x, y, indice, fecha);

                if (tileBytes == null || tileBytes.Length == 0)
                {
                    _logger.LogWarning("Copernicus devolvió tile vacío para {Indice} {Fecha}", indice, fecha);
                    return null;
                }

                // 5. Cachear en disco
                var dir = Path.GetDirectoryName(tileFullPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                await File.WriteAllBytesAsync(tileFullPath, tileBytes);

                _logger.LogDebug("Tile cacheado en disco: {Path} ({Size} bytes)", tileRelPath, tileBytes.Length);
                return tileBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tile de Copernicus: {Indice} {Fecha} z={Z} x={X} y={Y}",
                    indice, fecha, z, x, y);
                return null;
            }
        }

        // ============================================================
        // DATOS ANALÍTICOS: Índices por lote (FASE 1)
        // ============================================================

        /// <summary>
        /// Obtiene los índices satelitales de un lote.
        /// Busca primero en SQL; si no hay datos, retorna null (el scheduler n8n se encarga de poblar).
        /// </summary>
        public async Task<IndicesSatelitalesLoteDto?> GetIndicesLoteAsync(int idLote, int? idCampania = null)
        {
            try
            {
                // Cache key en MemoryCache (solo JSON, no PNG)
                var cacheKey = $"{CACHE_KEY_PREFIX_INDICES}Lote_{idLote}_Campania_{idCampania ?? 0}";
                if (_memoryCache.TryGetValue<IndicesSatelitalesLoteDto>(cacheKey, out var cached))
                {
                    return cached;
                }

                // Query SQL para índices persistidos
                var query = _unitOfWork.Repository<IndiceSatelital>().Query()
                    .Where(i => i.IdLote == idLote);

                if (idCampania.HasValue)
                {
                    query = query.Where(i => i.IdCampania == idCampania);
                }

                // Si no hay filtro de campaña, traer últimos 90 días
                if (!idCampania.HasValue)
                {
                    var fechaLimite = TimeHelper.GetArgentinaTime().AddDays(-90);
                    query = query.Where(i => i.FechaCaptura >= fechaLimite);
                }

                var indices = await query
                    .OrderByDescending(i => i.FechaCaptura)
                    .ToListAsync();

                if (indices.Count == 0)
                {
                    _logger.LogDebug("No hay índices satelitales persistidos para lote {IdLote}", idLote);
                    return null;
                }

                var result = MapToDto(indices);
                result.EsSatelital = true;

                // Cachear en MemoryCache (solo JSON)
                _memoryCache.Set(cacheKey, result, IndicesMemoryCacheDuration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener índices satelitales del lote {IdLote}", idLote);
                return null;
            }
        }

        /// <summary>
        /// Obtiene los índices satelitales promedio de un campo completo.
        /// Agrega los datos de todos los lotes del campo.
        /// </summary>
        public async Task<IndicesSatelitalesLoteDto?> GetIndicesCampoAsync(int idCampo, int? idCampania = null)
        {
            try
            {
                var cacheKey = $"{CACHE_KEY_PREFIX_INDICES}Campo_{idCampo}_Campania_{idCampania ?? 0}";
                if (_memoryCache.TryGetValue<IndicesSatelitalesLoteDto>(cacheKey, out var cached))
                {
                    return cached;
                }

                // Obtener IDs de todos los lotes del campo
                var lotes = await _unitOfWork.Repository<Lote>().Query()
                    .Where(l => l.IdCampo == idCampo)
                    .Select(l => l.Id)
                    .ToListAsync();

                if (lotes.Count == 0)
                {
                    _logger.LogDebug("El campo {IdCampo} no tiene lotes", idCampo);
                    return null;
                }

                // Obtener índices para TODOS los lotes del campo
                var query = _unitOfWork.Repository<IndiceSatelital>().Query()
                    .Where(i => lotes.Contains(i.IdLote));

                if (idCampania.HasValue)
                {
                    query = query.Where(i => i.IdCampania == idCampania);
                }

                if (!idCampania.HasValue)
                {
                    var fechaLimite = TimeHelper.GetArgentinaTime().AddDays(-90);
                    query = query.Where(i => i.FechaCaptura >= fechaLimite);
                }

                var indices = await query
                    .OrderByDescending(i => i.FechaCaptura)
                    .ToListAsync();

                if (indices.Count == 0)
                {
                    _logger.LogDebug("No hay índices satelitales persistidos para el campo {IdCampo}", idCampo);
                    return null;
                }

                var result = MapToDto(indices);
                result.EsSatelital = true;

                _memoryCache.Set(cacheKey, result, IndicesMemoryCacheDuration);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener índices satelitales del campo {IdCampo}", idCampo);
                return null;
            }
        }

        /// <summary>
        /// Obtiene la lista de lotes pendientes de actualización satelital
        /// (para el scheduler n8n, FASE 2).
        /// </summary>
        public async Task<List<LotePendienteActualizacionDto>> GetLotesPendientesAsync(
            int diasSinActualizar = 5, int? idLicencia = null)
        {
            try
            {
                var licenciaEfectiva = idLicencia ?? _userContext.IdLicencia;
                var fechaCorte = TimeHelper.GetArgentinaTime().AddDays(-diasSinActualizar);

                // Última consulta satelital por lote (agregada)
                var ultimasConsultas = await _unitOfWork.Repository<LogConsultaSatelital>().Query()
                    .Where(l => l.Exitoso && l.TipoConsulta != "TILE")
                    .GroupBy(l => l.IdLote)
                    .Select(g => new { IdLote = g.Key, UltimaFecha = g.Max(l => l.FechaConsulta) })
                    .ToListAsync();

                var lotesConFechaReciente = ultimasConsultas
                    .Where(u => u.UltimaFecha >= fechaCorte)
                    .Select(u => u.IdLote)
                    .ToHashSet();

                // Todos los lotes de la licencia
                var lotes = await _unitOfWork.Repository<Lote>().Query()
                    .Include(l => l.Campo)
                    .Where(l => l.IdLicencia == licenciaEfectiva)
                    .ToListAsync();

                // Lotes pendientes: no tienen consulta reciente
                var pendientes = new List<LotePendienteActualizacionDto>();

                foreach (var lote in lotes)
                {
                    if (lotesConFechaReciente.Contains(lote.Id))
                        continue; // Actualizado recientemente

                    var ultimaConsulta = ultimasConsultas
                        .FirstOrDefault(u => u.IdLote == lote.Id)?.UltimaFecha;

                    var geometria = await _unitOfWork.Repository<LoteGeometria>().Query()
                        .Where(g => g.IdLote == lote.Id)
                        .Select(g => g.GeometriaSimplificada)
                        .FirstOrDefaultAsync();

                    pendientes.Add(new LotePendienteActualizacionDto
                    {
                        IdLote = lote.Id,
                        NombreLote = lote.Nombre,
                        IdCampo = lote.IdCampo,
                        NombreCampo = lote.Campo?.Nombre ?? "N/A",
                        UltimaConsulta = ultimaConsulta,
                        DiasSinActualizar = ultimaConsulta.HasValue
                            ? (int)(TimeHelper.GetArgentinaTime() - ultimaConsulta.Value).TotalDays
                            : 999,
                        GeometriaSimplificada = geometria
                    });
                }

                return pendientes.OrderByDescending(p => p.DiasSinActualizar).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes pendientes de actualización satelital");
                return new List<LotePendienteActualizacionDto>();
            }
        }

        /// <summary>
        /// Actualiza los índices satelitales de un lote consultando Copernicus Processing API
        /// y persistiendo en SQL.
        ///
        /// Flujo optimizado:
        /// 1. Descubre escenas disponibles vía STAC API para determinar fechas con datos
        /// 2. Acota el rango de fechas a aquellas con escenas disponibles
        /// 3. Consulta Processing API con evalscripts para calcular NDVI/NDWI promedio
        /// 4. Persiste resultados en SQL con metadatos de nubosidad (priorizando STAC)
        ///
        /// En producción, n8n scheduler orquesta las actualizaciones.
        /// </summary>
        public async Task<int> ActualizarIndicesLoteAsync(
            int idLote, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var fechaDesdeEfectiva = fechaDesde ?? TimeHelper.GetArgentinaTime().AddDays(-30);
            var fechaHastaEfectiva = fechaHasta ?? TimeHelper.GetArgentinaTime();
            var registrosInsertados = 0;
            var stacScenes = new List<(DateTime Fecha, decimal CloudCover)>();

            try
            {
                // 1. Obtener geometría del lote (necesaria para Processing API y STAC)
                var geometria = await _unitOfWork.Repository<LoteGeometria>().Query()
                    .FirstOrDefaultAsync(g => g.IdLote == idLote);

                if (geometria == null || string.IsNullOrEmpty(geometria.GeometriaSimplificada))
                {
                    _logger.LogWarning("Lote {IdLote} no tiene geometría simplificada. " +
                        "Debe cargarse previamente en LotesGeometria.", idLote);
                    return 0;
                }

                // 2. Descubrir escenas disponibles via STAC API para acotar fechas
                stacScenes = await DiscoverBestScenesAsync(
                    geometria.GeometriaSimplificada,
                    fechaDesdeEfectiva,
                    fechaHastaEfectiva,
                    maxCloudCover: 80,
                    maxResults: 20);

                if (stacScenes.Count > 0)
                {
                    // Acotar rango de fechas a aquellas con escenas disponibles
                    var stacMinDate = stacScenes.Min(s => s.Fecha);
                    var stacMaxDate = stacScenes.Max(s => s.Fecha);
                    fechaDesdeEfectiva = stacMinDate.AddDays(-1);
                    fechaHastaEfectiva = stacMaxDate.AddDays(1);

                    _logger.LogInformation(
                        "STAC: {Count} escenas disponibles para lote {IdLote}, " +
                        "rango acotado a [{Desde:yyyy-MM-dd}, {Hasta:yyyy-MM-dd}]",
                        stacScenes.Count, idLote, fechaDesdeEfectiva, fechaHastaEfectiva);
                }
                else
                {
                    _logger.LogWarning(
                        "STAC: sin escenas disponibles para lote {IdLote} en [{Desde:yyyy-MM-dd}, {Hasta:yyyy-MM-dd}]. " +
                        "Se usará rango original sin optimización STAC.",
                        idLote, fechaDesdeEfectiva, fechaHastaEfectiva);
                }

                // 3. Parsear GeoJSON simplificado
                using var geoDoc = JsonDocument.Parse(geometria.GeometriaSimplificada);
                var geometry = geoDoc.RootElement;

                // 4. Obtener token OIDC de Copernicus
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("No se pudo obtener token OIDC para Copernicus Processing API");
                    return 0;
                }

                // 5. Consultar Processing API para NDVI y NDWI
                var ndviResultados = await FetchStatisticsFromCopernicusAsync(
                    token, geometry, fechaDesdeEfectiva, fechaHastaEfectiva, "NDVI");

                var ndwiResultados = await FetchStatisticsFromCopernicusAsync(
                    token, geometry, fechaDesdeEfectiva, fechaHastaEfectiva, "NDWI");

                // 6. Parsear respuestas
                var fechasNdvi = ParseProcessingResponse(ndviResultados, "NDVI");
                var fechasNdwi = ParseProcessingResponse(ndwiResultados, "NDWI");

                // Crear lookup rápido para NDWI y STAC
                var ndwiDict = fechasNdwi.ToDictionary(e => e.Fecha, e => e);
                var stacDict = stacScenes
                    .GroupBy(s => s.Fecha.Date)
                    .ToDictionary(g => g.Key, g => g.MinBy(s => s.CloudCover));

                // 7. Persistir en SQL
                var licenciaId = _userContext.IdLicencia;
                var repo = _unitOfWork.Repository<IndiceSatelital>();

                // Obtener fechas ya existentes para evitar duplicados
                var fechasExistentes = await repo.Query()
                    .Where(i => i.IdLote == idLote)
                    .Select(i => i.FechaCaptura.Date)
                    .ToListAsync();

                var existentesSet = new HashSet<DateTime>(fechasExistentes);

                foreach (var (fecha, ndvi, cloudCoverNdvi) in fechasNdvi)
                {
                    if (existentesSet.Contains(fecha.Date))
                        continue;

                    ndwiDict.TryGetValue(fecha, out var ndwiEntry);

                    // Priorizar cloud cover de STAC (más preciso que el de Processing API)
                    decimal? cloudCoverFinal = cloudCoverNdvi ?? ndwiEntry.CloudCover;
                    if (stacDict.TryGetValue(fecha.Date, out var stacScene) && stacScene.CloudCover < (cloudCoverFinal ?? 100))
                    {
                        cloudCoverFinal = stacScene.CloudCover;
                    }

                    var indice = new IndiceSatelital
                    {
                        IdLote = idLote,
                        IdLicencia = licenciaId,
                        FechaCaptura = fecha,
                        FechaConsulta = TimeHelper.GetArgentinaTime(),
                        Fuente = "Sentinel-2 (Copernicus)",
                        ResolucionMts = 10,
                        CloudCover = cloudCoverFinal,
                        NDVI = ndvi,
                        NDWI = ndwiEntry.Valor,
                        EsValido = (cloudCoverFinal ?? 0) < 80, // Válido si <80% nubes
                        IdCampania = null // Se asigna con el scheduler en producción
                    };

                    await repo.AddAsync(indice);
                    registrosInsertados++;
                }

                if (registrosInsertados > 0)
                {
                    await _unitOfWork.SaveAsync();

                    // Invalidar cache de MemoryCache para este lote
                    var cacheKey = $"{CACHE_KEY_PREFIX_INDICES}Lote_{idLote}_Campania_0";
                    _memoryCache.Remove(cacheKey);

                    _logger.LogInformation("Actualizados {Count} registros satelitales para lote {IdLote}",
                        registrosInsertados, idLote);
                }

                // 8. Log de auditoría
                stopwatch.Stop();
                await LogConsultaAsync(idLote, "TIME_SERIES", "ALL",
                    fechaDesdeEfectiva, fechaHastaEfectiva,
                    stopwatch.ElapsedMilliseconds, true, null,
                    $"insertados={registrosInsertados}|stac_scenes={stacScenes.Count}");

                return registrosInsertados;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error al actualizar índices satelitales del lote {IdLote}", idLote);

                await LogConsultaAsync(idLote, "TIME_SERIES", "ALL",
                    fechaDesdeEfectiva, fechaHastaEfectiva,
                    stopwatch.ElapsedMilliseconds, false, ex.Message);

                return 0;
            }
        }

        /// <summary>
        /// Verifica la salud del servicio satelital probando la conexión con Copernicus OIDC.
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_config.ClientId) || string.IsNullOrEmpty(_config.ClientSecret))
                {
                    _logger.LogWarning("HealthCheck: Copernicus no configurado (ClientId/ClientSecret vacíos)");
                    return false;
                }

                var token = await GetAccessTokenAsync();
                var ok = !string.IsNullOrEmpty(token);

                _logger.LogInformation("HealthCheck satelital (Copernicus): {Status}", ok ? "OK" : "FAIL");
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HealthCheck satelital (Copernicus): Error");
                return false;
            }
        }

        /// <summary>
        /// Descubre las mejores escenas satelitales disponibles para una geometría y rango de fechas
        /// usando la STAC API de Copernicus (stac.dataspace.copernicus.eu).
        ///
        /// Retorna las escenas ordenadas por cobertura de nubes ascendente (mejores primero).
        /// La STAC API no requiere autenticación.
        /// </summary>
        public async Task<List<(DateTime Fecha, decimal CloudCover)>> DiscoverBestScenesAsync(
            string geometryJson, DateTime fechaDesde, DateTime fechaHasta,
            int maxCloudCover = 80, int maxResults = 10)
        {
            try
            {
                // Parsear geometría GeoJSON para incluir en el filtro CQL2
                var geometry = JsonSerializer.Deserialize<JsonElement>(geometryJson);

                // Construir payload CQL2 JSON para STAC API
                var stacPayload = new
                {
                    filter = new
                    {
                        op = "and",
                        args = new object[]
                        {
                            new { op = "=", args = new object[] { new { property = "collection" }, "sentinel-2-l2a" } },
                            new { op = "<=", args = new object[] { new { property = "eo:cloud_cover" }, maxCloudCover } },
                            new { op = ">=", args = new object[] { new { property = "datetime" }, new { timestamp = fechaDesde.ToString("o") } } },
                            new { op = "<=", args = new object[] { new { property = "datetime" }, new { timestamp = fechaHasta.ToString("o") } } },
                            new { op = "s_intersects", args = new object[] { new { property = "geometry" }, geometry } }
                        }
                    },
                    limit = maxResults
                };

                var jsonPayload = JsonSerializer.Serialize(stacPayload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var stacUrl = $"https://{_config.StacBaseUrl}/v1/search";

                using var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, stacUrl)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                using var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("STAC API error {StatusCode}: {Error}",
                        (int)response.StatusCode,
                        errorBody.Length > 300 ? errorBody[..300] : errorBody);
                    return new List<(DateTime, decimal)>();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var scenes = new List<(DateTime Fecha, decimal CloudCover)>();

                if (root.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
                {
                    foreach (var feature in features.EnumerateArray())
                    {
                        if (!feature.TryGetProperty("properties", out var properties))
                            continue;

                        // Extraer datetime del feature
                        DateTime? fecha = null;
                        if (properties.TryGetProperty("datetime", out var dtProp))
                        {
                            var dtStr = dtProp.GetString();
                            if (!string.IsNullOrEmpty(dtStr) && DateTime.TryParse(dtStr, out var parsedDt))
                                fecha = parsedDt;
                        }

                        if (fecha == null)
                            continue;

                        // Extraer cloud cover
                        decimal cloudCover = 100;
                        if (properties.TryGetProperty("eo:cloud_cover", out var ccProp))
                        {
                            cloudCover = (decimal)ccProp.GetDouble();
                        }

                        scenes.Add((fecha.Value, cloudCover));
                    }
                }

                // Ordenar por cobertura de nubes ascendente (mejores primero) y limitar
                scenes = scenes.OrderBy(s => s.CloudCover).Take(maxResults).ToList();

                _logger.LogInformation(
                    "STAC API: {Count} escenas encontradas para [{Desde:yyyy-MM-dd}, {Hasta:yyyy-MM-dd}], nubes<={MaxNubes}%",
                    scenes.Count, fechaDesde, fechaHasta, maxCloudCover);

                return scenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descubrir escenas via STAC API");
                return new List<(DateTime, decimal)>();
            }
        }

        // ============================================================
        // MÉTODOS PRIVADOS
        // ============================================================

        /// <summary>
        /// Mapea una lista de entidades IndiceSatelital a DTO.
        /// </summary>
        private static IndicesSatelitalesLoteDto MapToDto(List<IndiceSatelital> indices)
        {
            var validos = indices.Where(i => i.EsValido).ToList();
            var serieTemporal = indices
                .OrderBy(i => i.FechaCaptura)
                .Select(i => new DatoIndiceSatelitalDto
                {
                    Fecha = i.FechaCaptura,
                    NDVI = i.NDVI,
                    NDWI = i.NDWI,
                    CloudCover = i.CloudCover,
                    EsValido = i.EsValido
                })
                .ToList();

            return new IndicesSatelitalesLoteDto
            {
                NDVIPromedio = validos.Any(i => i.NDVI.HasValue)
                    ? Math.Round(validos.Where(i => i.NDVI.HasValue).Average(i => i.NDVI!.Value), 4)
                    : null,
                NDWIPromedio = validos.Any(i => i.NDWI.HasValue)
                    ? Math.Round(validos.Where(i => i.NDWI.HasValue).Average(i => i.NDWI!.Value), 4)
                    : null,
                UltimoNDVI = indices.FirstOrDefault(i => i.NDVI.HasValue)?.NDVI,
                UltimoNDWI = indices.FirstOrDefault(i => i.NDWI.HasValue)?.NDWI,
                UltimaFecha = indices.FirstOrDefault()?.FechaCaptura,
                CloudCoverPromedio = indices.Any(i => i.CloudCover.HasValue)
                    ? Math.Round(indices.Where(i => i.CloudCover.HasValue).Average(i => i.CloudCover!.Value), 1)
                    : null,
                EsSatelital = true,
                SerieTemporal = serieTemporal
            };
        }

        /// <summary>
        /// Obtiene un token de acceso OIDC de Copernicus Data Space Ecosystem.
        /// Cacheado en MemoryCache por 55 minutos (token expira a los 60 min).
        /// 
        /// Endpoint: https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token
        /// Flow: client_credentials (machine-to-machine)
        /// </summary>
        private async Task<string?> GetAccessTokenAsync()
        {
            // Early exit si no hay credenciales configuradas
            if (string.IsNullOrEmpty(_config.ClientId) || string.IsNullOrEmpty(_config.ClientSecret))
            {
                _logger.LogWarning("GetAccessToken: Copernicus no configurado (ClientId/ClientSecret vacíos). " +
                    "Configure las credenciales en appsettings.json sección 'Copernicus'.");
                return null;
            }

            return await _memoryCache.GetOrCreateAsync(CACHE_KEY_TOKEN, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TokenCacheDuration;

                _logger.LogDebug("Solicitando nuevo token OIDC a Copernicus...");

                using var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _config.AuthUrl)
                {
                    Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _config.ClientId),
                        new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
                    })
                };

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // OIDC response: el token puede venir como "access_token" (estándar OAuth2/OIDC)
                var token = doc.RootElement.GetProperty("access_token").GetString();

                _logger.LogDebug("Token OIDC obtenido exitosamente de Copernicus");
                return token;
            });
        }

        /// <summary>
        /// Solicita un tile a Copernicus Processing API.
        /// Usa el endpoint POST /process/v1 con evalscript para generar el tile NDVI/NDWI.
        /// 
        /// Documentación: https://documentation.dataspace.copernicus.eu/APIs/SentinelHub/Evalscript.html
        /// Base URL: https://sh.dataspace.copernicus.eu/process/v1
        /// 
        /// NOTA: Copernicus NO tiene WMTS público para índices personalizados (NDVI, NDWI).
        /// La Processing API es el equivalente oficial y gratuito dentro del ecosistema Copernicus.
        /// </summary>
        private async Task<byte[]?> FetchTileFromCopernicusAsync(int z, int x, int y, string indice, string fecha)
        {
            try
            {
                // 1. Obtener token OIDC (ahora dentro del try-catch para manejar errores gracefulmente)
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("FetchTile: sin token OIDC. Copernicus no disponible (ClientId/ClientSecret vacíos).");
                    return null;
                }

                // 2. Convertir coordenadas de tile (z/x/y) a bounding box geográfico (EPSG:3857)
                var bbox = TileXYToBBox(x, y, z);

                var evalscript = GetEvalScript(indice, isVisual: true);

                // 3. Construir payload para Processing API
                // Documentación: https://documentation.dataspace.copernicus.eu/APIs/SentinelHub/ApiReference.html
                var payload = new
                {
                    input = new
                    {
                        bounds = new
                        {
                            bbox = new[] { bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax },
                            properties = new { crs = "http://www.opengis.net/def/crs/EPSG/0/3857" }
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "sentinel-2-l2a",
                                dataFilter = new
                                {
                                    timeRange = new
                                    {
                                        from = DateTime.Parse(fecha).AddDays(-15).ToString("yyyy-MM-dd") + "T00:00:00Z",
                                        to = $"{fecha}T23:59:59Z"
                                    },
                                    maxCloudCoverage = 80
                                }
                            }
                        }
                    },
                    output = new
                    {
                        width = 256,
                        height = 256,
                        responses = new[]
                        {
                            new
                            {
                                identifier = "default",
                                format = new
                                {
                                    type = "image/png"
                                }
                            }
                        }
                    },
                    evalscript = evalscript
                };

                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // URL correcta de Copernicus Processing API: /api/v1/process (NO /process/v1)
                var processUrl = $"{_config.BaseUrl}/api/v1/process";
                using var client = _httpClientFactory.CreateClient("Copernicus");
                var request = new HttpRequestMessage(HttpMethod.Post, processUrl)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Copernicus Processing API error {StatusCode} para {Indice} {Fecha}: {Error}",
                        (int)response.StatusCode, indice, fecha,
                        errorBody.Length > 2000 ? errorBody[..2000] : errorBody);
                    return null;
                }

                // La respuesta es el contenido binario de la imagen (PNG)
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar tile a Copernicus Processing API: {Indice} {Fecha}",
                    indice, fecha);
                return null;
            }
        }

        /// <summary>
        /// Consulta Copernicus Processing API para obtener estadísticas de un índice sobre un polígono.
        /// Usa width:0, height:0 para obtener solo estadísticas agregadas, no una imagen.
        /// 
        /// Documentación: https://documentation.dataspace.copernicus.eu/APIs/SentinelHub/Process.html
        /// </summary>
        private async Task<JsonDocument?> FetchStatisticsFromCopernicusAsync(
            string token,
            JsonElement geometry,
            DateTime fechaDesde,
            DateTime fechaHasta,
            string indice)
        {
            try
            {
                var evalscript = GetEvalScript(indice, isVisual: false);

                var payload = new
                {
                    input = new
                    {
                        bounds = new
                        {
                            geometry = JsonSerializer.Deserialize<object>(geometry.GetRawText()),
                            properties = new { crs = "http://www.opengis.net/def/crs/EPSG/0/4326" }
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "sentinel-2-l2a",
                                dataFilter = new
                                {
                                    timeRange = new
                                    {
                                        from = fechaDesde.ToString("o"),
                                        to = fechaHasta.ToString("o")
                                    },
                                    maxCloudCoverage = 80
                                }
                            }
                        }
                    },
                    aggregation = new
                    {
                        timeInterval = new
                        {
                            from = fechaDesde.ToString("o"),
                            to = fechaHasta.ToString("o")
                        },
                        temporalLoops = new[] { "P1D" }, // Agregación por día
                        evalscript = evalscript,
                        width = 0, // Estadísticas, no imágenes
                        height = 0
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // URL correcta de Copernicus Processing API: /api/v1/process (NO /process/v1)
                var processUrl = $"{_config.BaseUrl}/api/v1/process";
                using var client = _httpClientFactory.CreateClient("Copernicus");
                var request = new HttpRequestMessage(HttpMethod.Post, processUrl)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Copernicus Processing API (stats) error {StatusCode} para {Indice}: {Error}",
                        (int)response.StatusCode, indice,
                        errorBody.Length > 500 ? errorBody[..500] : errorBody);
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Processing API para {Indice}", indice);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el evalscript para calcular un índice específico.
        /// Los evalscripts son compatibles tanto con Sentinel Hub como con Copernicus Processing API.
        /// Si isVisual es true, retorna un evalscript que mapea el índice a colores RGBA,
        /// necesario para que Processing API genere un PNG (image/png no soporta FLOAT32).
        /// </summary>
        private static string GetEvalScript(string indice, bool isVisual = false)
        {
            if (isVisual)
            {
                return indice switch
                {
                    "NDVI" => """
                        //VERSION=3
                        function setup() {
                            return {
                                input: ["B04", "B08", "dataMask"],
                                output: { bands: 4, sampleType: "AUTO8" }
                            };
                        }
                        function evaluatePixel(sample) {
                            if (sample.dataMask !== 1) return [0, 0, 0, 0];
                            let denom = sample.B08 + sample.B04;
                            if (denom === 0) return [0, 0, 0, 0];
                            let ndvi = (sample.B08 - sample.B04) / denom;
                            
                            if (ndvi < -0.2) return [0.75, 0.75, 0.75, 1];
                            if (ndvi < 0) return [0.86, 0.86, 0.86, 1];
                            if (ndvi < 0.1) return [1, 1, 0.8, 1];
                            if (ndvi < 0.2) return [0.73, 0.82, 0.56, 1];
                            if (ndvi < 0.3) return [0.5, 0.7, 0.38, 1];
                            if (ndvi < 0.4) return [0.35, 0.6, 0.25, 1];
                            if (ndvi < 0.5) return [0.25, 0.53, 0.2, 1];
                            if (ndvi < 0.6) return [0.16, 0.44, 0.13, 1];
                            if (ndvi < 0.7) return [0.12, 0.36, 0.11, 1];
                            if (ndvi < 0.8) return [0.07, 0.28, 0.08, 1];
                            return [0.04, 0.2, 0.05, 1];
                        }
                        """,
                    "NDWI" => """
                        //VERSION=3
                        function setup() {
                            return {
                                input: ["B03", "B08", "dataMask"],
                                output: { bands: 4, sampleType: "AUTO8" }
                            };
                        }
                        function evaluatePixel(sample) {
                            if (sample.dataMask !== 1) return [0, 0, 0, 0];
                            let denom = sample.B03 + sample.B08;
                            if (denom === 0) return [0, 0, 0, 0];
                            let ndwi = (sample.B03 - sample.B08) / denom;
                            
                            if (ndwi < -0.3) return [0.2, 0.6, 0.2, 1]; 
                            if (ndwi < 0) return [0.6, 0.8, 0.4, 1];
                            if (ndwi < 0.2) return [0.6, 0.8, 0.8, 1]; 
                            if (ndwi < 0.4) return [0.4, 0.6, 0.8, 1]; 
                            return [0, 0, 0.8, 1];
                        }
                        """,
                    _ => throw new ArgumentException($"Índice no soportado para visualización: {indice}")
                };
            }

            return indice switch
            {
                "NDVI" => """
                    //VERSION=3
                    function setup() {
                        return {
                            input: ["B04", "B08"],
                            output: { bands: 1, sampleType: "FLOAT32" }
                        };
                    }
                    function evaluatePixel(sample) {
                        let ndvi = (sample.B08 - sample.B04) / (sample.B08 + sample.B04);
                        return [ndvi];
                    }
                    """,
                "NDWI" => """
                    //VERSION=3
                    function setup() {
                        return {
                            input: ["B03", "B08"],
                            output: { bands: 1, sampleType: "FLOAT32" }
                        };
                    }
                    function evaluatePixel(sample) {
                        let ndwi = (sample.B03 - sample.B08) / (sample.B03 + sample.B08);
                        return [ndwi];
                    }
                    """,
                _ => throw new ArgumentException($"Índice no soportado: {indice}")
            };
        }

        /// <summary>
        /// Parsea la respuesta de Processing API (con agregación) extrayendo fecha, valor promedio y cloud cover.
        /// El formato de respuesta es diferente al de Sentinel Hub Statistics API:
        /// Processing API devuelve datos en formato similar a:
        /// {
        ///   "data": [
        ///     {
        ///       "interval": { "from": "...", "to": "..." },
        ///       "outputs": { "B0": { "bands": { "B0": { "stats": { "mean": 0.5, "stDev": 0.1 } } } } },
        ///       "data": { "cloudCover": 12.5 }
        ///     }
        ///   ]
        /// }
        /// </summary>
        private static List<(DateTime Fecha, decimal? Valor, decimal? CloudCover)> ParseProcessingResponse(
            JsonDocument? responseDoc, string indice)
        {
            var resultados = new List<(DateTime Fecha, decimal? Valor, decimal? CloudCover)>();

            if (responseDoc == null)
                return resultados;

            var root = responseDoc.RootElement;

            // Buscar los datos en la respuesta de Processing API
            // La estructura puede variar, intentamos diferentes formatos
            if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    if (!item.TryGetProperty("interval", out var interval))
                        continue;

                    var fromStr = interval.GetProperty("from").GetString();
                    if (string.IsNullOrEmpty(fromStr) || !DateTime.TryParse(fromStr, out var fecha))
                        continue;

                    decimal? valor = null;
                    decimal? cloudCover = null;

                    // Extraer valor del índice de outputs
                    if (item.TryGetProperty("outputs", out var outputs))
                    {
                        // Processing API con aggregation devuelve outputs.B0.bands.B0.statistics.mean
                        if (outputs.TryGetProperty("B0", out var b0Output) &&
                            b0Output.TryGetProperty("bands", out var bands) &&
                            bands.TryGetProperty("B0", out var b0Band) &&
                            b0Band.TryGetProperty("statistics", out var stats) &&
                            stats.TryGetProperty("mean", out var mean))
                        {
                            valor = (decimal?)mean.GetDouble();
                        }
                    }

                    // Cloud cover del intervalo
                    if (item.TryGetProperty("data", out var intervalData) &&
                        intervalData.TryGetProperty("cloudCover", out var cc))
                    {
                        cloudCover = (decimal?)cc.GetDouble();
                    }

                    resultados.Add((fecha, valor, cloudCover));
                }
            }
            // Si no hay aggregation, probar formato simple (imagen única, no estadísticas)
            else if (root.TryGetProperty("value", out var value))
            {
                // Formato simple: solo un valor, sin series temporales
                // Esto ocurre cuando no se usa aggregation
            }

            return resultados;
        }

        /// <summary>
        /// Registra una consulta en el log de auditoría (LogsConsultasSatelitales).
        /// </summary>
        private async Task LogConsultaAsync(
            int? idLote, string tipoConsulta, string indiceSolicitado,
            DateTime? fechaDesde, DateTime? fechaHasta,
            long duracionMs, bool exitoso, string? errorMessage,
            string? parametrosExtra = null)
        {
            try
            {
                var log = new LogConsultaSatelital
                {
                    IdLote = idLote,
                    FechaConsulta = TimeHelper.GetArgentinaTime(),
                    TipoConsulta = tipoConsulta,
                    IndiceSolicitado = indiceSolicitado,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    DuracionMs = (int)Math.Min(duracionMs, int.MaxValue),
                    Exitoso = exitoso,
                    ErrorMessage = errorMessage?.Length > 500 ? errorMessage[..500] : errorMessage,
                    CostoEstimado = null, // Copernicus Processing API no tiene costo unitario
                    IdLicencia = _userContext.IdLicencia,
                    Parametros = parametrosExtra
                };

                await _unitOfWork.Repository<LogConsultaSatelital>().AddAsync(log);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // No propagar error de logging
                _logger.LogWarning(ex, "Error al registrar log de consulta satelital");
            }
        }

        // ============================================================
        // UTILIDADES GEOGRÁFICAS
        // ============================================================

        /// <summary>
        /// Convierte coordenadas de tile (z/x/y) a bounding box en EPSG:3857 (Web Mercator).
        /// </summary>
        private static (double XMin, double YMin, double XMax, double YMax) TileXYToBBox(int x, int y, int z)
        {
            var tileSize = 256.0;
            var initialResolution = 2 * Math.PI * 6378137 / tileSize;
            var originShift = 2 * Math.PI * 6378137 / 2.0;

            var resolution = initialResolution / Math.Pow(2, z);

            var xMin = x * tileSize * resolution - originShift;
            var yMax = originShift - y * tileSize * resolution;
            var xMax = (x + 1) * tileSize * resolution - originShift;
            var yMin = originShift - (y + 1) * tileSize * resolution;

            return (xMin, yMin, xMax, yMax);
        }
    }
}
