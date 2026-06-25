using DevManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<State> States { get; }
    DbSet<City> Cities { get; }
    DbSet<ProgrammingLanguage> ProgrammingLanguages { get; }
    DbSet<Developer> Developers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
