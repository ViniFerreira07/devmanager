using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Infrastructure.Repositories;

public class StateRepository : Repository<State>, IStateRepository
{
    public StateRepository(DevManagerDbContext context) : base(context)
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

    public async Task<bool> ExistsByUFAsync(string uf, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.UF.ToLower() == uf.ToLower(), cancellationToken);
    }
}