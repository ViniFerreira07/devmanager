using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Infrastructure.Repositories;

public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(DevManagerDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAndStateAsync(string name, Guid stateId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.StateId == stateId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndStateAsync(Guid excludeId, string name, Guid stateId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Id != excludeId && x.Name.ToLower() == name.ToLower() && x.StateId == stateId, cancellationToken);
    }
}