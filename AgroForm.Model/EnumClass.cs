using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class EnumClass
    {
        public enum Roles
        {
            Administrador,
            Operador,
            Tecnico
        }
        public enum EstadosCamapaña
        {
            [Display(Name = "En Curso")]
            EnCurso,
            [Display(Name = "Finalizada")]
            Finalizada,
            [Display(Name = "Cancelada")]
            Cancelada
        }

        public enum TipoClima
        {
            Lluvia,
            Granizo
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? enumValue.ToString();
        }
    }
}
