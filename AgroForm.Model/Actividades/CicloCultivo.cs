using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Model.Actividades
{
    public class CicloCultivo : EntityBaseWithLicencia
    {
        public int IdLote { get; set; }
        public Lote Lote { get; set; } = null!;

        public int IdCultivo { get; set; }
        public Cultivo Cultivo { get; set; } = null!;

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        public int? IdVariedad { get; set; }
        public Variedad? Variedad { get; set; }

        public EpocaSiembra? Epoca { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public List<Siembra> Siembras { get; set; } = new();
        public List<Riego> Riegos { get; set; } = new();
        public List<Fertilizacion> Fertilizaciones { get; set; } = new();
        public List<Pulverizacion> Pulverizaciones { get; set; } = new();
        public List<Monitoreo> Monitoreos { get; set; } = new();
        public List<AnalisisSuelo> AnalisisSuelos { get; set; } = new();
        public List<Cosecha> Cosechas { get; set; } = new();
        public List<OtraLabor> OtrasLabores { get; set; } = new();
    }
}
