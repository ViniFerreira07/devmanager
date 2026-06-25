using DevManager.Application.Common;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateUserRequest> _validator;

    public UserService(IAppDbContext context, IPasswordHasher passwordHasher, IValidator<CreateUserRequest> validator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users.AsNoTracking().OrderBy(x => x.Name).Select(x => new UserDto(x.Id, x.Name, x.Email)).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<UserDto>>.Ok(users);
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return user is null ? Result<UserDto>.Fail("User not found.") : Result<UserDto>.Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<UserDto>.Fail(string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));
        }

        if (await _context.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
        {
            return Result<UserDto>.Fail("Email already registered.");
        }

        var user = new User { Name = request.Name, Email = request.Email, PasswordHash = _passwordHasher.HashPassword(request.Password) };
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<UserDto>.Ok(new UserDto(user.Id, user.Name, user.Email), "User created successfully.");
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.Fail("User not found.");
        }

        if (await _context.Users.AnyAsync(x => x.Id != id && x.Email == request.Email, cancellationToken))
        {
            return Result<UserDto>.Fail("Email already registered.");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<UserDto>.Ok(new UserDto(user.Id, user.Name, user.Email), "User updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return Result<bool>.Fail("User not found.");
        }

        user.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "User deleted successfully.");
    }
}
