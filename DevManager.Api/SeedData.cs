using DevManager.Application.Interfaces;
using DevManager.Domain.Entities;
using DevManager.Domain.Enums;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Api;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DevManagerDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (!context.States.Any())
        {
            context.States.AddRange(
                new State { Id = Guid.NewGuid(), Name = "Sao Paulo", UF = "SP", CreatedAt = DateTime.UtcNow },
                new State { Id = Guid.NewGuid(), Name = "Rio de Janeiro", UF = "RJ", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        if (!context.Cities.Any())
        {
            var sp = await context.States.FirstAsync(x => x.UF == "SP");
            var rj = await context.States.FirstAsync(x => x.UF == "RJ");

            context.Cities.AddRange(
                new City { Id = Guid.NewGuid(), Name = "Sao Paulo", StateId = sp.Id, CreatedAt = DateTime.UtcNow },
                new City { Id = Guid.NewGuid(), Name = "Campinas", StateId = sp.Id, CreatedAt = DateTime.UtcNow },
                new City { Id = Guid.NewGuid(), Name = "Rio de Janeiro", StateId = rj.Id, CreatedAt = DateTime.UtcNow });
        }

        if (!context.ProgrammingLanguages.Any())
        {
            context.ProgrammingLanguages.AddRange(
                new ProgrammingLanguage { Id = Guid.NewGuid(), Name = "React", Type = ProgrammingLanguageType.Frontend, CreatedAt = DateTime.UtcNow },
                new ProgrammingLanguage { Id = Guid.NewGuid(), Name = "ASP.NET Core", Type = ProgrammingLanguageType.Backend, CreatedAt = DateTime.UtcNow },
                new ProgrammingLanguage { Id = Guid.NewGuid(), Name = "Flutter", Type = ProgrammingLanguageType.Mobile, CreatedAt = DateTime.UtcNow },
                new ProgrammingLanguage { Id = Guid.NewGuid(), Name = "PostgreSQL", Type = ProgrammingLanguageType.Database, CreatedAt = DateTime.UtcNow },
                new ProgrammingLanguage { Id = Guid.NewGuid(), Name = "Docker", Type = ProgrammingLanguageType.DevOps, CreatedAt = DateTime.UtcNow });
        }

        if (!context.Users.Any(x => x.Email == "admin@devmanager.com"))
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Email = "admin@devmanager.com",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
