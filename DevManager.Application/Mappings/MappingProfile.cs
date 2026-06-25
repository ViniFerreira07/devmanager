using AutoMapper;
using DevManager.Application.DTOs;
using DevManager.Domain.Entities;

namespace DevManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<State, StateDto>()
            .ConstructUsing(s => new StateDto(s.Id, s.Name, s.UF));
        CreateMap<CreateStateRequest, State>();
        CreateMap<UpdateStateRequest, State>();

        CreateMap<City, CityDto>()
            .ConstructUsing(s => new CityDto(s.Id, s.Name, s.StateId, s.State != null ? s.State.Name : null))
            .ForMember(d => d.StateName, opt => opt.MapFrom(s => s.State != null ? s.State.Name : null));
        CreateMap<CreateCityRequest, City>();
        CreateMap<UpdateCityRequest, City>();

        CreateMap<ProgrammingLanguage, ProgrammingLanguageDto>()
            .ConstructUsing(s => new ProgrammingLanguageDto(s.Id, s.Name, s.Type));
        CreateMap<CreateProgrammingLanguageRequest, ProgrammingLanguage>();
        CreateMap<UpdateProgrammingLanguageRequest, ProgrammingLanguage>();

        CreateMap<Developer, DeveloperDto>()
            .ConstructUsing(s => new DeveloperDto(
                s.Id,
                s.Name,
                s.Email,
                s.Seniority,
                s.CityId,
                s.City != null ? s.City.Name : null,
                s.City != null && s.City.State != null ? s.City.State.Name : null,
                s.Observations,
                s.ProgrammingLanguages.Select(x => x.Name).ToList()))
            .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.City != null ? s.City.Name : null))
            .ForMember(d => d.StateName, opt => opt.MapFrom(s => s.City != null && s.City.State != null ? s.City.State.Name : null))
            .ForMember(d => d.ProgrammingLanguages, opt => opt.MapFrom(s => s.ProgrammingLanguages.Select(x => x.Name).ToList()));
        CreateMap<CreateDeveloperRequest, Developer>();
        CreateMap<UpdateDeveloperRequest, Developer>();
    }
}
