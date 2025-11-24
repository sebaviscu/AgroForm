using AgroForm.Model.Actividades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Lote : EntityBaseWithLicencia, IEntityBaseWithCampania
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

        public decimal CostoTotalLaboresArs =>
             (Siembras.Any(_ => _.Costo != null) ? Siembras.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (Riegos.Any(_ => _.Costo != null) ? Riegos.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (Fertilizaciones.Any(_ => _.Costo != null) ? Fertilizaciones.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (Pulverizaciones.Any(_ => _.Costo != null) ? Pulverizaciones.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (Monitoreos.Any(_ => _.Costo != null) ? Monitoreos.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (AnalisisSuelos.Any(_ => _.Costo != null) ? AnalisisSuelos.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
             (Cosechas.Any(_ => _.Costo != null) ? Cosechas.Sum(x => x.CostoARS.GetValueOrDefault()) : 0) +
              (OtrasLabores.Any(_ => _.Costo != null) ? OtrasLabores.Sum(x => x.CostoARS.GetValueOrDefault()) : 0);

        public decimal CostoTotalLaboresUsd =>
             (Siembras.Any(_ => _.Costo != null) ? Siembras.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (Riegos.Any(_ => _.Costo != null) ? Riegos.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (Fertilizaciones.Any(_ => _.Costo != null) ? Fertilizaciones.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (Pulverizaciones.Any(_ => _.Costo != null) ? Pulverizaciones.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (Monitoreos.Any(_ => _.Costo != null) ? Monitoreos.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (AnalisisSuelos.Any(_ => _.Costo != null) ? AnalisisSuelos.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (Cosechas.Any(_ => _.Costo != null) ? Cosechas.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0) +
             (OtrasLabores.Any(_ => _.Costo != null) ? OtrasLabores.Sum(x => x.CostoUSD.GetValueOrDefault()) : 0);

    }
}
