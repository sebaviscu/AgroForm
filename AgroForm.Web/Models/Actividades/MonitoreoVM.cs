using AgroForm.Model;

namespace AgroForm.Web.Models
{
    public class MonitoreoVM : ActividadVM
    {
        public int? IdEstadoFenologico { get; set; }
        public EstadoFenologicoVM? EstadoFenologico { get; set; }

        public int IdTipoMonitoreo { get; set; }
        public CatalogoVM TipoMonitoreo { get; set; } = null!;

    }

}
