using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models.IndexVM
{
    public class ActividadRapidaVM
    {
        public int? IdLabor { get; set; }
        public int? IdLote { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public List<int> LotesIds { get; set; } = new List<int>();

        //public List<SelectListItem>? Lotes { get; set; }
        public List<LoteVM>? Lotes { get; set; }

        public int TipoIdActividad { get; set; }

        public string? Observacion { get; set; }

        public decimal? Cantidad { get; set; }
        public int? IdInsumo { get; set; }

        public List<ActividadVM>? TiposActividadCompletos { get; set; }

        // Nuevas propiedades para manejar tipos específicos
        public string? TipoActividad { get; set; }
        public DatosEspecificosVM? DatosEspecificos { get; set; }

        // Configuración de unidades de medida (serializada como JSON)
        public string? UnidadesConfigJson { get; set; }

        // Gestión de CicloCultivo
        public int? IdCicloCultivo { get; set; }
        public List<CicloCultivoVM>? CiclosActivos { get; set; }
    }

    public class DatosEspecificosVM
    {
        public decimal? Costo { get; set; }
        public bool? EsDolar { get; set; }

        // Siembra
        public decimal? Superficie { get; set; }
        public int? IdUnidadSuperficie { get; set; }
        public decimal? Densidad { get; set; }
        public int? IdUnidadDensidad { get; set; }
        public int IdCultivo { get; set; }
        public int? IdMetodoSiembra { get; set; }

        // Riego
        public decimal? HorasRiego { get; set; }
        public decimal? VolumenAgua { get; set; }
        public int? IdUnidadVolumenAgua { get; set; }
        public int? IdMetodoRiego { get; set; }
        public int? IdFuenteAgua { get; set; }

        // Fertilización
        public decimal? Cantidad { get; set; }
        public int? IdUnidadCantidad { get; set; }
        public decimal? Dosis { get; set; }
        public int? IdUnidadDosis { get; set; }
        public int? IdNutriente { get; set; }
        public int? IdTipoFertilizante { get; set; }
        public int? IdMetodoAplicacion { get; set; }

        // Pulverización
        public decimal? Volumen { get; set; }
        public int? IdUnidadVolumen { get; set; }
        public string? CondicionesClimaticas { get; set; }
        public int? IdProductoAgroquimico { get; set; }

        // Monitoreo
        public int? IdTipoMonitoreo { get; set; }
        public int? IdEstadoFenologico { get; set; }
        public IdMonitoreoEnum? IdMonitoreo { get; set; }

        // Análisis de Suelo
        public decimal? ProfundidadCm { get; set; }
        public decimal? PH { get; set; }
        public decimal? MateriaOrganica { get; set; }
        public decimal? Nitrogeno { get; set; }
        public decimal? Fosforo { get; set; }
        public decimal? Potasio { get; set; }
        public decimal? ConductividadElectrica { get; set; }
        public decimal? CIC { get; set; }
        public string? Textura { get; set; }
        public int? IdLaboratorio { get; set; }

        // Cosecha
        public decimal? Rendimiento { get; set; }
        public int? IdUnidadRendimiento { get; set; }
        public decimal? HumedadGrano { get; set; }
        public decimal? SuperficieCosechada { get; set; }
        public int? IdUnidadSuperficieCosechada { get; set; }

        // Acopio
        public TipoAcopio? TipoAcopio { get; set; }
        public string? Codigo { get; set; }
        public DateTime? FechaIngreso { get; set; }
        [Required(ErrorMessage = "La Cantidad Actual (Tn) es obligatoria")]
        public decimal? CantidadActualTn { get; set; }
        public decimal? CapacidadTotalTn { get; set; }
        public string? Estado { get; set; }
        public string? Ubicacion { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? Diametro { get; set; }
        public DateTime? FechaEmbolsado { get; set; }
        public string? TipoSilo { get; set; }
        public bool? Aireacion { get; set; }
        public decimal? TemperaturaGrano { get; set; }
        public string? Empresa { get; set; }
        public string? NumeroContrato { get; set; }
        public decimal? TarifaAlmacenaje { get; set; }
    }
}
