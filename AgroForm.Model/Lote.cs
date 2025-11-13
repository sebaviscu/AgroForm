using AgroForm.Model.Actividades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Lote : EntityBaseWithLicencia
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int IdCampo { get; set; }
        public Campo Campo { get; set; } = null!;

        public int IdCampania { get; set; }
        public Campania Campania { get; set; } = null!;

        //public List<RegistroClima> RegistrosClima { get; set; } = new List<RegistroClima>();
               
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
