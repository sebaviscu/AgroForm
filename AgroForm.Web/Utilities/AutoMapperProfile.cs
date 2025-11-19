using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AutoMapper;

namespace AgroForm.Web.Utilities
{
    public class AutoMapperProfile : Profile
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        public AutoMapperProfile()
        {
            CreateMap<EstadoFenologicoVM, EstadoFenologico>().ReverseMap();
            CreateMap<VariedadVM, Variedad>().ReverseMap();
            CreateMap<CultivoVM, Cultivo>().ReverseMap();
            CreateMap<CatalogoVM, Catalogo>().ReverseMap();
            CreateMap<PulverizacionVM, Pulverizacion>().ReverseMap();
            CreateMap<OtraLaborVM, OtraLabor>().ReverseMap();
            CreateMap<AnalisisSueloVM, AnalisisSuelo>().ReverseMap();
            CreateMap<CosechaVM, Cosecha>().ReverseMap();
            CreateMap<MonitoreoVM, Monitoreo>().ReverseMap();
            CreateMap<FertilizacionVM, Fertilizacion>().ReverseMap();
            CreateMap<RiegoVM, Riego>().ReverseMap();
            CreateMap<SiembraVM, Siembra>().ReverseMap();
            
            CreateMap<Campania, CampaniaVM>().ReverseMap();
            CreateMap<Campo, CampoVM>().ReverseMap();
            CreateMap<Licencia, LicenciaVM>().ReverseMap();
            CreateMap<Lote, LoteVM>().ReverseMap();
            CreateMap<Moneda, MonedaVM>().ReverseMap();
            CreateMap<RegistroClima, RegistroClimaVM>().ReverseMap();
            CreateMap<TipoActividad, TipoActividadVM>().ReverseMap();
            CreateMap<Usuario, UsuarioVM>().ReverseMap();
            CreateMap<Ajuste, AjusteVM>().ReverseMap();
            CreateMap<PagoLicencia, PagoLicenciaVM>().ReverseMap();
            CreateMap<Gasto, GastoVM>().ReverseMap();

            CreateMap<Gasto, GastoDto>()
                  .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha.ToDateTime(TimeOnly.MinValue)))
                  .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.TipoGasto.ToString()))
                  .ForMember(dest => dest.Costo, opt => opt.MapFrom(src => src.Costo))
                  .ForMember(dest => dest.CostoARS, opt => opt.MapFrom(src => src.CostoARS))
                  .ForMember(dest => dest.CostoUSD, opt => opt.MapFrom(src => src.CostoUSD));

            CreateMap<LaborDTO, GastoDto>()
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.TipoActividad))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.Costo, opt => opt.MapFrom(src => src.Costo))
                .ForMember(dest => dest.CostoARS, opt => opt.MapFrom(src => src.CostoARS))
                .ForMember(dest => dest.CostoUSD, opt => opt.MapFrom(src => src.CostoUSD));

        }
    }
}