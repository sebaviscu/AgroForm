using AgroForm.Model;
using AgroForm.Model.Actividades;

namespace AgroForm.Web.Models
{
    public class LoteVM : EntityBaseWithLicenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? SuperficieHectareas { get; set; }

        public int idCampo { get; set; }
        public CampoVM? Campo { get; set; }

        public int? IdCultivo => SiembraACosechar != null ? SiembraACosechar.IdCultivo : null;

        public List<SiembraVM> Siembras { get; set; } = new();
        public List<CosechaVM> Cosechas { get; set; } = new();

        public decimal SuperficieSembrada => SiembraACosechar != null ? SiembraACosechar.SuperficieHa.GetValueOrDefault() : 0;

        public bool PermiteSiembra
        {
            get
            {
                // Si no hay siembras ni cosechas, permitir sembrar
                if (Siembras.Count == 0 && Cosechas.Count == 0)
                    return true;

                // Si hay igual cantidad de siembras y cosechas, permitir sembrar
                if (Siembras.Count == Cosechas.Count)
                    return true;

                // Si hay más cosechas que siembras, no permitir sembrar (esto no debería pasar en flujo normal)
                if (Cosechas.Count > Siembras.Count)
                    return false;

                // Por defecto, no permitir sembrar si hay más siembras que cosechas
                return false;
            }
        }

        public bool PermiteCosechas
        {
            get
            {
                // Solo permitir cosechar si hay al menos una siembra y hay menos cosechas que siembras
                if (Siembras.Count > 0 && Cosechas.Count < Siembras.Count)
                    return true;

                return false;
            }
        }

        public string SiembraACosecharString
        {
            get
            {
                if (SiembraACosechar == null) return string.Empty;

                return $"{SiembraACosechar.Cultivo.Nombre.ToUpper()}";
            }
        }

        public SiembraVM? SiembraACosechar
        {
            get
            {
                if (!PermiteCosechas) return null;

                // La siembra a cosechar es la última siembra que no tiene cosecha
                // Ordenamos por fecha descendente y tomamos la primera que no tenga cosecha
                return Siembras
                    .OrderByDescending(s => s.RegistrationDate)
                    .Skip(Cosechas.Count)
                    .FirstOrDefault();
            }
        }

        public int idCampania { get; set; }

        public ICollection<LaborDTO> Actividades { get; set; } = new List<LaborDTO>();

    }
}
