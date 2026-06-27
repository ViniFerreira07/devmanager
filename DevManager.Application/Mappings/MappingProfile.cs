using AutoMapper;
using DevManager.Application.DTOs;
using DevManager.Domain.Entities;
using DevManager.Domain.Enums;

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
            .ConstructUsing(s => new ProgrammingLanguageDto(
                s.Id,
                s.Name,
                s.Type,
                GetLanguageColor(s.Type),
                GetLanguageIcon(s.Type)));
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

    private static string GetLanguageColor(ProgrammingLanguageType type) => type switch
    {
        ProgrammingLanguageType.Backend => "#3B82F6",
        ProgrammingLanguageType.Frontend => "#10B981",
        ProgrammingLanguageType.Mobile => "#8B5CF6",
        ProgrammingLanguageType.Database => "#F59E0B",
        ProgrammingLanguageType.Cloud => "#06B6D4",
        ProgrammingLanguageType.DevOps => "#6B7280",
        ProgrammingLanguageType.Game => "#EF4444",
        _ => "#6B7280"
    };

    private static string GetLanguageIcon(ProgrammingLanguageType type) => type switch
    {
        ProgrammingLanguageType.Backend => "server",
        ProgrammingLanguageType.Frontend => "code",
        ProgrammingLanguageType.Mobile => "smartphone",
        ProgrammingLanguageType.Database => "database",
        ProgrammingLanguageType.Cloud => "cloud",
        ProgrammingLanguageType.DevOps => "settings",
        ProgrammingLanguageType.Game => "gamepad-2",
        _ => "code"
    };
}