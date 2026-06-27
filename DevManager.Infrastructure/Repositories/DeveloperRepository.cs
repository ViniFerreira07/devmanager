using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Infrastructure.Repositories;

public class DeveloperRepository : Repository<Developer>, IDeveloperRepository
{
    public DeveloperRepository(DevManagerDbContext context) : base(context)
    {
    }

    public async Task<Developer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Guid excludeId, string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Id != excludeId && x.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<Developer>> GetPagedAsync(int page, int pageSize, string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Include(x => x.ProgrammingLanguages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var searchName = name.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(searchName));
        }

        if (cityId.HasValue)
        {
            query = query.Where(x => x.CityId == cityId.Value);
        }

        if (languageId.HasValue)
        {
            query = query.Where(x => x.ProgrammingLanguages.Any(p => p.Id == languageId.Value));
        }

        if (!string.IsNullOrWhiteSpace(seniority))
        {
            var searchSeniority = seniority.ToLower();
            query = query.Where(x => x.Seniority.ToLower().Contains(searchSeniority));
        }

        return await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string? name = null, Guid? cityId = null, Guid? languageId = null, string? seniority = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var searchName = name.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(searchName));
        }

        if (cityId.HasValue)
        {
            query = query.Where(x => x.CityId == cityId.Value);
        }

        if (languageId.HasValue)
        {
            query = query.Where(x => x.ProgrammingLanguages.Any(p => p.Id == languageId.Value));
        }

        if (!string.IsNullOrWhiteSpace(seniority))
        {
            var searchSeniority = seniority.ToLower();
            query = query.Where(x => x.Seniority.ToLower().Contains(searchSeniority));
        }

        return await query.CountAsync(cancellationToken);
    }
}