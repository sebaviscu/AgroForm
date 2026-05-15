using System.ComponentModel.DataAnnotations.Schema;

namespace AgroForm.Model.Satelital
{
    /// <summary>
    /// Log de auditoría para todas las consultas a Sentinel Hub.
    /// Permite:
    /// - Monitorear consumo de API (separando tiles de consultas analíticas)
    /// - Detectar patrones de uso anómalos
    /// - Estimar costos
    /// - Auditar qué lotes se consultaron y cuándo
    /// 
    /// ⚠️ Métricas separadas: tiles NO cuentan para el límite "caro"
    /// - TipoConsulta = 'TILE' → visualización de mapa (bajo valor, se cachea)
    /// - TipoConsulta = 'TIME_SERIES' → datos analíticos (alto valor)
    /// - TipoConsulta = 'CURRENT_INDEX' → snapshot del último índice
    /// </summary>
    [Table("LogsConsultasSatelitales")]
    public class LogConsultaSatelital
    {
        public long Id { get; set; }

        /// <summary>Lote consultado (puede ser null si es consulta global)</summary>
        public int? IdLote { get; set; }

        /// <summary>Cuándo se hizo la consulta</summary>
        public DateTime FechaConsulta { get; set; } = TimeHelper.GetArgentinaTime();

        /// <summary>Tipo de consulta: 'TILE', 'TIME_SERIES', 'CURRENT_INDEX'</summary>
        public string TipoConsulta { get; set; } = string.Empty;

        /// <summary>Índice solicitado: 'NDVI', 'NDWI', 'ALL'</summary>
        public string IndiceSolicitado { get; set; } = string.Empty;

        /// <summary>Fecha desde (para series temporales)</summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>Fecha hasta (para series temporales)</summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>Parámetros adicionales en JSON</summary>
        public string? Parametros { get; set; }

        /// <summary>Duración de la consulta en milisegundos</summary>
        public int? DuracionMs { get; set; }

        /// <summary>Si la consulta fue exitosa</summary>
        public bool Exitoso { get; set; }

        /// <summary>Mensaje de error si falló</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Costo estimado en USD de esta consulta</summary>
        public decimal? CostoEstimado { get; set; }

        /// <summary>Licencia que realizó la consulta</summary>
        public int? IdLicencia { get; set; }
    }
}
