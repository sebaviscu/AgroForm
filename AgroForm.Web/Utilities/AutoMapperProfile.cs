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
            //// Mapeo de Actividad a ActividadVM y viceversa
            //CreateMap<Actividad, ActividadVM>().ReverseMap();

            //// Mapeo de Campo a SelectListItem (para el dropdown)
            //CreateMap<Campo, SelectListItem>()
            //    .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()))
            //    .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Nombre))
            //    .ForMember(dest => dest.Selected, opt => opt.Ignore());

            //// Mapeo para listas
            //CreateMap<List<Actividad>, List<ActividadVM>>()
            //    .ConvertUsing(src => src.Select(a => Map<Actividad, ActividadVM>(a)).ToList());

            //CreateMap<List<Campo>, List<SelectListItem>>()
            //    .ConvertUsing(src => src.Select(c => new SelectListItem
            //    {
            //        Value = c.Id.ToString(),
            //        Text = c.Nombre
            //    }).ToList());

            //// Mapeos adicionales para otras entidades del sistema
            //CreateMap<Usuario, UserAuth>()
            //    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.NombreUsuario))
            //    .ForMember(dest => dest.IdUsuario, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.IdRol, opt => opt.MapFrom(src => src.RolId))
            //    .ForMember(dest => dest.IdLicencia, opt => opt.MapFrom(src => src.LicenciaId))
            //    .ForMember(dest => dest.Result, opt => opt.Ignore());

            //// Mapeo para crear actividades
            //CreateMap<ActividadVM, Actividad>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.Campo, opt => opt.Ignore())
            //    .ForMember(dest => dest.CampoId, opt => opt.MapFrom(src => src.CampoId))
            //    .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.Now))
            //    .ForMember(dest => dest.UsuarioCreacion, opt => opt.MapFrom((src, dest, destMember, context) =>
            //        context.Items["CurrentUser"] as string ?? "Sistema"));

            //// Mapeo para actualizar actividades
            //CreateMap<ActividadVM, Actividad>()
            //    .ForMember(dest => dest.Campo, opt => opt.Ignore())
            //    .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            //    .ForMember(dest => dest.UsuarioCreacion, opt => opt.Ignore())
            //    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
            //        srcMember != null && !srcMember.Equals(GetDefaultValue(srcMember.GetType()))));

            //// Mapeo para reportes de actividades
            //CreateMap<Actividad, ActividadReporteVM>()
            //    .ForMember(dest => dest.CampoNombre, opt => opt.MapFrom(src => src.Campo.Nombre))
            //    .ForMember(dest => dest.Ubicacion, opt => opt.MapFrom(src => src.Campo.Ubicacion))
            //    .ForMember(dest => dest.Hectareas, opt => opt.MapFrom(src => src.Campo.Hectareas))
            //    .ForMember(dest => dest.DuracionHoras, opt => opt.MapFrom(src =>
            //        src.Duracion.HasValue ? Math.Round(src.Duracion.Value.TotalHours, 2) : 0))
            //    .ForMember(dest => dest.Mes, opt => opt.MapFrom(src => src.Fecha.ToString("MMMM yyyy")))
            //    .ForMember(dest => dest.Trimestre, opt => opt.MapFrom(src =>
            //        $"T{(src.Fecha.Month - 1) / 3 + 1} {src.Fecha.Year}"));
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}