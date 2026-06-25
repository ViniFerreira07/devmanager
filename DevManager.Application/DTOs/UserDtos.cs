namespace DevManager.Application.DTOs;

public record UserDto(Guid Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email, string Password);
public record UpdateUserRequest(Guid Id, string Name, string Email, string? Password);
