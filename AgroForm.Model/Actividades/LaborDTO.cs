using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Actividades
{
    public class LaborDTO
    {
        public int Id { get; set; }
        public int IdTipoActividad { get; set; }
        public int IdCampania { get; set; }
        public string Campania { get; set; } = string.Empty;
        public int IdLote { get; set; } 
        public string Lote { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public bool EsDolar { get; set; }

        public string TipoActividad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public string Responsable { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal? Costo { get; set; }
        public decimal? CostoARS { get; set; }
        public decimal? CostoUSD { get; set; }

        public string IconoTipoActividad { get; set; } = string.Empty;
        public string IconoColorTipoActividad { get; set; } = string.Empty;


        public string Observacion { get; set; } = string.Empty;
        public string ObservacionCortada
        {
            get
            {
                if (string.IsNullOrEmpty(Observacion))
                    return string.Empty;

                if (Observacion.Length <= 70)
                    return Observacion;

                return Observacion.Substring(0, 70) + "...";
            }
        }

        public string FechaRelativa
        {
            get
            {
                var ahora = TimeHelper.GetArgentinaTime();
                var diferencia = ahora - RegistrationDate.GetValueOrDefault();

                if (diferencia.TotalMinutes < 60)
                {
                    // Menos de 1 hora: mostrar minutos
                    var minutos = (int)diferencia.TotalMinutes;
                    if (minutos <= 1)
                        return "(Hace 1 minuto)";
                    else if (minutos < 5)
                        return "(Hace unos minutos)";
                    else
                        return $"(Hace {minutos} minutos)";
                }
                else if (diferencia.TotalHours < 24)
                {
                    // Menos de 1 día: mostrar horas
                    var horas = (int)diferencia.TotalHours;
                    return horas <= 1 ? "(Hace 1 hora)" : $"(Hace {horas} horas)";
                }
                else if (diferencia.TotalDays < 7)
                {
                    // Menos de 7 días: mostrar días
                    var dias = (int)diferencia.TotalDays;
                    return dias <= 1 ? "(Hace 1 día)" : $"(Hace {dias} días)";
                }
                else
                {
                    // Más de 7 días: mostrar fecha
                    return RegistrationDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                }
            }
        }
        public string FiltroTiempo
        {
            get
            {
                var ahora = TimeHelper.GetArgentinaTime();
                var diferencia = ahora - RegistrationDate.GetValueOrDefault();

                if (RegistrationDate.GetValueOrDefault().Date == ahora.Date) return "today";
                else if (diferencia.TotalDays < 7) return "week";
                else return "older";
            }
        }
    }
}
