namespace AgroForm.Business.Contracts
{
    /// <summary>
    /// Servicio de integración con Copernicus Data Space Ecosystem para índices satelitales.
    /// Maneja autenticación OIDC, proxy de tiles vía Processing API, caché en disco,
    /// consultas de series temporales (STAC + Processing API) y persistencia local en SQL Server.
    ///
    /// Registro gratuito: https://dataspace.copernicus.eu/
    /// </summary>
    public interface ISateliteService
    {
        /// <summary>
        /// Obtiene un tile para un índice y fecha específicos.
        /// Busca primero en caché de disco; si no existe, lo obtiene de Copernicus Processing API y lo cachea.
        /// </summary>
        /// <param name="z">Zoom level</param>
        /// <param name="x">Tile X coordinate</param>
        /// <param name="y">Tile Y coordinate</param>
        /// <param name="indice">Nombre del índice: "NDVI", "NDWI"</param>
        /// <param name="fecha">Fecha en formato YYYY-MM-DD (explícita, nunca null)</param>
        /// <returns>Bytes de la imagen PNG del tile, o null si no hay datos</returns>
        Task<byte[]?> GetTileAsync(int z, int x, int y, string indice, string fecha);

        /// <summary>
        /// Obtiene los índices satelitales recientes de un lote.
        /// Busca primero en SQL (datos persistidos); si no hay datos, retorna null
        /// (el scheduler n8n se encarga de poblar los datos).
        /// </summary>
        /// <param name="idLote">ID del lote</param>
        /// <param name="idCampania">Campaña para filtrar (opcional)</param>
        /// <returns>Índices satelitales del lote</returns>
        Task<IndicesSatelitalesLoteDto?> GetIndicesLoteAsync(int idLote, int? idCampania = null);

        /// <summary>
        /// Obtiene los índices satelitales para un campo completo (promedio de todos los lotes).
        /// </summary>
        /// <param name="idCampo">ID del campo</param>
        /// <param name="idCampania">Campaña para filtrar (opcional)</param>
        /// <returns>Índices satelitales promedio del campo</returns>
        Task<IndicesSatelitalesLoteDto?> GetIndicesCampoAsync(int idCampo, int? idCampania = null);

        /// <summary>
        /// Obtiene la lista de lotes pendientes de actualización satelital
        /// (para el scheduler n8n).
        /// </summary>
        /// <param name="diasSinActualizar">Días sin actualizar para considerar pendiente. Default: 5</param>
        /// <param name="idLicencia">Licencia específica (opcional)</param>
        /// <returns>Lista de lotes pendientes</returns>
        Task<List<LotePendienteActualizacionDto>> GetLotesPendientesAsync(int diasSinActualizar = 5, int? idLicencia = null);

        /// <summary>
        /// Descubre las mejores escenas satelitales disponibles para una geometría y rango de fechas
        /// usando la STAC API de Copernicus (stac.dataspace.copernicus.eu).
        ///
        /// Retorna las escenas ordenadas por cobertura de nubes ascendente (mejores primero).
        /// Útil para determinar qué fechas tienen datos disponibles antes de llamar a Processing API.
        /// </summary>
        /// <param name="geometryJson">Geometría GeoJSON del polígono del lote</param>
        /// <param name="fechaDesde">Inicio del rango de búsqueda</param>
        /// <param name="fechaHasta">Fin del rango de búsqueda</param>
        /// <param name="maxCloudCover">Cobertura de nubes máxima (0-100, default: 80)</param>
        /// <param name="maxResults">Máximo de resultados a retornar (default: 10)</param>
        /// <returns>Lista de (Fecha, CloudCover) ordenada por mejor escena primero</returns>
        Task<List<(DateTime Fecha, decimal CloudCover)>> DiscoverBestScenesAsync(
            string geometryJson, DateTime fechaDesde, DateTime fechaHasta,
            int maxCloudCover = 80, int maxResults = 10);

        /// <summary>
        /// Actualiza los índices satelitales de un lote consultando Copernicus Processing API
        /// y persistiendo en SQL. Usado por n8n scheduler o bajo demanda.
        ///
        /// Para optimizar, primero descubre escenas vía STAC API y solo consulta Processing API
        /// para las fechas con datos disponibles.
        /// </summary>
        /// <param name="idLote">ID del lote a actualizar</param>
        /// <param name="fechaDesde">Fecha desde (opcional, default: 30 días atrás)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional, default: hoy)</param>
        /// <returns>Cantidad de registros insertados</returns>
        Task<int> ActualizarIndicesLoteAsync(int idLote, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

        /// <summary>
        /// Verifica la salud del servicio satelital probando la conexión con Copernicus OIDC.
        /// </summary>
        Task<bool> HealthCheckAsync();
    }
}
