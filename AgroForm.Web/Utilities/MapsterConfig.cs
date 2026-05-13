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

            // Mapeos complejos con configuración personalizada (solo los necesarios)
            
            // Mapeo para Gasto -> GastoDto (requiere conversión de DateOnly a DateTime)
            TypeAdapterConfig<Gasto, GastoDto>.NewConfig()
                .Map(dest => dest.Fecha, src => src.Fecha.ToDateTime(TimeOnly.MinValue))
                .Map(dest => dest.Descripcion, src => src.TipoGasto.ToString())
                .Map(dest => dest.EsLabor, src => false);
                // Las propiedades Costo, CostoARS, CostoUSD se mapean automáticamente por nombre

            // Mapeo para LaborDTO -> GastoDto (requiere mapeo de propiedades con nombres diferentes)
            TypeAdapterConfig<LaborDTO, GastoDto>.NewConfig()
                .Map(dest => dest.Descripcion, src => src.TipoActividad)
                .Map(dest => dest.EsLabor, src => true);
                // Las demás propiedades se mapean automáticamente por nombre

            // Mapeo para Riego -> RiegoVM (mapea VolumenAgua a VolumenAguaM3 para la vista)
            TypeAdapterConfig<Riego, RiegoVM>.NewConfig()
                .Map(dest => dest.VolumenAguaM3, src => src.VolumenAgua)
                .Map(dest => dest.VolumenAgua, src => src.VolumenAgua)
                .Map(dest => dest.IdUnidadVolumenAgua, src => src.IdUnidadVolumenAgua);
            // Las propiedades HorasRiego, IdMetodoRiego, IdFuenteAgua se mapean automáticamente por nombre

            // Mapeo para Fertilizacion -> FertilizacionVM (mapea propiedades con nombres diferentes)
            TypeAdapterConfig<Fertilizacion, FertilizacionVM>.NewConfig()
                .Map(dest => dest.CantidadKgHa, src => src.Cantidad)
                .Map(dest => dest.Cantidad, src => src.Cantidad)
                .Map(dest => dest.IdUnidadCantidad, src => src.IdUnidadCantidad)
                .Map(dest => dest.DosisKgHa, src => src.Dosis)
                .Map(dest => dest.Dosis, src => src.Dosis)
                .Map(dest => dest.IdUnidadDosis, src => src.IdUnidadDosis);
            // Las propiedades IdNutriente, IdTipoFertilizante, IdMetodoAplicacion se mapean automáticamente por nombre

            // Los siguientes mapeos son redundantes y pueden ser eliminados:
            // - Variedad -> VariedadVM: Mapster mapea automáticamente las propiedades por nombre
            // - Cultivo -> CultivoVM: Mapster mapea automáticamente las colecciones por nombre
            // - Licencia -> LicenciaVM: Mapster mapea automáticamente las colecciones por nombre
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
