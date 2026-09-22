using AutoMapper;
using Core.Citas.Dtos;
using Domain.AgendaYAtencion;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Citas.Mappings
{
    public class CitaProfile : Profile
    {
        public CitaProfile()
        {
            CreateMap<Cita, GetCitaDto>()
                .ForMember(dest => dest.EstadoCitaDescripcion,
                           opt => opt.MapFrom(src => src.EstadoCita != null ? src.EstadoCita.Nombre : null));

            CreateMap<CreateCitaDto, Cita>()
                .ForMember(dest => dest.CitaID, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.EstadoCita, opt => opt.Ignore());
        }
    }
}