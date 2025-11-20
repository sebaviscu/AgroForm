using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models.IndexVM
{
    public class ActividadRapidaVM
    {
        public int? IdLabor { get; set; }
        public int? idLote { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public List<int> LotesIds { get; set; } = new List<int>();

        //public List<SelectListItem>? Lotes { get; set; }
        public List<LoteVM>? Lotes { get; set; }

        public int TipoidActividad { get; set; }

        public string? Observacion { get; set; }

        public decimal? Cantidad { get; set; }
        public int? idInsumo { get; set; }

        public List<ActividadVM>? TiposActividadCompletos { get; set; }

        // Nuevas propiedades para manejar tipos específicos
        public string? TipoActividad { get; set; }
        public DatosEspecificosVM? DatosEspecificos { get; set; }
    }

    public class DatosEspecificosVM
    {
        public decimal? Costo { get; set; }
        public bool? EsDolar { get; set; }

        // Siembra
        public decimal? SuperficieHa { get; set; }
        public decimal? DensidadSemillaKgHa { get; set; }
        public int IdCultivo { get; set; }
        public int? IdVariedad { get; set; }
        public int? IdMetodoSiembra { get; set; }

        // Riego
        public decimal? HorasRiego { get; set; }
        public decimal? VolumenAguaM3 { get; set; }
        public int? IdMetodoRiego { get; set; }
        public int? IdFuenteAgua { get; set; }

        // Fertilización
        public decimal? CantidadKgHa { get; set; }
        public decimal? DosisKgHa { get; set; }
        public int? IdNutriente { get; set; }
        public int? IdTipoFertilizante { get; set; }
        public int? IdMetodoAplicacion { get; set; }

        // Pulverización
        public decimal? VolumenLitrosHa { get; set; }
        public decimal? Dosis { get; set; }
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
        public decimal? RendimientoTonHa { get; set; }
        public decimal? HumedadGrano { get; set; }
        public decimal? SuperficieCosechadaHa { get; set; }
    }
}
