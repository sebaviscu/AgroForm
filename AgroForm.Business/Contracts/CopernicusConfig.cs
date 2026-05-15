namespace AgroForm.Business.Contracts
{
    /// <summary>
    /// Configuración de Copernicus Data Space Ecosystem bindeada desde appsettings.json
    /// Sección: "Copernicus"
    /// 
    /// Registro gratuito en: https://dataspace.copernicus.eu/
    /// Crear OAuth Client en: https://identity.dataspace.copernicus.eu/
    /// 
    /// APIs utilizadas:
    /// - Processing API: https://sh.dataspace.copernicus.eu/process/v1
    /// - STAC API: https://stac.dataspace.copernicus.eu/v1
    /// - Auth OIDC: https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token
    /// 
    /// NOTA: Copernicus NO usa instanceId. La autenticación es directa vía OAuth2 cliente.
    /// </summary>
    public class CopernicusConfig
    {
        /// <summary>
        /// OAuth Client ID (crear en Copernicus Data Space Dashboard)
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URL base de la Processing API de Copernicus (sin trailing slash)
        /// Default: https://sh.dataspace.copernicus.eu
        /// </summary>
        public string BaseUrl { get; set; } = "https://sh.dataspace.copernicus.eu";

        /// <summary>
        /// URL de autenticación OIDC (OpenID Connect) de Copernicus
        /// Default: https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token
        /// </summary>
        public string AuthUrl { get; set; } = "https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token";

        /// <summary>
        /// URL base de la STAC API de Copernicus (sin trailing slash)
        /// Default: https://stac.dataspace.copernicus.eu
        /// </summary>
        public string StacBaseUrl { get; set; } = "https://stac.dataspace.copernicus.eu";

        /// <summary>
        /// TTL en días para caché de tiles en disco. Default: 7
        /// </summary>
        public int TileCacheDias { get; set; } = 7;

        /// <summary>
        /// Ruta raíz para el caché de tiles en disco
        /// </summary>
        public string TileCacheRoot { get; set; } = "satelite-tiles";
    }
}
