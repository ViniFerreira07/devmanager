namespace DevManager.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string Name);
public record RegisterRequest(string Name, string Email, string Password);
public record RegisterResponse(string Email, string Name);
