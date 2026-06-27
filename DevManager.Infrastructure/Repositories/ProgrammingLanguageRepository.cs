using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Infrastructure.Repositories;

public class ProgrammingLanguageRepository : Repository<ProgrammingLanguage>, IProgrammingLanguageRepository
{
    public ProgrammingLanguageRepository(DevManagerDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(Guid excludeId, string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Id != excludeId && x.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.DeveloperProgrammingLanguages.AnyAsync(x => x.ProgrammingLanguageId == id, cancellationToken);
    }
}