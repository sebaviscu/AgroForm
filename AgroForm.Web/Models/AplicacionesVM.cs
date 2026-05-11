using AgroForm.Business.Contracts;

namespace AgroForm.Web.Models
{
    public class AplicacionesVM
    {
        /// <summary>
        /// Datos completos del reporte de aplicaciones agrícolas
        /// </summary>
        public AplicacionReporteDto Datos { get; set; } = new();

        // Filtros aplicados
        public int? FiltroIdCampania { get; set; }
        public int? FiltroIdCampo { get; set; }
        public int? FiltroIdLote { get; set; }
        public int? FiltroIdCultivo { get; set; }
        public int? FiltroIdTipoAplicacion { get; set; }
        public int? FiltroIdProducto { get; set; }
        public DateTime? FiltroFechaDesde { get; set; }
        public DateTime? FiltroFechaHasta { get; set; }

        // Datos para los filtros (selects)
        public List<FiltroItem> Campanias { get; set; } = new();
        public List<FiltroItem> Campos { get; set; } = new();
        public List<FiltroItem> Lotes { get; set; } = new();
        public List<FiltroItem> Cultivos { get; set; } = new();
        public List<FiltroItem> Productos { get; set; } = new();

        /// <summary>
        /// Moneda seleccionada: "ARS" o "USD"
        /// </summary>
        public string Moneda { get; set; } = "ARS";
    }
}
