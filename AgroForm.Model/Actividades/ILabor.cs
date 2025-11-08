using AgroForm.Model.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public interface ILabor
    {
        public int Id { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

        public DateTime Fecha { get; set; }
        public string Observacion { get; set; }

        public int IdLote { get; set; }
        public Lote? Lote { get; set; }

        public int IdTipoActividad { get; set; }
        public TipoActividad? TipoActividad { get; set; }

        public int IdUsuario { get; set; }
        public int IdCampania { get; set; }
        public int IdLicencia { get; set; }
    }
}
