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

        public enum Monedas
        {
            Peso = 1,
            Dolar = 2
        }

        public enum EstadosCamapaña
        {
            Iniciada,
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

        public enum TipoActividadEnum
        {
            [Display(Name = "Analisis de suelo")]
            AnalisisSuelo = 1,
            Siembra = 2,
            Pulverizacion = 3,
            Fertilizado = 4,
            Riego = 5,
            Monitoreo = 6,
            Cosecha = 7, 
            [Display(Name = "Otras Labores")]
            OtrasLabores = 8
        }

        public enum TipoCatalogo
        {
            // Monitoreo
            Plaga = 10,
            Maleza = 11,
            Enfermedad = 12,

            // Fertilización y productos
            TipoFertilizante = 20,
            Nutriente = 21,
            ProductoAgroquimico = 22,

            // Actividades y operaciones
            MetodoSiembra = 30,
            MetodoRiego = 31,
            MetodoAplicacion = 32,

            // Infraestructura y personas
            Maquinaria = 40,
            FuenteAgua = 41,

            // Otros
            Laboratorio = 50,
            Otro = 99
        }

        public enum TipoVariedad
        {
            Variedad,
            Subproducto,
            Descarte
        }

        public enum IdMonitoreoEnum
        {
            Plaga = 10,
            Maleza = 11,
            Enfermedad = 12
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
