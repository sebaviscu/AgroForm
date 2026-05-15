using System.ComponentModel.DataAnnotations.Schema;

namespace AgroForm.Model.Satelital
{
    /// <summary>
    /// Índice satelital calculado para un lote en una fecha específica.
    /// Almacena valores de NDVI, NDWI y otros índices agronómicos
    /// obtenidos desde Sentinel Hub (o futuras fuentes).
    /// 
    /// Esta tabla permite:
    /// - Consultar series temporales históricas sin llamar a la API externa
    /// - Comparar campañas actuales vs anteriores con datos reales
    /// - Alimentar alertas inteligentes basadas en cambios de vegetación
    /// - Servir datos al dashboard sin depender de disponibilidad de Sentinel Hub
    /// </summary>
    [Table("IndicesSatelitales")]
    public class IndiceSatelital
    {
        public int Id { get; set; }

        /// <summary>Lote al que pertenece este índice</summary>
        public int IdLote { get; set; }

        /// <summary>Licencia (multitenant). Puede ser null si se usan datos globales de referencia.</summary>
        public int? IdLicencia { get; set; }

        /// <summary>Fecha de captura de la imagen satelital</summary>
        public DateTime FechaCaptura { get; set; }

        /// <summary>Cuándo se consultó/procesó este dato</summary>
        public DateTime FechaConsulta { get; set; } = TimeHelper.GetArgentinaTime();

        /// <summary>Fuente del dato: Sentinel-2, Landsat-9, etc.</summary>
        public string Fuente { get; set; } = "Sentinel-2";

        /// <summary>Resolución en metros (Sentinel-2 = 10m)</summary>
        public int ResolucionMts { get; set; } = 10;

        /// <summary>Porcentaje de cobertura de nubes (0.00 a 100.00)</summary>
        public decimal? CloudCover { get; set; }

        /// <summary>NDVI: Normalized Difference Vegetation Index (-1.000 a 1.000)</summary>
        public decimal? NDVI { get; set; }

        /// <summary>NDWI: Normalized Difference Water Index (-1.000 a 1.000)</summary>
        public decimal? NDWI { get; set; }

        /// <summary>EVI: Enhanced Vegetation Index (opcional, FASE 4)</summary>
        public decimal? EVI { get; set; }

        /// <summary>NDRE: Normalized Difference Red Edge (opcional, FASE 4)</summary>
        public decimal? NDRE { get; set; }

        /// <summary>SAVI: Soil-Adjusted Vegetation Index (opcional, FASE 4)</summary>
        public decimal? SAVI { get; set; }

        /// <summary>GNDVI: Green Normalized Difference Vegetation Index (opcional, FASE 4)</summary>
        public decimal? GNDVI { get; set; }

        /// <summary>False si cloud cover supera el umbral configurado</summary>
        public bool EsValido { get; set; } = true;

        /// <summary>Campaña asociada (opcional, para filtrar por campaña)</summary>
        public int? IdCampania { get; set; }
    }
}
