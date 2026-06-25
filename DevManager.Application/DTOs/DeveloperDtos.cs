namespace DevManager.Application.DTOs;

public record DeveloperDto(
    Guid Id,
    string Name,
    string Email,
    string Seniority,
    Guid CityId,
    string? CityName,
    string? StateName,
    string? Observations,
    IReadOnlyList<string> ProgrammingLanguages);

public record CreateDeveloperRequest(
    string Name,
    string Email,
    string Seniority,
    Guid CityId,
    string? Observations,
    IReadOnlyList<Guid> ProgrammingLanguageIds);

public record UpdateDeveloperRequest(
    Guid Id,
    string Name,
    string Email,
    string Seniority,
    Guid CityId,
    string? Observations,
    IReadOnlyList<Guid> ProgrammingLanguageIds);

public record DeveloperFilterRequest(int Page = 1, int PageSize = 10, string? Name = null, Guid? CityId = null, Guid? LanguageId = null, string? Seniority = null);
