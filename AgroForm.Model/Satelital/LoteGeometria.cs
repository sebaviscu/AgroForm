using System.ComponentModel.DataAnnotations.Schema;

namespace AgroForm.Model.Satelital
{
    /// <summary>
    /// Geometría optimizada de lotes para consultas satelitales.
    /// Almacena tanto el GeoJSON original como la versión simplificada
    /// (Douglas-Peucker) para reducir payload y costo de procesamiento en Sentinel Hub.
    /// 
    /// MVP: tabla creada, precarga manual de geometrías.
    /// FASE 2: simplificación automática con NetTopologySuite.
    /// </summary>
    [Table("LotesGeometria")]
    public class LoteGeometria
    {
        public int Id { get; set; }

        /// <summary>Lote al que pertenece esta geometría</summary>
        public int IdLote { get; set; }

        /// <summary>GeoJSON original (sin simplificar)</summary>
        public string GeometriaOriginal { get; set; } = string.Empty;

        /// <summary>GeoJSON simplificado (Douglas-Peucker)</summary>
        public string GeometriaSimplificada { get; set; } = string.Empty;

        /// <summary>Tolerancia usada en la simplificación (grados decimales)</summary>
        public decimal? ToleranciaSimplificacion { get; set; }

        /// <summary>Área calculada desde la geometría (hectáreas)</summary>
        public decimal? AreaHa { get; set; }

        /// <summary>Latitud del centroide</summary>
        public decimal? CentroLat { get; set; }

        /// <summary>Longitud del centroide</summary>
        public decimal? CentroLng { get; set; }

        /// <summary>Bounding box en JSON</summary>
        public string? BoundsJson { get; set; }

        /// <summary>Cuándo se calculó esta geometría</summary>
        public DateTime FechaCalculo { get; set; } = TimeHelper.GetArgentinaTime();
    }
}
