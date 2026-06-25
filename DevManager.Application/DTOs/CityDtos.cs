namespace DevManager.Application.DTOs;

public record CityDto(Guid Id, string Name, Guid StateId, string? StateName);
public record CreateCityRequest(string Name, Guid StateId);
public record UpdateCityRequest(Guid Id, string Name, Guid StateId);
