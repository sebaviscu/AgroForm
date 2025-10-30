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
            CreateMap<Actividad, ActividadVM>().ReverseMap();
            CreateMap<Campania, CampaniaVM>().ReverseMap();
            CreateMap<Campo, CampoVM>().ReverseMap();
            CreateMap<HistoricoPrecioInsumo, HistoricoPrecioInsumoVM>().ReverseMap();
            CreateMap<Insumo, InsumoVM>().ReverseMap();
            CreateMap<Licencia, LicenciaVM>().ReverseMap();
            CreateMap<Lote, LoteVM>().ReverseMap();
            CreateMap<Marca, MarcaVM>().ReverseMap();
            CreateMap<Moneda, MonedaVM>().ReverseMap();
            CreateMap<MovimientoInsumo, MovimientoInsumoVM>().ReverseMap();
            CreateMap<Proveedor, ProveedorVM>().ReverseMap();
            CreateMap<RegistroClima, RegistroClimaVM>().ReverseMap();
            CreateMap<TipoActividad, TipoActividadVM>().ReverseMap();
            CreateMap<TipoInsumo, TipoInsumoVM>().ReverseMap();
            CreateMap<Usuario, UsuarioVM>().ReverseMap();
            CreateMap<Ajuste, AjusteVM>().ReverseMap();

        }
    }
}