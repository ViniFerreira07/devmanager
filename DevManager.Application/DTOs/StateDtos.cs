namespace DevManager.Application.DTOs;

public record StateDto(Guid Id, string Name, string UF);
public record CreateStateRequest(string Name, string UF);
public record UpdateStateRequest(Guid Id, string Name, string UF);
