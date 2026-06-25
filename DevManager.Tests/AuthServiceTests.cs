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
            new MockRepository<User>(context),
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
        result.Message.Should().Contain("Invalid credentials");
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
        result.Message.Should().Contain("Invalid credentials");
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

        var result = await service.RegisterAsync(new RegisterRequest("New User", "existing@test.com", "Password123"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email already registered");
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

    private class MockRepository<T> : IRepository<T> where T : class
    {
        private readonly DevManagerDbContext _context;
        public MockRepository(DevManagerDbContext context) => _context = context;
        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<T?>(_context.Set<T>().FindAsync([id], ct).GetAwaiter().GetResult());
        public Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<T>>(_context.Set<T>().ToListAsync(ct).GetAwaiter().GetResult());
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            var result = await _context.Set<T>().AddAsync(entity, ct);
            return result.Entity;
        }
        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
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