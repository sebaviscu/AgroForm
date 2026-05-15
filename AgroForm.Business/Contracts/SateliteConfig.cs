namespace AgroForm.Business.Contracts
{
    /// <summary>
    /// Configuración de Sentinel Hub bindeada desde appsettings.json
    /// Sección: "SentinelHub"
    /// </summary>
    public class SentinelHubConfig
    {
        /// <summary>
        /// Instance ID de la configuración en Sentinel Hub Dashboard
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Client ID para autenticación
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Client Secret para autenticación
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URL base de la API de Sentinel Hub (sin trailing slash)
        /// Default: https://services.sentinel-hub.com
        /// </summary>
        public string BaseUrl { get; set; } = "https://services.sentinel-hub.com";

        /// <summary>
        /// URL de autenticación OAuth
        /// Default: https://services.sentinel-hub.com/oauth/token
        /// </summary>
        public string AuthUrl { get; set; } = "https://services.sentinel-hub.com/oauth/token";

        /// <summary>
        /// Límite mensual de requests (depende del plan: free=30000)
        /// </summary>
        public int LimiteMensual { get; set; } = 30000;

        /// <summary>
        /// Umbral de advertencia (0.0 a 1.0). Default: 0.8 (80%)
        /// </summary>
        public double UmbralAdvertencia { get; set; } = 0.8;

        /// <summary>
        /// TTL en días para caché de tiles en disco. Default: 7
        /// </summary>
        public int TileCacheDias { get; set; } = 7;

        /// <summary>
        /// Ruta raíz para el caché de tiles en disco
        /// </summary>
        public string TileCacheRoot { get; set; } = "satelite-tiles";

        /// <summary>
        /// Resolución WMTS por defecto. Default: "10m" (Sentinel-2)
        /// </summary>
        public string Resolucion { get; set; } = "10m";
    }
}
