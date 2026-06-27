using DevManager.Domain.Enums;

namespace DevManager.Application.DTOs;

public record ProgrammingLanguageDto(Guid Id, string Name, ProgrammingLanguageType Type, string Color, string Icon);
public record CreateProgrammingLanguageRequest(string Name, ProgrammingLanguageType Type);
public record UpdateProgrammingLanguageRequest(Guid Id, string Name, ProgrammingLanguageType Type);