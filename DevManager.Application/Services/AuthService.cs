using DevManager.Application.Common;
using DevManager.Application.Common.Exceptions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using FluentValidation;

namespace DevManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher, IValidator<RegisterRequest> registerValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _registerValidator = registerValidator;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Fail("Usuário não encontrado.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Fail("Senha incorreta.");
        }

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email, user.Name);
        return Result<LoginResponse>.Ok(new LoginResponse(token, user.Email, user.Name), "Login realizado com sucesso.");
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<RegisterResponse>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        var exists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (exists)
        {
            throw new ConflictException("USER_EMAIL_ALREADY_EXISTS", "Já existe um usuário cadastrado com este e-mail.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RegisterResponse>.Ok(new RegisterResponse(user.Email, user.Name), "Usuário registrado com sucesso.");
    }
}