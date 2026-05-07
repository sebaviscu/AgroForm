using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using Mapster;

namespace AgroForm.Web.Utilities
{
    public static class MapsterConfig
    {
        public static DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        public static void Configure()
        {
            // Configuración global de Mapster
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.MaxDepth(3);

            // Mapeos básicos - Mapster usa configuración implícita bidireccional
            // No necesita TwoWay() como AutoMapper, los mapeos son implícitos
            
            // Mapeos complejos con configuración personalizada
            TypeAdapterConfig<Gasto, GastoDto>.NewConfig()
                .Map(dest => dest.Fecha, src => src.Fecha.ToDateTime(TimeOnly.MinValue))
                .Map(dest => dest.Descripcion, src => src.TipoGasto.ToString())
                .Map(dest => dest.Costo, src => src.Costo)
                .Map(dest => dest.CostoARS, src => src.CostoARS)
                .Map(dest => dest.CostoUSD, src => src.CostoUSD);

            TypeAdapterConfig<LaborDTO, GastoDto>.NewConfig()
                .Map(dest => dest.Descripcion, src => src.TipoActividad)
                .Map(dest => dest.Fecha, src => src.Fecha)
                .Map(dest => dest.Costo, src => src.Costo)
                .Map(dest => dest.CostoARS, src => src.CostoARS)
                .Map(dest => dest.CostoUSD, src => src.CostoUSD);
        }

        // Métodos de extensión para facilitar el uso
        public static TDestination AdaptTo<TDestination>(this object source)
        {
            return source.Adapt<TDestination>();
        }

        public static TDestination AdaptTo<TSource, TDestination>(this TSource source)
        {
            return source.Adapt<TDestination>();
        }
    }
}
