using AgroForm.Model;
using AgroForm.Model.Actividades;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models.IndexVM
{
    public class HomeIndexVM
    {
        public string CotizacionDolar { get; set; } = string.Empty;
        public string NombreCorizacion { get; set; } = string.Empty;
        public string CotizacionFecha { get; set; } = string.Empty;
        public string HaSembradas { get; set; } = string.Empty;
        public string Gastos { get; set; } = string.Empty;

        public List<SiembraVM> Cultivos { get; set; } = new()
        {
            new SiembraVM { CultivoNombre = "Soja", SuperficieHa = 120 },
            new SiembraVM { CultivoNombre = "Maíz", SuperficieHa = 80 },
            new SiembraVM { CultivoNombre = "Trigo", SuperficieHa = 60 },
            new SiembraVM { CultivoNombre = "Girasol", SuperficieHa = 40 }
        };

        public List<GastoVM> DistribucionGastos { get; set; } = new();

        // --- Acopio KPI ---
        public string TotalAcopioTn { get; set; } = "-";
        public List<AcopioResumenVM> AcopiosDetalle { get; set; } = new();

        public void CargarDistribucionGastos(List<Gasto> gastos)
        {
            var gastosAgrupados = gastos
                .Where(g => g.CostoARS.HasValue)
                .GroupBy(g => g.TipoGasto)
                .Select(grupo => new GastoVM
                {
                    TipoGasto = grupo.Key,
                    Costo = grupo.Sum(g => g.CostoARS ?? 0)
                })
                .OrderByDescending(g => g.Costo)
                .ToList();

            DistribucionGastos = gastosAgrupados;
            //Gastos = DistribucionGastos.Any() ? DistribucionGastos.Sum(g => g.Costo).Value.ToString("N0") : "-";
            Gastos = "-";
        }

        public void CargarCultivosDesdeSiembras(List<Siembra> siembras)
        {
            var cultivosAgrupados = siembras
                .Where(s => s.Superficie.HasValue && s.Superficie > 0)
                .GroupBy(s => s.Cultivo.Nombre)
                .Select(grupo => new SiembraVM
                {
                    CultivoNombre = grupo.Key,
                    SuperficieHa = grupo.Sum(s => s.Superficie ?? 0)
                })
                .OrderByDescending(s => s.SuperficieHa)
                .ToList();

            Cultivos = cultivosAgrupados;
            HaSembradas = Cultivos.Any() ? Cultivos.Sum(s => s.SuperficieHa).ToString() : "-";
        }

        public void CargarAcopiosDesdeDatos(List<Acopio> acopios)
        {
            var agrupados = acopios
                .Where(a => a.CantidadActualTn.HasValue && a.CantidadActualTn > 0)
                .GroupBy(a => new { a.TipoAcopio, CultivoNombre = a.Cultivo?.Nombre ?? "Sin cultivo" })
                .Select(grupo => new AcopioResumenVM
                {
                    TipoAcopio = grupo.Key.TipoAcopio,
                    TipoAcopioNombre = grupo.Key.TipoAcopio.GetDisplayName(),
                    CultivoNombre = grupo.Key.CultivoNombre,
                    CantidadTotalTn = grupo.Sum(a => a.CantidadActualTn ?? 0)
                })
                .OrderByDescending(a => a.CantidadTotalTn)
                .ToList();

            AcopiosDetalle = agrupados;
            TotalAcopioTn = AcopiosDetalle.Any()
                ? AcopiosDetalle.Sum(a => a.CantidadTotalTn).ToString("N1")
                : "-";
        }
    }

    public class AcopioResumenVM
    {
        public TipoAcopio TipoAcopio { get; set; }
        public string TipoAcopioNombre { get; set; } = string.Empty;
        public string CultivoNombre { get; set; } = string.Empty;
        public decimal CantidadTotalTn { get; set; }
    }
}
