using AgroForm.Model;
using AgroForm.Model.Actividades;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models.IndexVM
{
    public class HomeIndexVM
    {
        public string CotizacionDolar { get; set; } = "1450";
        public string CotizacionFecha { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public string HaSembradas { get; set; } = "300";
        public string Gastos { get; set; } = "100,000";

        public List<SiembraVM> Cultivos { get; set; } = new()
        {
            new SiembraVM { CultivoNombre = "Soja", SuperficieHa = 120 },
            new SiembraVM { CultivoNombre = "Maíz", SuperficieHa = 80 },
            new SiembraVM { CultivoNombre = "Trigo", SuperficieHa = 60 },
            new SiembraVM { CultivoNombre = "Girasol", SuperficieHa = 40 }
        };

        public List<GastoVM> DistribucionGastos { get; set; } = new()
        {
            new GastoVM { TipoGasto = TipoGastoEnum.Sueldo, Costo = 45000 },
            new GastoVM { TipoGasto = TipoGastoEnum.Impuestos, Costo = 28000 },
            new GastoVM { TipoGasto = TipoGastoEnum.Mantenimiento, Costo = 15000 },
            new GastoVM { TipoGasto = TipoGastoEnum.Combustible, Costo = 12000 }
        };

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
            Gastos = DistribucionGastos.Any() ? DistribucionGastos.Sum(g => g.Costo).ToString() : "-";
        }

        public void CargarCultivosDesdeSiembras(List<Siembra> siembras)
        {
            var cultivosAgrupados = siembras
                .Where(s => s.SuperficieHa.HasValue && s.SuperficieHa > 0)
                .GroupBy(s => s.Cultivo.Nombre)
                .Select(grupo => new SiembraVM
                {
                    CultivoNombre = grupo.Key,
                    SuperficieHa = grupo.Sum(s => s.SuperficieHa ?? 0)
                })
                .OrderByDescending(s => s.SuperficieHa)
                .ToList();

            Cultivos = cultivosAgrupados;
            HaSembradas = Cultivos.Any() ? Cultivos.Sum(s => s.SuperficieHa).ToString() : "-";
        }
    }
}
