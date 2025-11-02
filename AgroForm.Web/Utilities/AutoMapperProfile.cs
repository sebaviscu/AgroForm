using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgroForm.Web.Utilities
{
    public class AutoMapperProfile : Profile
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        public AutoMapperProfile()
        {
            CreateMap<ActividadVM, Actividad>();
            CreateMap<Campania, CampaniaVM>().ReverseMap();
            CreateMap<Campo, CampoVM>().ReverseMap();
            CreateMap<HistoricoPrecioInsumo, HistoricoPrecioInsumoVM>().ReverseMap();
            CreateMap<Insumo, InsumoVM>().ReverseMap();
            CreateMap<Licencia, LicenciaVM>().ReverseMap();
            CreateMap<Lote, LoteVM>().ReverseMap();
            CreateMap<Marca, MarcaVM>().ReverseMap();
            CreateMap<Moneda, MonedaVM>().ReverseMap();
            CreateMap<Proveedor, ProveedorVM>().ReverseMap();
            CreateMap<RegistroClima, RegistroClimaVM>().ReverseMap();
            CreateMap<TipoActividad, TipoActividadVM>().ReverseMap();
            CreateMap<TipoInsumo, TipoInsumoVM>().ReverseMap();
            CreateMap<Usuario, UsuarioVM>().ReverseMap();
            CreateMap<Ajuste, AjusteVM>().ReverseMap();


            CreateMap<Actividad, ActividadVM>()
                .ForMember(dest => dest.TipoActividad, opt => opt.MapFrom(src =>
                    src.TipoActividad != null ? src.TipoActividad.Nombre : string.Empty))
                .ForMember(dest => dest.IconoColorTipoActividad, opt => opt.MapFrom(src =>
                    src.TipoActividad != null ? src.TipoActividad.ColorIcono : string.Empty))
                .ForMember(dest => dest.IconoTipoActividad, opt => opt.MapFrom(src =>
                    src.TipoActividad != null ? src.TipoActividad.Icono : string.Empty))
                .ForMember(dest => dest.Responsable, opt => opt.MapFrom(src =>
                    src.RegistrationUser != null ? src.RegistrationUser : string.Empty))
                .ForMember(dest => dest.Lote, opt => opt.MapFrom(src =>
                    src.Lote != null ? src.Lote.Nombre : string.Empty))
                .ForMember(dest => dest.Campo, opt => opt.MapFrom(src =>
                    src.Lote != null && src.Lote.Campo != null ? src.Lote.Campo.Nombre : string.Empty))
                .ForMember(dest => dest.idCampo, opt => opt.MapFrom(src =>
                    src.Lote != null ? src.Lote.IdCampo : 0))
                // Datos del insumo
                .ForMember(dest => dest.Insumo, opt => opt.MapFrom(src =>
                    src.Insumo != null ? src.Insumo.Nombre : string.Empty))
                .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src =>
                    src.Insumo != null ? src.Insumo.UnidadMedida : string.Empty))
                .ForMember(dest => dest.CantidadInsumo, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.PrecioInsumo, opt => opt.MapFrom(src =>
                    src.Insumo != null ? src.Insumo.PrecioActual : 0))
                .ForMember(dest => dest.Costo, opt => opt.MapFrom(src =>
                    src.Cantidad.HasValue && src.Insumo != null && src.Insumo.PrecioActual.HasValue
                        ? src.Cantidad.Value * src.Insumo.PrecioActual.Value
                        : 0));
        }
    }
}