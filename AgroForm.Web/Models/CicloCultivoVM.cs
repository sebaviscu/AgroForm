using AgroForm.Model;
using AgroForm.Model.Actividades;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Models
{
    public class CicloCultivoVM : EntityBaseWithLicenciaVM
    {
        public int IdLote { get; set; }
        public LoteVM? Lote { get; set; }

        public int IdCultivo { get; set; }
        public CultivoVM Cultivo { get; set; } = null!;

        public int IdCampania { get; set; }

        public int? IdVariedad { get; set; }
        public VariedadVM? Variedad { get; set; }

        public EpocaSiembra? Epoca { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool EstaActivo => FechaFin == null;

        public string EpocaDisplay => Epoca.HasValue ? Epoca.Value.GetDisplayName() : "-";

        public string CultivoConEpoca => $"{Cultivo?.Nombre ?? "?"} ({EpocaDisplay})";

        // Flattened activity lists for UI convenience
        public List<SiembraVM> Siembras { get; set; } = new();
        public List<CosechaVM> Cosechas { get; set; } = new();
        public List<RiegoVM> Riegos { get; set; } = new();
        public List<FertilizacionVM> Fertilizaciones { get; set; } = new();
        public List<PulverizacionVM> Pulverizaciones { get; set; } = new();
        public List<MonitoreoVM> Monitoreos { get; set; } = new();
        public List<AnalisisSueloVM> AnalisisSuelos { get; set; } = new();
        public List<OtraLaborVM> OtrasLabores { get; set; } = new();
    }
}
