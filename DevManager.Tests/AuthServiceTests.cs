using DevManager.Application.Interfaces;
using DevManager.Application.DTOs;
using DevManager.Application.Services;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DevManager.Tests;

public class AuthServiceTests
{
    private static AuthService CreateService(DevManagerDbContext context, Mock<IPasswordHasher>? hasherMock = null)
    {
        var passwordHasher = hasherMock?.Object ?? new MockPasswordHasher();
        return new AuthService(
            new MockUserRepository(context),
            new MockUnitOfWork(context),
            new MockJwtTokenService(),
            passwordHasher,
            new MockValidator<RegisterRequest>()
        );
    }

    private static DevManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevManagerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DevManagerDbContext(options);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenEmailNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest("notfound@test.com", "password"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Usuário não encontrado");
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "wronghash",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.VerifyPassword("password", "wronghash")).Returns(false);
        
        var service = CreateService(context, hasherMock);

        var result = await service.LoginAsync(new LoginRequest("test@test.com", "password"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Senha incorreta");
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        await using var context = CreateContext();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "existing@test.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<Application.Common.Exceptions.ConflictException>(() =>
            service.RegisterAsync(new RegisterRequest("New User", "existing@test.com", "Password123")));
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword_WhenRegistering()
    {
        await using var context = CreateContext();
        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.HashPassword("Password123")).Returns("hashedpassword");
        
        var service = CreateService(context, hasherMock);

        var result = await service.RegisterAsync(new RegisterRequest("Test User", "test@test.com", "Password123"));

        result.Success.Should().BeTrue();
        hasherMock.Verify(h => h.HashPassword("Password123"), Times.Once);
    }

    private class MockUserRepository : IUserRepository
    {
        private readonly DevManagerDbContext _context;
        public MockUserRepository(DevManagerDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct)!;

        public Task<IReadOnlyList<User>> ListAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<User>>(_context.Users.ToList());

        public async Task<User> AddAsync(User entity, CancellationToken ct = default)
        {
            var result = await _context.Users.AddAsync(entity, ct);
            return result.Entity;
        }

        public Task UpdateAsync(User entity, CancellationToken ct = default)
        {
            _context.Users.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User entity, CancellationToken ct = default)
        {
            _context.Users.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct)!;

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
            _context.Users.AnyAsync(u => u.Email == email, ct);
    }

    private class MockUnitOfWork : IUnitOfWork
    {
        private readonly DevManagerDbContext _context;
        public MockUnitOfWork(DevManagerDbContext context) => _context = context;
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
        public void Dispose() => _context.Dispose();
    }

    private class MockJwtTokenService : IJwtTokenService
    {
        public string GenerateToken(Guid userId, string email, string name) => "mock-token";
    }

    private class MockValidator<T> : AbstractValidator<T>
    {
    }

    private class MockPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        public bool VerifyPassword(string password, string hash) => hash == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}