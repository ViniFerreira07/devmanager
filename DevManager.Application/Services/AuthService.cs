using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;

namespace DevManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(IRepository<User> userRepository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher, IValidator<RegisterRequest> registerValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _registerValidator = registerValidator;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = (await _userRepository.ListAsync(cancellationToken)).FirstOrDefault(u => u.Email == request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Fail("Invalid credentials.");
        }

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.Name);
        return Result<LoginResponse>.Ok(new LoginResponse(token, user.Email, user.Name), "Login successful.");
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<RegisterResponse>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        var exists = (await _userRepository.ListAsync(cancellationToken)).Any(u => u.Email == request.Email);
        if (exists)
        {
            return Result<RegisterResponse>.Fail("Email already registered.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RegisterResponse>.Ok(new RegisterResponse(user.Email, user.Name), "User registered successfully.");
    }

}
